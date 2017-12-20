using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Json;
using PT.PM.Common.Nodes;
using PT.PM.CSharpParseTreeUst;
using PT.PM.JavaScriptParseTreeUst;
using PT.PM.Matching;
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
    public abstract class WorkflowBase<TInputGraph, TStage, TWorkflowResult, TPattern, TMatchResult> : ILoggable
        where TStage : struct, IConvertible
        where TWorkflowResult : WorkflowResultBase<TStage, TPattern, TMatchResult>
        where TMatchResult : MatchResultBase<TPattern>
    {
        protected ILogger logger = DummyLogger.Instance;
        protected Task filesCountTask;
        protected Task convertPatternsTask;

        protected Language[] languages;

        public TStage Stage { get; set; }

        public TStage StartStage { get; set; }

        public SourceCodeRepository SourceCodeRepository { get; set; }

        public IPatternsRepository PatternsRepository { get; set; }

        public IPatternConverter<TPattern> PatternConverter { get; set; }

        public IUstPatternMatcher<TInputGraph, TPattern, TMatchResult> UstPatternMatcher { get; set; }

        public LanguageDetector LanguageDetector { get; set; } = new ParserLanguageDetector();

        public bool IsIncludeIntermediateResult { get; set; }

        public bool IsIncludePreprocessing { get; set; } = true;

        public bool IsAsyncPatternsConversion { get; set; } = true;

        public JavaScriptType JavaScriptType { get; set; } = JavaScriptType.Undefined;

        public ILogger Logger
        {
            get { return logger; }
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
                if (UstPatternMatcher != null)
                {
                    UstPatternMatcher.Logger = logger;
                }
                if (LanguageDetector != null)
                {
                    LanguageDetector.Logger = logger;
                }
                if (logger != null)
                {
                    logger.SourceCodeRepository = SourceCodeRepository;
                }
            }
        }

        public int ThreadCount { get; set; }

        public long MemoryConsumptionMb { get; set; } = 300;

        public HashSet<Language> AnalyzedLanguages => SourceCodeRepository?.Languages ?? new HashSet<Language>();

        public HashSet<Language> BaseLanguages { get; set; } = new HashSet<Language>(LanguageUtils.Languages.Values);

        public HashSet<TStage> RenderStages { get; set; } = new HashSet<TStage>();

        public HashSet<TStage> DumpStages { get; set; } = new HashSet<TStage>();

        public GraphvizOutputFormat RenderFormat { get; set; } = GraphvizOutputFormat.Png;

        public GraphvizDirection RenderDirection { get; set; } = GraphvizDirection.TopBottom;

        public bool IndentedDump { get; set; } = true;

        public bool DumpWithTextSpans { get; set; } = true;

        public bool IncludeCodeInDump { get; set; } = true;

        public bool LineColumnTextSpans { get; set; } = false;

        public string LogsDir { get; set; } = "";

        public string DumpDir { get; set; } = "";

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
                if (SourceCodeRepository.IsFileIgnored(fileName))
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
                    Language detectedLanguage = null;

                    if (StartStage.Is(PM.Stage.File))
                    {
                        stopwatch.Restart();
                        detectedLanguage = LanguageDetector.DetectIfRequired(sourceCodeFile.Name, sourceCodeFile.Code, workflowResult.BaseLanguages);
                        if (detectedLanguage == null)
                        {
                            Logger.LogInfo($"Input languages set is empty or {shortFileName} language has not been detected. File has not been converter.");
                            return null;
                        }
                        var parser = detectedLanguage.CreateParser();
                        parser.Logger = Logger;
                        if (parser is AntlrParser antlrParser)
                        {
                            antlrParser.MemoryConsumptionMb = MemoryConsumptionMb;
                            if (parser is JavaScriptAntlrParser javaScriptAntlrParser)
                            {
                                javaScriptAntlrParser.JavaScriptType = JavaScriptType;
                            }
                        }
                        parseTree = parser.Parse(sourceCodeFile);
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
                            IParseTreeToUstConverter converter = detectedLanguage.CreateConverter();
                            converter.Logger = Logger;
                            converter.AnalyzedLanguages = AnalyzedLanguages;
                            result = converter.Convert(parseTree);

                            DumpUst(result);
                        }
                        else
                        {
                            var jsonUstSerializer = new UstJsonSerializer()
                            {
                                Logger = Logger,
                                LineColumnTextSpans = LineColumnTextSpans
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
                        workflowResult.AddResultEntity(result, true);

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }
            return result;
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

        private void DumpUst(RootUst result)
        {
            if (DumpStages.Any(stage => stage.Is(PM.Stage.Ust)))
            {
                var serializer = new UstJsonSerializer
                {
                    Logger = Logger,
                    Indented = IndentedDump,
                    IncludeTextSpans = DumpWithTextSpans,
                    IncludeCode = IncludeCodeInDump,
                    LineColumnTextSpans = LineColumnTextSpans
                };
                string json = serializer.Serialize(result);
                string name = string.IsNullOrEmpty(result.SourceCodeFile.Name) ? "" : result.SourceCodeFile.Name + ".";
                Directory.CreateDirectory(DumpDir);
                File.WriteAllText(Path.Combine(DumpDir, name + ParseTreeDumper.UstSuffix), json);
            }
        }

        protected CodeFile ReadFile(string fileName, TWorkflowResult workflowResult)
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

        protected void StartConvertPatternsTaskIfRequired(TWorkflowResult workflowResult)
        {
            if (IsAsyncPatternsConversion)
            {
                if (Stage.Is(PM.Stage.Pattern)|| Stage.IsGreaterOrEqual(PM.Stage.Match))
                {
                    convertPatternsTask = new Task(() => ConvertPatterns(workflowResult));
                    convertPatternsTask.Start();
                }
            }
        }

        protected void WaitOrConverterPatterns(TWorkflowResult result)
        {
            if (IsAsyncPatternsConversion)
            {
                convertPatternsTask.Wait();
            }
            else
            {
                ConvertPatterns(result);
            }
        }

        protected void ConvertPatterns(TWorkflowResult workflowResult)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                IEnumerable<PatternDto> patternDtos = PatternsRepository.GetAll();
                UstPatternMatcher.Patterns = PatternConverter.Convert(patternDtos);
                stopwatch.Stop();
                workflowResult.AddPatternsTime(stopwatch.ElapsedTicks);
                workflowResult.AddResultEntity(UstPatternMatcher.Patterns);
            }
            catch (Exception ex)
            {
                Logger.LogError(new ParsingException(
                    new CodeFile("") { IsPattern = true }, ex, "Patterns can not be deserialized"));
            }
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

        protected void ClearCacheIfRequired(TWorkflowResult result)
        {
            if (result.TotalProcessedFilesCount > 1)
            {
                Language antlrLanguage = BaseLanguages.FirstOrDefault(language => language.HaveAntlrParser);
                if (antlrLanguage != null)
                {
                    var antlrParser = (AntlrParser)antlrLanguage.CreateParser();
                    antlrParser.MemoryConsumptionMb = 0;
                    antlrParser.ClearCache();
                }
            }
        }
    }
}
