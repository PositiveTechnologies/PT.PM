using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Patterns;
using PT.PM.Patterns.PatternsRepository;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using System.Threading.Tasks;
using PT.PM.Matching;
using System.Threading;
using PT.PM.Common.Exceptions;

namespace PT.PM
{
    public abstract class WorkflowBase<TStage, TWorkflowResult, TPattern, TMatchingResult> : ILoggable
        where TStage : struct, IConvertible
        where TWorkflowResult : WorkflowResultBase<TStage, TPattern, TMatchingResult>
        where TPattern : PatternBase
        where TMatchingResult : MatchingResultBase<TPattern>
    {
        protected ILogger logger = DummyLogger.Instance;
        protected int maxStackSize;
        private int maxTimespan;
        private int memoryConsumptionMb;

        protected Language[] languages;

        protected StageHelper<TStage> stageHelper;

        public TStage Stage { get; set; }

        public ISourceCodeRepository SourceCodeRepository { get; set; }

        public IPatternsRepository PatternsRepository { get; set; }

        public Dictionary<Language, ParserConverterSet> ParserConverterSets { get; set; } = new Dictionary<Language, ParserConverterSet>();

        public IPatternConverter<TPattern> PatternConverter { get; set; }

        public IUstPatternMatcher<TPattern, TMatchingResult> UstPatternMatcher { get; set; }

        public LanguageDetector LanguageDetector { get; set; } = new ParserLanguageDetector();

        public bool IsIncludeIntermediateResult { get; set; }

        public bool IsIncludePreprocessing { get; set; } = true;

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
                foreach (var languageParser in ParserConverterSets)
                {
                    if (languageParser.Value.Parser != null)
                    {
                        languageParser.Value.Parser.Logger = logger;
                    }
                    if (languageParser.Value.Converter != null)
                    {
                        languageParser.Value.Converter.Logger = logger;
                    }
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

        public int MaxStackSize
        {
            get
            {
                return maxStackSize;
            }
            set
            {
                maxStackSize = value;
                foreach (var languageParser in ParserConverterSets)
                {
                    var antlrParser = languageParser.Value.Parser as AntlrParser;
                    if (antlrParser != null)
                    {
                        antlrParser.MaxStackSize = maxStackSize;
                    }
                }
            }
        }

        public Language[] Languages
        {
            get
            {
                if (languages == null)
                {
                    languages = ParserConverterSets.Keys.Select(key => key).ToArray();
                }
                return languages;
            }
        }

        public int MaxTimespan
        {
            get
            {
                return maxTimespan;
            }
            set
            {
                maxTimespan = value;
                foreach (var pair in ParserConverterSets)
                {
                    var antlrParser = pair.Value?.Parser as AntlrParser;
                    if (antlrParser != null)
                    {
                        antlrParser.MaxTimespan = maxTimespan;
                    }
                }
            }
        }

        public int MemoryConsumptionMb
        {
            get
            {
                return memoryConsumptionMb;
            }
            set
            {
                memoryConsumptionMb = value;
                foreach (var pair in ParserConverterSets)
                {
                    var antlrParser = pair.Value?.Parser as AntlrParser;
                    if (antlrParser != null)
                    {
                        antlrParser.MemoryConsumptionMb = memoryConsumptionMb;
                    }
                }
            }
        }

        public abstract TWorkflowResult Process(TWorkflowResult workflowResult = null, CancellationToken cancellationToken = default(CancellationToken));

        public WorkflowBase(TStage stage)
        {
            Stage = stage;
            stageHelper = new StageHelper<TStage>(stage);
        }

        protected ParseTree ReadAndParse(string fileName, TWorkflowResult workflowResult, CancellationToken cancellationToken = default(CancellationToken))
        {
            ParseTree result = null;
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
                    Language? detectedLanguage = LanguageDetector.DetectIfRequired(sourceCodeFile.Name, sourceCodeFile.Code, Languages);
                    if (detectedLanguage == null)
                    {
                        Logger.LogInfo($"Input languages set is empty or {sourceCodeFile.Name} language has not been detected. File has not been converter.");
                        return null;
                    }
                    result = ParserConverterSets[(Language)detectedLanguage].Parser.Parse(sourceCodeFile);
                    stopwatch.Stop();
                    Logger.LogInfo($"File {fileName} has been parsed (Elapsed: {stopwatch.Elapsed}).");
                    workflowResult.AddParseTime(stopwatch.ElapsedTicks);

                    var antlrParseTree = result as AntlrParseTree;
                    if (antlrParseTree != null)
                    {
                        workflowResult.AddLexerTime(antlrParseTree.LexerTimeSpan.Ticks);
                        workflowResult.AddParserTicks(antlrParseTree.ParserTimeSpan.Ticks);
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            return result;
        }

        protected Task GetConvertPatternsTask(TWorkflowResult workflowResult)
        {
            Task convertPatternsTask = null;
            if (stageHelper.IsPatterns || stageHelper.IsContainsMatch)
            {
                convertPatternsTask = new Task(() =>
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
                });
                convertPatternsTask.Start();
            }

            return convertPatternsTask;
        }
    }
}
