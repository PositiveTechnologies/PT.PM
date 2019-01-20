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
using PT.PM.Common.Files;
using PT.PM.Common.MessagePack;

namespace PT.PM
{
    public abstract class WorkflowBase<TStage, TWorkflowResult, TPattern, TRenderStage> : ILoggable
        where TStage : Enum
        where TWorkflowResult : WorkflowResultBase<TStage, TPattern, TRenderStage>
        where TRenderStage : Enum
    {
        private int currentId = 0;

        protected ILogger logger = DummyLogger.Instance;

        static WorkflowBase()
        {
            Utils.RegisterAllParsersAndCovnerters();
        }

        public TStage Stage { get; set; }

        public TStage StartStage { get; set; }

        public SourceRepository SourceRepository { get; set; }

        public IPatternsRepository PatternsRepository { get; set; }

        public IPatternConverter<TPattern> PatternConverter { get; set; }

        public LanguageDetector LanguageDetector { get; } = new ParserLanguageDetector();

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

        public HashSet<Language> BaseLanguages { get; set; } = new HashSet<Language>(LanguageUtils.Languages);

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

        public string LogsDir { get; set; } = "";

        public string DumpDir { get; set; } = "";

        public string TempDir { get; set; } = "";

        public SerializationFormat SerializationFormat { get; set; }

        public event EventHandler<RootUst> UstConverted;

        public abstract TWorkflowResult Process(TWorkflowResult workflowResult = null, CancellationToken cancellationToken = default);

        public WorkflowBase(TStage stage)
        {
            Stage = stage;
        }

        public RootUst ReadParseAndConvert(string fileName, TWorkflowResult workflowResult, CancellationToken cancellationToken = default)
        {
            if (SourceRepository.IsFileIgnored(fileName, true))
            {
                Logger.LogInfo($"File {fileName} not read.");
                return null;
            }

            RootUst result = null;
            var stopwatch = Stopwatch.StartNew();
            var sourceFile = SourceRepository.ReadFile(fileName);
            stopwatch.Stop();

            LogSourceFile((sourceFile, stopwatch.Elapsed), workflowResult);

            cancellationToken.ThrowIfCancellationRequested();

            string shortFileName = sourceFile.Name;

            if (Stage.IsGreaterOrEqual(PM.Stage.ParseTree))
            {
                ParseTree parseTree = null;
                DetectionResult detectionResult = null;

                if (StartStage.Is(PM.Stage.File))
                {
                    TextFile sourceTextFile = (TextFile) sourceFile;

                    stopwatch.Restart();
                    LanguageDetector.MaxStackSize = MaxStackSize;
                    detectionResult = LanguageDetector.DetectIfRequired(sourceTextFile, workflowResult.BaseLanguages);

                    if (detectionResult == null)
                    {
                        Logger.LogInfo($"Input languages set is empty, {shortFileName} language can not been detected, or file too big (timeout break). File not converted.");
                        return null;
                    }

                    if (detectionResult.ParseTree == null)
                    {
                        var parser = detectionResult.Language.CreateParser();
                        parser.Logger = Logger;
                        if (parser is AntlrParser)
                        {
                            AntlrParser.MemoryConsumptionBytes = (long)MemoryConsumptionMb * 1024 * 1024;
                        }
                        if (parser is JavaScriptEsprimaParser javaScriptParser)
                        {
                            javaScriptParser.JavaScriptType = JavaScriptType;
                        }
                        parseTree = parser.Parse(sourceTextFile);
                    }
                    else
                    {
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
                    }

                    stopwatch.Stop();
                    Logger.LogInfo($"File {shortFileName} parsed {stopwatch.GetElapsedString()}.");

                    if (parseTree == null)
                    {
                        return null;
                    }

                    workflowResult.AddLexerTime(parseTree.LexerTimeSpan);
                    workflowResult.AddParserTicks(parseTree.ParserTimeSpan);

                    DumpTokensAndParseTree(parseTree);

                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (Stage.IsGreaterOrEqual(PM.Stage.Ust))
                {
                    stopwatch.Restart();

                    if (!StartStage.Is(PM.Stage.Ust))
                    {
                        IParseTreeToUstConverter converter = detectionResult.Language.CreateConverter();
                        if (converter is PhpAntlrParseTreeConverter phpConverter)
                        {
                            phpConverter.JavaScriptType = JavaScriptType;
                        }
                        converter.Logger = Logger;
                        converter.AnalyzedLanguages = AnalyzedLanguages;
                        result = converter.Convert(parseTree);
                    }
                    else
                    {
                        void ReadSourceFileAction((IFile, TimeSpan) fileAndTime) => LogSourceFile(fileAndTime, workflowResult);

                        if (SerializationFormat == SerializationFormat.Json)
                        {
                            var jsonUstSerializer = new UstJsonSerializer
                            {
                                SourceFiles = workflowResult.SourceFiles,
                                ReadSourceFileAction = ReadSourceFileAction,
                                Strict = StrictJson,
                                LineColumnTextSpans = LineColumnTextSpans,
                                Logger = Logger
                            };
                            result = (RootUst) jsonUstSerializer.Deserialize((TextFile)sourceFile);
                        }
                        else
                        {
                            BinaryFile binaryFile = (BinaryFile) sourceFile;
                            result = RootUstMessagePackSerializer.Deserialize(
                                binaryFile, LineColumnTextSpans, workflowResult.SourceFiles, ReadSourceFileAction, Logger, out _);
                        }

                        if (result == null || !AnalyzedLanguages.Any(lang => result.Sublanguages.Contains(lang)))
                        {
                            Logger.LogInfo($"File {fileName} ignored.");
                            return null;
                        }
                    }

                    stopwatch.Stop();
                    Logger.LogInfo($"File {shortFileName} converted {stopwatch.GetElapsedString()}.");
                    workflowResult.AddConvertTime(stopwatch.Elapsed);

                    if (result == null)
                    {
                        return null;
                    }

                    result.ApplyActionToDescendantsAndSelf(ust => ust.Key = Interlocked.Increment(ref currentId));

                    DumpUst(result);
                    UstConverted?.Invoke(this, result);

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            return result;
        }

        public void LogSourceFile((IFile, TimeSpan) fileAndTime, TWorkflowResult workflowResult)
        {
            IFile file = fileAndTime.Item1;
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

        private void DumpTokensAndParseTree(ParseTree parseTree)
        {
            if (DumpStages.Any(stage => stage.Is(PM.Stage.ParseTree)))
            {
                var dumper = Utils.CreateParseTreeDumper(parseTree.SourceLanguage);
                dumper.IncludeTextSpans = DumpWithTextSpans;
                dumper.IsLineColumn = LineColumnTextSpans;
                dumper.DumpDir = DumpDir;
                dumper.DumpTokens(parseTree);
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
                    byte[] bytes = RootUstMessagePackSerializer.Serialize(result, LineColumnTextSpans, logger);
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
