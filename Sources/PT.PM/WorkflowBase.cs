using Newtonsoft.Json;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Json;
using PT.PM.Common.Nodes;
using PT.PM.Common.Utils;
using PT.PM.CSharpParseTreeUst;
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

namespace PT.PM
{
    public abstract class WorkflowBase<TInputGraph, TStage, TWorkflowResult, TPattern, TMatchResult, TRenderStage> : ILoggable
        where TStage : Enum
        where TWorkflowResult : WorkflowResultBase<TStage, TPattern, TMatchResult, TRenderStage>
        where TMatchResult : MatchResultBase<TPattern>
        where TRenderStage : Enum
    {
        protected ILogger logger = DummyLogger.Instance;
        protected Task filesCountTask;
        protected Task<TPattern[]> convertPatternsTask;

        protected Language[] languages;

        public TStage Stage { get; set; }

        public TStage StartStage { get; set; }

        public SourceCodeRepository SourceCodeRepository { get; set; }

        public IPatternsRepository PatternsRepository { get; set; }

        public IPatternConverter<TPattern> PatternConverter { get; set; }

        public LanguageDetector LanguageDetector { get; set; } = new ParserLanguageDetector();

        public bool IsDumpJsonOutput { get; set; } = false;

        public bool IsSimplifyUst { get; set; } = true;

        public bool IsIgnoreFilenameWildcards { get; set; } = false;

        public JavaScriptType JavaScriptType { get; set; } = JavaScriptType.Undefined;

        public ILogger Logger
        {
            get => logger;
            set
            {
                logger = value;
                if (SourceCodeRepository != null)
                {
                    SourceCodeRepository.Logger = Logger;
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

        public HashSet<Language> AnalyzedLanguages => SourceCodeRepository?.Languages ?? new HashSet<Language>();

        public HashSet<Language> BaseLanguages { get; set; } = new HashSet<Language>(LanguageUtils.Languages.Values);

        public HashSet<TRenderStage> RenderStages { get; set; } = new HashSet<TRenderStage>();

        public HashSet<TStage> DumpStages { get; set; } = new HashSet<TStage>();

        public bool IsDumpPatterns { get; set; } = false;

        public GraphvizOutputFormat RenderFormat { get; set; } = GraphvizOutputFormat.Png;

        public GraphvizDirection RenderDirection { get; set; } = GraphvizDirection.TopBottom;

        public bool IndentedDump { get; set; } = false;

        public bool DumpWithTextSpans { get; set; } = true;

        public bool IncludeCodeInDump { get; set; } = false;

        public bool LineColumnTextSpans { get; set; } = false;

        public bool RelativeNamesInTextSpans { get; set; } = false;

        public bool StrictJson { get; set; } = false;

        public string LogsDir { get; set; } = "";

        public string DumpDir { get; set; } = "";

        public string TempDir { get; set; } = "";

        public abstract TWorkflowResult Process(TWorkflowResult workflowResult = null, CancellationToken cancellationToken = default);

        public WorkflowBase(TStage stage)
        {
            Stage = stage;
        }

        protected RootUst ReadParseAndConvert(string fileName, TWorkflowResult workflowResult, CancellationToken cancellationToken = default)
        {
            if (Stage.IsLess(PM.Stage.File))
            {
                return null;
            }

            RootUst result = null;
            var stopwatch = new Stopwatch();
            if (SourceCodeRepository.IsFileIgnored(fileName, true))
            {
                Logger.LogInfo($"File {fileName} not read.");
                return null;
            }

            stopwatch.Restart();
            CodeFile sourceCodeFile = SourceCodeRepository.ReadFile(fileName);
            stopwatch.Stop();

            LogCodeFile((sourceCodeFile, stopwatch.Elapsed), workflowResult);

            cancellationToken.ThrowIfCancellationRequested();

            string shortFileName = sourceCodeFile.Name;

            if (Stage.IsGreaterOrEqual(PM.Stage.ParseTree))
            {
                ParseTree parseTree = null;
                DetectionResult detectionResult = null;

                if (StartStage.Is(PM.Stage.File))
                {
                    stopwatch.Restart();
                    LanguageDetector.MaxStackSize = MaxStackSize;
                    detectionResult = LanguageDetector.DetectIfRequired(sourceCodeFile, workflowResult.BaseLanguages);

                    if (detectionResult == null)
                    {
                        Logger.LogInfo($"Input languages set is empty, {shortFileName} language can not been detected, or file too big (timeout break). File not converted.");
                        return null;
                    }

                    if (detectionResult.ParseTree == null)
                    {
                        var parser = detectionResult.Language.CreateParser();
                        parser.Logger = Logger;
                        if (parser is AntlrParser antlrParser)
                        {
                            AntlrParser.MemoryConsumptionBytes = (long)MemoryConsumptionMb * 1024 * 1024;
                            if (parser is JavaScriptEsprimaParser javaScriptParser)
                            {
                                javaScriptParser.JavaScriptType = JavaScriptType;
                            }
                        }
                        parseTree = parser.Parse(sourceCodeFile);
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
                    Logger.LogInfo($"File {shortFileName} parsed {GetElapsedString(stopwatch)}.");

                    if (parseTree == null)
                    {
                        return null;
                    }

                    workflowResult.AddResultEntity(parseTree);
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
                        var jsonUstSerializer = new UstJsonSerializer
                        {
                            Logger = Logger,
                            LineColumnTextSpans = LineColumnTextSpans,
                            Strict = StrictJson,
                            CodeFiles = workflowResult.SourceCodeFiles
                        };
                        jsonUstSerializer.ReadCodeFileEvent += (object sender, (CodeFile, TimeSpan) fileAndTime) =>
                        {
                            LogCodeFile(fileAndTime, workflowResult);
                        };
                        result = (RootUst)jsonUstSerializer.Deserialize(sourceCodeFile);

                        if (result == null || !AnalyzedLanguages.Any(lang => result.Sublanguages.Contains(lang)))
                        {
                            Logger.LogInfo($"File {fileName} ignored.");
                            return null;
                        }
                    }

                    stopwatch.Stop();
                    Logger.LogInfo($"File {shortFileName} converted {GetElapsedString(stopwatch)}.");
                    workflowResult.AddConvertTime(stopwatch.Elapsed);

                    if (result == null)
                    {
                        return null;
                    }

                    if (IsSimplifyUst)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var simplifier = new UstSimplifier { Logger = logger };
                        stopwatch.Restart();
                        result = simplifier.Simplify(result);
                        stopwatch.Stop();
                        Logger.LogInfo($"Ust of file {result.SourceCodeFile.Name} simplified {GetElapsedString(stopwatch)}.");
                        workflowResult.AddSimplifyTime(stopwatch.Elapsed);
                    }

                    DumpUst(result, workflowResult.SourceCodeFiles);
                    workflowResult.AddResultEntity(result);

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            return result;
        }

        public void LogCodeFile((CodeFile, TimeSpan) fileAndTime, TWorkflowResult workflowResult)
        {
            CodeFile codeFile = fileAndTime.Item1;
            TimeSpan elapsed = fileAndTime.Item2;

            Logger.LogInfo($"File {fileAndTime.Item1} read (Elapsed: {elapsed.Format()}).");

            workflowResult.AddProcessedCharsCount(codeFile.Code.Length);
            workflowResult.AddProcessedLinesCount(codeFile.GetLinesCount());
            workflowResult.AddReadTime(elapsed);
            workflowResult.AddResultEntity(codeFile);
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

        protected void DumpUst(RootUst result, HashSet<CodeFile> sourceCodeFiles)
        {
            if (DumpStages.Any(stage => stage.Is(PM.Stage.Ust)))
            {
                var serializer = new UstJsonSerializer
                {
                    Logger = Logger,
                    Indented = IndentedDump,
                    IncludeTextSpans = DumpWithTextSpans,
                    IncludeCode = IncludeCodeInDump,
                    LineColumnTextSpans = LineColumnTextSpans,
                    Strict = StrictJson,
                    CodeFiles = sourceCodeFiles,
                    CurrectCodeFile = result.SourceCodeFile,
                    RelativeFileNames = RelativeNamesInTextSpans
                };
                string json = serializer.Serialize(result);
                string name = string.IsNullOrEmpty(result.SourceCodeFile.Name) ? "" : result.SourceCodeFile.Name + ".";
                DirectoryExt.CreateDirectory(DumpDir);
                FileExt.WriteAllText(Path.Combine(DumpDir, name + ParseTreeDumper.UstSuffix), json);
            }
        }

        protected void DumpJsonOutput(TWorkflowResult workflow)
        {
            if (IsDumpJsonOutput)
            {
                string json = JsonConvert.SerializeObject(workflow,
                    IndentedDump ? Formatting.Indented : Formatting.None, LanguageJsonConverter.Instance);
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
                    new CodeFile("") { PatternKey = "ErroneousPattern" }, ex, $"Patterns can not be deserialized: {ex.FormatExceptionMessage()}"));
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
            jsonPatternSerializer.CodeFiles = new HashSet<CodeFile>(patterns.Select(root => root.CodeFile));

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
                    superLangInfo = LanguageUtils.Languages.FirstOrDefault(l => l.Value.Sublanguages.Contains(superLangInfo)).Value;
                    if (superLangInfo != null)
                    {
                        result.Add(superLangInfo);
                    }
                }
                while (superLangInfo != null);
            }
            return result;
        }

        protected ParallelOptions PrepareParallelOptions(CancellationToken cancellationToken)
        {
            return new ParallelOptions
            {
                MaxDegreeOfParallelism = ThreadCount == 0 ? -1 : ThreadCount,
                CancellationToken = cancellationToken
            };
        }

        protected string GetElapsedString(Stopwatch stopwatch) => $"(Elapsed: {stopwatch.Elapsed.Format()})";
    }
}
