using Newtonsoft.Json;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Json;
using PT.PM.Common.Nodes;
using PT.PM.CSharpParseTreeUst;
using PT.PM.JavaScriptParseTreeUst;
using PT.PM.Matching;
using PT.PM.Matching.Json;
using PT.PM.Matching.PatternsRepository;
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
        where TStage : struct, IConvertible
        where TWorkflowResult : WorkflowResultBase<TStage, TPattern, TMatchResult, TRenderStage>
        where TMatchResult : MatchResultBase<TPattern>
        where TRenderStage : struct, IConvertible
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

        public bool IsIncludeIntermediateResult { get; set; }

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

        public int MemoryConsumptionMb { get; set; } = 300;

        public TimeSpan FileTimeout { get; set; } = default(TimeSpan);

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

        public bool StrictJson { get; set; } = false;
 
        public string LogsDir { get; set; } = "";

        public string DumpDir { get; set; } = "";

        public string TempDir { get; set; } = "";

        public abstract TWorkflowResult Process(TWorkflowResult workflowResult = null, CancellationToken cancellationToken = default(CancellationToken));

        public WorkflowBase(TStage stage)
        {
            Stage = stage;
        }

        protected RootUst ReadParseAndConvert(string fileName, TWorkflowResult workflowResult, CancellationToken cancellationToken = default(CancellationToken))
        {
            RootUst result = null;
            var stopwatch = new Stopwatch();
            if (Stage.IsGreaterOrEqual(PM.Stage.File))
            {
                if (SourceCodeRepository.IsFileIgnored(fileName, true))
                {
                    Logger.LogInfo($"File {fileName} has not been read.");
                    return null;
                }

                CodeFile sourceCodeFile = ReadFile(fileName, workflowResult);

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
                            Logger.LogInfo($"Input languages set is empty, {shortFileName} language can not been detected, or file too big (timeout break). File has not been converter.");
                            return null;
                        }

                        if (detectionResult.ParseTree == null)
                        {
                            var parser = detectionResult.Language.CreateParser();
                            parser.Logger = Logger;
                            if (parser is AntlrParser antlrParser)
                            {
                                AntlrParser.MemoryConsumptionBytes = MemoryConsumptionMb * 1000 * 1000;
                                if (parser is JavaScriptAntlrParser javaScriptAntlrParser)
                                {
                                    javaScriptAntlrParser.JavaScriptType = JavaScriptType;
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
                        Logger.LogInfo($"File {shortFileName} has been parsed (Elapsed: {stopwatch.Elapsed}).");
                        workflowResult.AddParseTime(stopwatch.ElapsedTicks);
                        workflowResult.AddResultEntity(parseTree);

                        if (parseTree is AntlrParseTree antlrParseTree)
                        {
                            workflowResult.AddLexerTime(antlrParseTree.LexerTimeSpan.Ticks);
                            workflowResult.AddParserTicks(antlrParseTree.ParserTimeSpan.Ticks);
                        }

                        DumpTokensAndParseTree(parseTree);

                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    if (Stage.IsGreaterOrEqual(PM.Stage.Ust))
                    {
                        stopwatch.Restart();

                        if (!StartStage.Is(PM.Stage.Ust))
                        {
                            IParseTreeToUstConverter converter = detectionResult.Language.CreateConverter();
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
                            result = (RootUst)jsonUstSerializer.Deserialize(sourceCodeFile);

                            if (!AnalyzedLanguages.Any(lang => result.Sublanguages.Contains(lang)))
                            {
                                Logger.LogInfo($"File {fileName} has been ignored.");
                                return null;
                            }
                        }

                        stopwatch.Stop();
                        Logger.LogInfo($"File {shortFileName} has been converted (Elapsed: {stopwatch.Elapsed}).");
                        workflowResult.AddConvertTime(stopwatch.ElapsedTicks);

                        if (IsSimplifyUst)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var simplifier = new UstSimplifier { Logger = logger };
                            stopwatch.Restart();
                            result = simplifier.Simplify(result);
                            stopwatch.Stop();
                            Logger.LogInfo($"Ust of file {result.SourceCodeFile.Name} has been simplified (Elapsed: {stopwatch.Elapsed}).");
                            workflowResult.AddSimplifyTime(stopwatch.ElapsedTicks);
                        }

                        DumpUst(result, workflowResult.SourceCodeFiles);
                        workflowResult.AddResultEntity(result);

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }
            return result;
        }

        public CodeFile ReadFile(string fileName, TWorkflowResult workflowResult)
        {
            var stopwatch = Stopwatch.StartNew();
            CodeFile sourceCodeFile = SourceCodeRepository.ReadFile(fileName);
            stopwatch.Stop();

            Logger.LogInfo($"File {fileName} has been read (Elapsed: {stopwatch.Elapsed}).");

            workflowResult.AddProcessedCharsCount(sourceCodeFile.Code.Length);
            workflowResult.AddProcessedLinesCount(sourceCodeFile.GetLinesCount());
            workflowResult.AddReadTime(stopwatch.ElapsedTicks);
            workflowResult.AddResultEntity(sourceCodeFile);

            return sourceCodeFile;
        }

        private void DumpTokensAndParseTree(ParseTree parseTree)
        {
            if (DumpStages.Any(stage => stage.Is(PM.Stage.ParseTree)))
            {
                ParseTreeDumper dumper;
                if (parseTree.SourceLanguage.HaveAntlrParser)
                {
                    dumper = new AntlrDumper();
                }
                else
                {
                    dumper = new RoslynDumper();
                }
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
                    CurrectCodeFile = result.SourceCodeFile
                };
                string json = serializer.Serialize(result);
                string name = string.IsNullOrEmpty(result.SourceCodeFile.Name) ? "" : result.SourceCodeFile.Name + ".";
                Directory.CreateDirectory(DumpDir);
                File.WriteAllText(Path.Combine(DumpDir, name + ParseTreeDumper.UstSuffix), json);
            }
        }

        protected void DumpJsonOutput(TWorkflowResult workflow)
        {
            if (IsDumpJsonOutput)
            {
                string json = JsonConvert.SerializeObject(workflow,
                    IndentedDump ? Formatting.Indented : Formatting.None, LanguageJsonConverter.Instance);
                Directory.CreateDirectory(DumpDir);
                File.WriteAllText(Path.Combine(DumpDir, "output.json"), json);
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
                    workflowResult.AddPatternsTime(stopwatch.ElapsedTicks);
                    workflowResult.AddResultEntity(patterns);
                    workflowResult.AddProcessedPatternsCount(patterns.Count);
                }

                DumpPatterns(patterns);

                return patterns;
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ParsingException(
                    new CodeFile("") { IsPattern = true }, ex, $"Patterns can not be deserialized: {ex.FormatExceptionMessage()}"));
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
            Directory.CreateDirectory(DumpDir);
            File.WriteAllText(Path.Combine(DumpDir, "patterns.json"), json);
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
    }
}
