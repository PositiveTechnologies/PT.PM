using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.JavaScriptParseTreeUst;
using PT.PM.Matching;
using PT.PM.Matching.PatternsRepository;
using PT.PM.Patterns;
using PT.PM.Patterns.PatternsRepository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PT.PM
{
    public abstract class WorkflowBase<TInputGraph, TStage, TWorkflowResult, TPattern, TMatchingResult> : ILoggable
        where TStage : struct, IConvertible
        where TWorkflowResult : WorkflowResultBase<TStage, TPattern, TMatchingResult>
        where TMatchingResult : MatchingResultBase<TPattern>
    {
        protected ILogger logger = DummyLogger.Instance;
        protected int maxStackSize;
        protected Task filesCountTask;
        protected Task convertPatternsTask;

        protected Language[] languages;

        protected StageHelper<TStage> stageHelper;

        public TStage Stage { get; set; }

        public ISourceCodeRepository SourceCodeRepository { get; set; }

        public IPatternsRepository PatternsRepository { get; set; }

        public IPatternConverter<TPattern> PatternConverter { get; set; }

        public IUstPatternMatcher<TInputGraph, TPattern, TMatchingResult> UstPatternMatcher { get; set; }

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

        public int MaxStackSize { get; set; } = 0;

        public int MaxTimespan { get; set; } = 0;

        public long MemoryConsumptionMb { get; set; } = 300;

        public HashSet<Language> AnalyzedLanguages { get; set; } = new HashSet<Language>(LanguageExt.AllLanguages);

        public HashSet<Language> BaseLanguages { get; set; }

        public abstract TWorkflowResult Process(TWorkflowResult workflowResult = null, CancellationToken cancellationToken = default(CancellationToken));

        public WorkflowBase(TStage stage, IEnumerable<Language> languages)
        {
            AnalyzedLanguages = new HashSet<Language>(languages);
            Stage = stage;
            stageHelper = new StageHelper<TStage>(stage);
        }

        protected RootUst ReadParseAndConvert(string fileName, TWorkflowResult workflowResult, CancellationToken cancellationToken = default(CancellationToken))
        {
            RootUst result = null;
            var stopwatch = new Stopwatch();
            string file = fileName;
            if (stageHelper.IsContainsRead)
            {
                if (SourceCodeRepository.IsFileIgnored(fileName))
                {
                    Logger.LogInfo($"File {fileName} has not been read.");
                    return null;
                }

                stopwatch.Restart();
                SourceCodeFile sourceCodeFile = SourceCodeRepository.ReadFile(fileName);
                stopwatch.Stop();

                Logger.LogInfo($"File {fileName} has been read (Elapsed: {stopwatch.Elapsed}).");

                workflowResult.AddProcessedCharsCount(sourceCodeFile.Code.Length);
                workflowResult.AddProcessedLinesCount(TextHelper.GetLinesCount(sourceCodeFile.Code));
                workflowResult.AddReadTime(stopwatch.ElapsedTicks);
                workflowResult.AddResultEntity(sourceCodeFile);

                cancellationToken.ThrowIfCancellationRequested();

                file = sourceCodeFile.RelativePath;
                if (stageHelper.IsContainsParse)
                {
                    stopwatch.Restart();
                    Language? detectedLanguage = LanguageDetector.DetectIfRequired(sourceCodeFile.Name, sourceCodeFile.Code, workflowResult.BaseLanguages.ToArray());
                    if (detectedLanguage == null)
                    {
                        Logger.LogInfo($"Input languages set is empty or {sourceCodeFile.Name} language has not been detected. File has not been converter.");
                        return null;
                    }
                    var parser = ParserConverterFactory.CreateParser((Language)detectedLanguage);
                    parser.Logger = Logger;
                    if (parser is AntlrParser antlrParser)
                    {
                        antlrParser.MemoryConsumptionMb = MemoryConsumptionMb;
                        antlrParser.MaxTimespan = MaxTimespan;
                        antlrParser.MaxStackSize = MaxStackSize;
                        if (parser is JavaScriptAntlrParser javaScriptAntlrParser)
                        {
                            javaScriptAntlrParser.JavaScriptType = JavaScriptType;
                        }
                    }
                    ParseTree parseTree = parser.Parse(sourceCodeFile);
                    stopwatch.Stop();
                    Logger.LogInfo($"File {fileName} has been parsed (Elapsed: {stopwatch.Elapsed}).");
                    workflowResult.AddParseTime(stopwatch.ElapsedTicks);
                    workflowResult.AddResultEntity(parseTree);

                    var antlrParseTree = parseTree as AntlrParseTree;
                    if (antlrParseTree != null)
                    {
                        workflowResult.AddLexerTime(antlrParseTree.LexerTimeSpan.Ticks);
                        workflowResult.AddParserTicks(antlrParseTree.ParserTimeSpan.Ticks);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    if (stageHelper.IsContainsConvert)
                    {
                        stopwatch.Reset();

                        var converter = ParserConverterFactory.CreateConverter(parseTree.SourceLanguage);
                        converter.Logger = Logger;
                        converter.AnalyzedLanguages = AnalyzedLanguages;
                        result = converter.Convert(parseTree);
                        stopwatch.Stop();
                        Logger.LogInfo($"File {fileName} has been converted (Elapsed: {stopwatch.Elapsed}).");
                        workflowResult.AddConvertTime(stopwatch.ElapsedTicks);
                        workflowResult.AddResultEntity(result, true);

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }
            return result;
        }

        protected void StartConvertPatternsTaskIfRequired(TWorkflowResult workflowResult)
        {
            if (IsAsyncPatternsConversion)
            {
                if (stageHelper.IsPatterns || stageHelper.IsContainsMatch)
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
                Logger.LogError(new ParsingException("", ex, "Patterns can not be deserialized") { IsPattern = true });
            }
        }

        protected static HashSet<Language> GetBaseLanguages(HashSet<Language> analyzedLanguages)
        {
            HashSet<Language> result = new HashSet<Language>();
            foreach (Language language in analyzedLanguages)
            {
                result.Add(language);
                LanguageInfo superLangInfo = LanguageExt.LanguageInfos[language];
                do
                {
                    superLangInfo = LanguageExt.LanguageInfos.FirstOrDefault(l => l.Value.Sublanguages.Contains(superLangInfo.Language)).Value;
                    if (superLangInfo != null)
                    {
                        result.Add(superLangInfo.Language);
                    }
                }
                while (superLangInfo != null);
            }
            return result;
        }
    }
}
