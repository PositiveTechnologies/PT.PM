using Newtonsoft.Json;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.SourceRepository;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Json;
using PT.PM.Common.Nodes;
using PT.PM.Common.Utils;
using PT.PM.JavaScriptParseTreeUst;
using PT.PM.Matching;
using PT.PM.Matching.Json;
using PT.PM.Matching.PatternsRepository;
using PT.PM.PhpParseTreeUst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using PT.PM.Common.Files;
using PT.PM.Common.MessagePack;

namespace PT.PM
{
    public abstract class WorkflowBase<TStage, TWorkflowResult, TPattern, TRenderStage> : ILoggable
        where TStage : Enum
        where TWorkflowResult : WorkflowResultBase<TStage, TPattern, TRenderStage>
        where TRenderStage : Enum
    {
        private int currentFileId;

        protected ILogger logger = DummyLogger.Instance;

        static WorkflowBase()
        {
            Utils.RegisterAllLexersParsersAndConverters();
        }

        public TStage Stage { get; set; }

        public SourceRepository SourceRepository { get; set; }

        public IPatternsRepository PatternsRepository { get; set; }

        public IPatternConverter<TPattern> PatternConverter { get; set; }

        public LanguageDetector LanguageDetector { get; } = new LanguageDetector();

        public bool IsDumpJsonOutput { get; set; }

        public bool IsFoldConstants { get; set; } = true;

        public bool IsIgnoreFilenameWildcards { get; set; }

        public JavaScriptType JavaScriptType { get; set; } = JavaScriptType.Undefined;

        public ILogger Logger
        {
            get => logger;
            set
            {
                logger = value;
                if (SourceRepository != null)
                {
                    SourceRepository.Logger = Logger;
                }
                if (PatternsRepository != null)
                {
                    PatternsRepository.Logger = logger;
                }
                if (PatternConverter != null)
                {
                    PatternConverter.Logger = logger;
                }
                if (LanguageDetector != null)
                {
                    LanguageDetector.Logger = logger;
                }
                AntlrParser.StaticLogger = logger;
            }
        }

        public int ThreadCount { get; set; } = 0;

        public int MemoryConsumptionMb { get; set; } = 3000;

        public TimeSpan FileTimeout { get; set; } = default;

        public int MaxStackSize { get; set; } = Utils.DefaultMaxStackSize;

        public HashSet<Language> AnalyzedLanguages => SourceRepository?.Languages ?? new HashSet<Language>();

        public HashSet<TRenderStage> RenderStages { get; set; } = new HashSet<TRenderStage>();

        public HashSet<TStage> DumpStages { get; set; } = new HashSet<TStage>();

        public bool IsDumpPatterns { get; set; } = false;

        public GraphvizOutputFormat RenderFormat { get; set; } = GraphvizOutputFormat.Png;

        public GraphvizDirection RenderDirection { get; set; } = GraphvizDirection.TopBottom;

        public bool IndentedDump { get; set; } = false;

        public bool DumpWithTextSpans { get; set; } = true;

        public bool IncludeCodeInDump { get; set; } = false;

        public bool LineColumnTextSpans { get; set; } = false;

        public bool StrictJson { get; set; } = false;

        public bool CompressedSerialization { get; set; }

        public string LogsDir { get; set; } = "";

        public string DumpDir { get; set; } = "";

        public string TempDir { get; set; } = "";

        public SerializationFormat SerializationFormat { get; set; } = SerializationFormat.Json;

        public event EventHandler<RootUst> UstConverted;

        public abstract TWorkflowResult Process(TWorkflowResult workflowResult = null, CancellationToken cancellationToken = default);

        public WorkflowBase(TStage stage)
        {
            Stage = stage;
        }

        public RootUst ReadParseAndConvert(string fileName, TWorkflowResult workflowResult,
            CancellationToken cancellationToken = default)
        {
            Language[] languages = SourceRepository.GetLanguages(fileName, true);

            if (languages.Length == 0)
            {
                Logger.LogInfo($"File {fileName} ignored.");
                return null;
            }

            // TODO: implement more fast language detection if there are several languages (SQL dialects at first), see https://github.com/PositiveTechnologies/PT.PM/issues/158
            Language language = languages[0];

            bool isSerialization = language.IsSerializing();

            RootUst result = null;
            var stopwatch = Stopwatch.StartNew();
            var sourceFile = SourceRepository.ReadFile(fileName);
            stopwatch.Stop();

            LogSourceFile((sourceFile, stopwatch.Elapsed), workflowResult);

            cancellationToken.ThrowIfCancellationRequested();

            if (Stage.Is(PM.Stage.File))
            {
                return null;
            }

            ParseTree parseTree = null;
            DetectionResult detectionResult = null;

            if (!isSerialization)
            {
                TextFile sourceTextFile = (TextFile) sourceFile;

                LanguageDetector.MaxStackSize = MaxStackSize;
                detectionResult = LanguageDetector.DetectIfRequired(sourceTextFile, out TimeSpan detectionTimeSpan);

                if (detectionResult == null)
                {
                    Logger.LogInfo(
                        $"File {sourceFile}: unable to detect a language.");
                    return null;
                }

                if (detectionTimeSpan > TimeSpan.Zero)
                {
                    workflowResult.AddDetectTime(detectionTimeSpan);
                    Logger.LogInfo($"File {sourceFile} detected as {detectionResult.Language} (Elapsed: {detectionTimeSpan.Format()}).");
                }

                if (!workflowResult.BaseLanguages.Contains(detectionResult.Language))
                {
                    Logger.LogInfo($"File {sourceFile} ignored.");
                    return null;
                }

                if (Stage.Is(PM.Stage.Language))
                {
                    return null;
                }

                if (detectionResult.ParseTree == null)
                {
                    var parser = detectionResult.Language.CreateParser();
                    parser.Logger = Logger;

                    TimeSpan lexerTimeSpan = TimeSpan.Zero;
                    TimeSpan parserTimeSpan = TimeSpan.Zero;

                    if (parser is AntlrParser antlrParser)
                    {
                        AntlrBaseHandler.MemoryConsumptionBytes = (long)MemoryConsumptionMb * 1024 * 1024;

                        var antlrLexer = (AntlrLexer)antlrParser.Language.CreateLexer();
                        antlrLexer.Logger = Logger;
                        var tokens = antlrLexer.GetTokens(sourceTextFile, out lexerTimeSpan);

                        Logger.LogInfo($"File {sourceFile} tokenized {lexerTimeSpan.GetElapsedString()}.");
                        workflowResult.AddLexerTime(lexerTimeSpan);

                        DumpTokens(tokens, detectionResult.Language, sourceTextFile);

                        if (Stage.Is(PM.Stage.Tokens))
                        {
                            return null;
                        }

                        antlrParser.SourceFile = sourceTextFile;
                        antlrParser.ErrorListener = antlrLexer.ErrorListener;
                        parseTree = antlrParser.Parse(tokens, out parserTimeSpan);
                    }
                    else
                    {
                        if (Stage.Is(PM.Stage.Tokens))
                        {
                            return null;
                        }

                        if (parser is JavaScriptEsprimaParser javaScriptParser)
                        {
                            javaScriptParser.JavaScriptType = JavaScriptType;
                        }

                        parseTree = ((ILanguageParser<TextFile>)parser).Parse(sourceTextFile, out parserTimeSpan);
                    }

                    Logger.LogInfo($"File {sourceFile} parsed {parserTimeSpan.GetElapsedString()}.");

                    workflowResult.AddParserTime(parserTimeSpan);
                }
                else
                {
                    if (Stage.Is(PM.Stage.Tokens))
                    {
                        return null;
                    }

                    foreach (string debug in detectionResult.Debugs)
                    {
                        Logger.LogDebug(debug);
                    }

                    foreach (object info in detectionResult.Infos)
                    {
                        Logger.LogInfo(info);
                    }

                    foreach (Exception error in detectionResult.Errors)
                    {
                        Logger.LogError(error);
                    }

                    parseTree = detectionResult.ParseTree;

                    Logger.LogInfo($"File {sourceFile} parsed");
                }

                if (parseTree == null)
                {
                    return null;
                }

                DumpParseTree(parseTree);

                cancellationToken.ThrowIfCancellationRequested();
            }

            if (Stage.IsGreaterOrEqual(PM.Stage.Ust))
            {
                stopwatch.Restart();

                if (!isSerialization)
                {
                    IParseTreeToUstConverter converter = detectionResult.Language.CreateConverter();
                    if (converter is PhpAntlrParseTreeConverter phpConverter)
                    {
                        phpConverter.JavaScriptType = JavaScriptType;
                    }

                    converter.Logger = Logger;
                    converter.AnalyzedLanguages = AnalyzedLanguages;
                    result = converter.Convert(parseTree);

                    result.FileKey = Interlocked.Increment(ref currentFileId);

                    int currentId = 0;
                    result.ApplyActionToDescendantsAndSelf(ust => ust.Key = ++currentId);

                    stopwatch.Stop();
                    Logger.LogInfo($"File {sourceFile} converted {stopwatch.GetElapsedString()}.");
                    workflowResult.AddConvertTime(stopwatch.Elapsed);
                }
                else
                {
                    void ReadSourceFileAction((IFile, TimeSpan) fileAndTime) =>
                        LogSourceFile(fileAndTime, workflowResult);

                    if (language == Language.Json)
                    {
                        var jsonUstSerializer = new UstJsonSerializer
                        {
                            SourceFiles = workflowResult.SourceFiles,
                            ReadSourceFileAction = ReadSourceFileAction,
                            Strict = StrictJson,
                            Logger = Logger
                        };
                        result = (RootUst) jsonUstSerializer.Deserialize((TextFile) sourceFile);
                    }
                    else if (language == Language.MessagePack)
                    {
                        BinaryFile binaryFile = (BinaryFile) sourceFile;
                        result = RootUstMessagePackSerializer.Deserialize(
                            binaryFile, workflowResult.SourceFiles, ReadSourceFileAction, Logger, out _);
                    }
                    else
                    {
                        Logger.LogError(new ReadException(sourceFile, message: $"Unknown serialization format {language}"));
                    }

                    stopwatch.Stop();
                    Logger.LogInfo($"File {sourceFile} deserialized {stopwatch.GetElapsedString()}.");
                    workflowResult.AddDeserializeTime(stopwatch.Elapsed);

                    if (result == null || !workflowResult.BaseLanguages.Any(lang => result.Sublanguages.Contains(lang)))
                    {
                        Logger.LogInfo($"File {fileName} ignored.");
                        return null;
                    }
                }

                DumpUst(result);
                UstConverted?.Invoke(this, result);

                cancellationToken.ThrowIfCancellationRequested();
            }

            return result;
        }

        public void LogSourceFile((IFile, TimeSpan) fileAndTime, TWorkflowResult workflowResult)
        {
            IFile file = fileAndTime.Item1;

            // File already has been processed
            if (workflowResult.SourceFiles.Contains(file))
            {
                return;
            }

            TimeSpan elapsed = fileAndTime.Item2;

            Logger.LogInfo($"File {fileAndTime.Item1} read (Elapsed: {elapsed.Format()}).");

            workflowResult.AddProcessedFilesCount(1);
            if (file is TextFile sourceFile)
            {
                workflowResult.AddProcessedCharsCount(sourceFile.Data.Length);
                workflowResult.AddProcessedLinesCount(sourceFile.GetLinesCount());
            }

            workflowResult.AddReadTime(elapsed);
            workflowResult.AddResultEntity(file);
        }

        private void DumpTokens(IList<IToken> tokens, Language language, TextFile sourceFile)
        {
            if (DumpStages.Any(stage => stage.Is(PM.Stage.Tokens)) && language.IsAntlr())
            {
                var dumper = new AntlrDumper();
                dumper.IncludeTextSpans = DumpWithTextSpans;
                dumper.IsLineColumn = LineColumnTextSpans;
                dumper.DumpDir = DumpDir;
                dumper.DumpTokens(tokens, language, sourceFile);
            }
        }

        private void DumpParseTree(ParseTree parseTree)
        {
            if (DumpStages.Any(stage => stage.Is(PM.Stage.ParseTree)))
            {
                var dumper = Utils.CreateParseTreeDumper(parseTree.SourceLanguage);
                dumper.IncludeTextSpans = DumpWithTextSpans;
                dumper.IsLineColumn = LineColumnTextSpans;
                dumper.DumpDir = DumpDir;
                dumper.DumpTree(parseTree);
            }
        }

        protected void DumpUst(RootUst result)
        {
            if (DumpStages.Any(stage => stage.Is(PM.Stage.Ust)))
            {
                DirectoryExt.CreateDirectory(DumpDir);
                string name = string.IsNullOrEmpty(result.SourceFile.Name) ? "" : result.SourceFile.Name + ".";
                string dumpName = Path.Combine(DumpDir, name + "ust." + SerializationFormat.GetExtension());

                if (SerializationFormat == SerializationFormat.Json)
                {
                    var ustJsonSerializer = new UstJsonSerializer
                    {
                        CurrectSourceFile = result.SourceFile,
                        IncludeTextSpans = DumpWithTextSpans,
                        IncludeCode = IncludeCodeInDump,
                        Indented = IndentedDump,
                        Strict = StrictJson,
                        LineColumnTextSpans = LineColumnTextSpans,
                        Logger = Logger
                    };
                    string json = ustJsonSerializer.Serialize(result);
                    FileExt.WriteAllText(dumpName, json);
                }
                else
                {
                    byte[] bytes = RootUstMessagePackSerializer.Serialize(result, LineColumnTextSpans, CompressedSerialization, logger);
                    FileExt.WriteAllBytes(dumpName, bytes);
                }
            }
        }

        protected void DumpJsonOutput(TWorkflowResult workflow)
        {
            if (IsDumpJsonOutput)
            {
                string json = JsonConvert.SerializeObject(workflow,
                    IndentedDump ? Formatting.Indented : Formatting.None);
                DirectoryExt.CreateDirectory(DumpDir);
                FileExt.WriteAllText(Path.Combine(DumpDir, "output.json"), json);
            }
        }

        protected List<TPattern> ConvertPatterns(TWorkflowResult workflowResult)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                IEnumerable<PatternDto> patternDtos = PatternsRepository.GetAll();
                List<TPattern> patterns = PatternConverter.Convert(patternDtos);
                stopwatch.Stop();
                if (patterns.Count > 0)
                {
                    workflowResult.AddPatternsTime(stopwatch.Elapsed);
                    workflowResult.AddResultEntity(patterns);
                    workflowResult.AddProcessedPatternsCount(patterns.Count);
                }

                DumpPatterns(patterns);

                return patterns;
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ParsingException(
                    new TextFile("") { PatternKey = "ErroneousPattern" }, ex, $"Patterns can not be deserialized: {ex.FormatExceptionMessage()}"));
                return new List<TPattern>(0);
            }
        }

        protected virtual void DumpPatterns(List<TPattern> patterns)
        {
            if (IsDumpPatterns)
            {
                DumpPatterns(patterns.Where(pattern => pattern is PatternRoot).Cast<PatternRoot>().ToList());
            }
        }

        protected void DumpPatterns(List<PatternRoot> patterns)
        {
            var jsonPatternSerializer = new JsonPatternSerializer
            {
                IncludeTextSpans = DumpWithTextSpans,
                ExcludeDefaults = true,
                Indented = IndentedDump,
                LineColumnTextSpans = LineColumnTextSpans
            };
            jsonPatternSerializer.SourceFiles = new HashSet<IFile>(patterns.Select(root => root.File));

            string json = jsonPatternSerializer.Serialize(patterns);
            DirectoryExt.CreateDirectory(DumpDir);
            FileExt.WriteAllText(Path.Combine(DumpDir, "patterns.json"), json);
        }

        protected static HashSet<Language> GetBaseLanguages(HashSet<Language> analyzedLanguages)
        {
            HashSet<Language> result = new HashSet<Language>();
            foreach (Language language in analyzedLanguages)
            {
                result.Add(language);
                Language superLangInfo = language;
                do
                {
                    superLangInfo = LanguageUtils.Languages.FirstOrDefault(l => l.GetSublanguages().Contains(superLangInfo));
                    if (superLangInfo != Language.Uncertain)
                    {
                        result.Add(superLangInfo);
                    }
                }
                while (superLangInfo != Language.Uncertain);
            }
            return result;
        }

        public ParallelOptions PrepareParallelOptions(CancellationToken cancellationToken)
        {
            return new ParallelOptions
            {
                MaxDegreeOfParallelism = ThreadCount == 0 ? -1 : ThreadCount,
                CancellationToken = cancellationToken
            };
        }
    }
}
