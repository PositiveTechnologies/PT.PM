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
        private static readonly object lockObj = new object();
        private int currentFileId;
        private int currentId;

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

        public int CurrentFileId
        {
            get => currentFileId;
            set => currentFileId = value;
        }

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

        public int ThreadCount { get; set; }

        public int MemoryConsumptionMb { get; set; } = 2000;

        public TimeSpan FileTimeout { get; set; }

        public int MaxStackSize { get; set; } = Utils.DefaultMaxStackSize;

        public HashSet<Language> AnalyzedLanguages => SourceRepository?.Languages ?? new HashSet<Language>();

        public HashSet<TRenderStage> RenderStages { get; set; } = new HashSet<TRenderStage>();

        public HashSet<TStage> DumpStages { get; set; } = new HashSet<TStage>();

        public bool IsDumpPatterns { get; set; }

        public GraphvizOutputFormat RenderFormat { get; set; } = GraphvizOutputFormat.Png;

        public GraphvizDirection RenderDirection { get; set; } = GraphvizDirection.TopBottom;

        public bool IndentedDump { get; set; }

        public bool DumpWithTextSpans { get; set; } = true;

        public bool IncludeCodeInDump { get; set; }

        public bool LineColumnTextSpans { get; set; }

        public bool StrictJson { get; set; }

        public string LogsDir { get; set; } = "";

        public string DumpDir { get; set; } = "";

        public string TempDir { get; set; } = "";

        public SerializationFormat SerializationFormat { get; set; } = SerializationFormat.MsgPack;

        public event EventHandler<IFile> FileRead;

        public event EventHandler<RootUst> UstConverted;

        public abstract TWorkflowResult Process(TWorkflowResult workflowResult = null, CancellationToken cancellationToken = default);

        public WorkflowBase(TStage stage)
        {
            Stage = stage;
        }

        public RootUst ReadParseAndConvert(string fileName, TWorkflowResult workflowResult,
            CancellationToken cancellationToken = default)
        {
            IFile sourceFile = Read(fileName, workflowResult, cancellationToken, out Language language);

            if (sourceFile == null)
            {
                return null;
            }

            bool isSerializing = language.IsSerializing();

            ParseTree parseTree = null;
            DetectionResult detectionResult = null;

            if (!isSerializing)
            {
                detectionResult = Detect(workflowResult, sourceFile);

                if (detectionResult == null)
                {
                    return null;
                }

                if (detectionResult.ParseTree != null)
                {
                    parseTree = LogParseTree(sourceFile, detectionResult);
                }
                else if (!detectionResult.Language.IsParserConverter())
                {
                    parseTree = Parse(workflowResult, sourceFile, detectionResult.Language, cancellationToken);

                    if (parseTree == null)
                    {
                        return null;
                    }
                }

                GetParseTreeDumper(language)?.DumpTree(parseTree);
            }

            RootUst rootUst = Convert(fileName, workflowResult, sourceFile, isSerializing, detectionResult,
                language, parseTree, cancellationToken);

            return rootUst;
        }

        private IFile Read(string fileName, TWorkflowResult workflowResult, CancellationToken cancellationToken,
            out Language language)
        {
            Language[] languages = SourceRepository.GetLanguages(fileName, true);
            language = Language.Uncertain;

            if (languages.Length == 0)
            {
                Logger.LogInfo($"File {fileName} ignored.");
                return null;
            }

            // TODO: implement more fast language detection if there are several languages (SQL dialects at first), see https://github.com/PositiveTechnologies/PT.PM/issues/158
            language = languages[0];

            var stopwatch = Stopwatch.StartNew();
            IFile sourceFile = SourceRepository.ReadFile(fileName);
            stopwatch.Stop();

            LogSourceFile((sourceFile, stopwatch.Elapsed), workflowResult, false);

            cancellationToken.ThrowIfCancellationRequested();

            if (Stage.Is(PM.Stage.File))
            {
                return null;
            }

            return sourceFile;
        }

        private DetectionResult Detect(TWorkflowResult workflowResult, IFile sourceFile)
        {
            TextFile sourceTextFile = (TextFile) sourceFile;

            LanguageDetector.MaxStackSize = MaxStackSize;
            DetectionResult detectionResult = LanguageDetector.DetectIfRequired(sourceTextFile, out TimeSpan detectionTimeSpan);

            if (detectionResult == null)
            {
                Logger.LogInfo(
                    $"File {sourceFile}: unable to detect a language.");
                return null;
            }

            if (detectionTimeSpan > TimeSpan.Zero)
            {
                workflowResult.AddDetectTime(detectionTimeSpan);
                Logger.LogInfo(
                    $"File {sourceFile} detected as {detectionResult.Language} (Elapsed: {detectionTimeSpan.Format()}).");
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

            return detectionResult;
        }

        private ParseTree Parse(TWorkflowResult workflowResult,
            IFile sourceFile, Language language, CancellationToken cancellationToken)
        {
            ParseTree result;
            TextFile sourceTextFile = (TextFile) sourceFile;
            ILanguageParserBase parser = language.CreateParser();
            parser.Logger = Logger;

            TimeSpan parserTimeSpan;

            if (parser is AntlrParser antlrParser)
            {
                IList<IToken> tokens = Tokenize(workflowResult, language, sourceTextFile);

                if (Stage.Is(PM.Stage.Tokens))
                {
                    return null;
                }

                result = antlrParser.Parse(tokens, out parserTimeSpan);
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

                result = ((ILanguageParser<TextFile>) parser).Parse(sourceTextFile, out parserTimeSpan);
            }

            Logger.LogInfo($"File {sourceFile} parsed {parserTimeSpan.GetElapsedString()}.");

            workflowResult.AddParserTime(parserTimeSpan);

            return result;
        }

        private ParseTree LogParseTree(IFile sourceFile, DetectionResult detectionResult)
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

            Logger.LogInfo($"File {sourceFile} parse tree processed");
            return detectionResult.ParseTree;
        }

        private RootUst Convert(string fileName, TWorkflowResult workflowResult, IFile sourceFile,
            bool isSerializing, DetectionResult detectionResult, Language language, ParseTree parseTree,
            CancellationToken cancellationToken)
        {
            if (Stage.IsLess(PM.Stage.Ust))
            {
                return null;
            }

            RootUst result = null;
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (!isSerializing)
            {
                Language detectedLanguage = detectionResult.Language;
                if (detectedLanguage.IsParserConverter())
                {
                    var parserConverter = (AntlrParserConverter) detectedLanguage.CreateParserConverter();
                    parserConverter.Logger = Logger;
                    parserConverter.ParseTreeDumper = (AntlrDumper) GetParseTreeDumper(detectedLanguage);

                    if (detectionResult.ParseTree == null)
                    {
                        IList<IToken> tokens = Tokenize(workflowResult, detectedLanguage, (TextFile) sourceFile);

                        result = parserConverter.ParseConvert(tokens, out TimeSpan parserTimeSpan,
                            out TimeSpan converterTimeSpan);

                        stopwatch.Stop();
                        Logger.LogInfo($"File {sourceFile} parsed and converted {stopwatch.GetElapsedString()}.");
                        workflowResult.AddParserTime(parserTimeSpan);
                        workflowResult.AddConvertTime(converterTimeSpan);
                    }
                    else
                    {
                        result = parserConverter.Convert((AntlrParseTree) detectionResult.ParseTree);

                        stopwatch.Stop();
                        Logger.LogInfo($"File {sourceFile} converted {stopwatch.GetElapsedString()}.");
                        workflowResult.AddConvertTime(stopwatch.Elapsed);
                    }
                }
                else
                {
                    IParseTreeToUstConverter converter = detectedLanguage.CreateConverter();
                    if (converter is PhpAntlrParseTreeConverter phpConverter)
                    {
                        phpConverter.JavaScriptType = JavaScriptType;
                    }

                    converter.Logger = Logger;
                    converter.AnalyzedLanguages = AnalyzedLanguages;
                    result = converter.Convert(parseTree);

                    stopwatch.Stop();
                    Logger.LogInfo($"File {sourceFile} converted {stopwatch.GetElapsedString()}.");
                    workflowResult.AddConvertTime(stopwatch.Elapsed);
                }

                if (result != null)
                {
                    result.FileKey = Interlocked.Increment(ref currentFileId);
                    lock (lockObj)
                    {
                        result.ApplyActionToDescendantsAndSelf(ust => ust.Key = ++currentId);
                    }
                }
            }
            else
            {
                void ReadSourceFileAction((IFile, TimeSpan) fileAndTime) =>
                    LogSourceFile(fileAndTime, workflowResult, false);

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

            if (result != null)
            {
                DumpUst(result);
                UstConverted?.Invoke(this, result);
            }

            cancellationToken.ThrowIfCancellationRequested();

            return result;
        }

        public void LogSourceFile((IFile, TimeSpan) fileAndTime, TWorkflowResult workflowResult, bool saveFile)
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
            if (saveFile)
            {
                workflowResult.AddResultEntity(file);
            }

            FileRead?.Invoke(this, file);
        }

        private IList<IToken> Tokenize(TWorkflowResult workflowResult, Language language, TextFile sourceTextFile)
        {
            var antlrLexer = (AntlrLexer) language.CreateLexer();
            antlrLexer.Logger = Logger;
            IList<IToken> tokens = antlrLexer.GetTokens(sourceTextFile, out TimeSpan lexerTimeSpan);

            Logger.LogInfo($"File {sourceTextFile} tokenized {lexerTimeSpan.GetElapsedString()}.");
            workflowResult.AddLexerTime(lexerTimeSpan);

            DumpTokens(tokens, language, sourceTextFile);

            return tokens;
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

        private ParseTreeDumper GetParseTreeDumper(Language language)
        {
            if (DumpStages.Any(stage => stage.Is(PM.Stage.ParseTree)))
            {
                var dumper = Utils.CreateParseTreeDumper(language);
                dumper.IncludeTextSpans = DumpWithTextSpans;
                dumper.IsLineColumn = LineColumnTextSpans;
                dumper.DumpDir = DumpDir;
                return dumper;
            }

            return null;
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
                    byte[] bytes = RootUstMessagePackSerializer.Serialize(result, LineColumnTextSpans, logger);
                    FileExt.WriteAllBytes(dumpName, bytes);
                }

                Logger.LogInfo(new ProgressEventArgs(0.0, dumpName, Utils.FileSerializedMessage));
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
                CancellationToken = cancellationToken,
            };
        }
    }
}
