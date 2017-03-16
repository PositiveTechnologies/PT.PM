using PT.PM.AntlrUtils;
using PT.PM.UstPreprocessing;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Matching;
using PT.PM.Patterns;
using PT.PM.Patterns.PatternsRepository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace PT.PM
{
    public abstract class CommonWorkflow : ILoggable
    {
        protected ILogger logger = DummyLogger.Instance;
        protected int maxStackSize;
        private int maxTimespan;
        private int memoryConsumptionMb;

        protected long totalReadTicks;
        protected long totalParseTicks;
        protected long totalConvertTicks;
        protected long totalPreprocessTicks;
        protected long totalMatchTicks;
        protected long totalPatternsTicks;

        protected int totalProcessedFileCount;
        protected int totalProcessedCharsCount;
        protected int totalProcessedLinesCount;

        protected long totalLexerTicks;
        protected long totalParserTicks;

        protected Language[] languages;

        public TimeSpan TotalReadTimeSpan => new TimeSpan(totalReadTicks);
        public TimeSpan TotalParseTimeSpan => new TimeSpan(totalParseTicks);
        public TimeSpan TotalConvertTimeSpan => new TimeSpan(totalConvertTicks);
        public TimeSpan TotalPreprocessTimeSpan => new TimeSpan(totalPreprocessTicks);
        public TimeSpan TotalMatchTimeSpan => new TimeSpan(totalMatchTicks);
        public TimeSpan TotalPatternsTimeSpan => new TimeSpan(totalPatternsTicks);
        public TimeSpan TotalLexerTicks => new TimeSpan(totalLexerTicks);
        public TimeSpan TotalParserTicks => new TimeSpan(totalParserTicks);

        public int TotalProcessedFileCount => totalProcessedFileCount;
        public int TotalProcessedCharsCount => totalProcessedCharsCount;
        public int TotalProcessedLinesCount => totalProcessedLinesCount;

        public ISourceCodeRepository SourceCodeRepository { get; set; }

        public IPatternsRepository PatternsRepository { get; set; }

        public Dictionary<Language, ParserConverterSet> ParserConverterSets { get; set; } = new Dictionary<Language, ParserConverterSet>();

        public IPatternConverter<CommonPatternsDataStructure> PatternConverter { get; set; }

        public IUstPatternMatcher<CommonPatternsDataStructure> UstPatternMatcher { get; set; }

        public IUstPreprocessor UstPreprocessor { get; set; } = new UstPreprocessor();

        public LanguageDetector LanguageDetector { get; set; } = new ParserLanguageDetector();

        public bool IsIncludeIntermediateResult { get; set; }

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
                if (UstPreprocessor != null)
                {
                    UstPreprocessor.Logger = Logger;
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

        public abstract void LogStatistics();

        public abstract WorkflowResult Process();

        protected long GetTotalTimeTicks()
        {
            return totalReadTicks + totalParseTicks + totalConvertTicks +
                   totalPreprocessTicks + totalMatchTicks + totalPatternsTicks;
        }

        protected ParseTree ReadAndParse(string fileName, WorkflowResult workflowResult)
        {
            ParseTree result = null;
            var stopwatch = new Stopwatch();
            string file = fileName;
            if (ContainsReadingStage)
            {
                stopwatch.Restart();
                SourceCodeFile sourceCodeFile = SourceCodeRepository.ReadFile(fileName);
                stopwatch.Stop();
                Interlocked.Add(ref totalReadTicks, stopwatch.ElapsedTicks);
                Interlocked.Add(ref totalProcessedCharsCount, sourceCodeFile.Code.Length);
                Interlocked.Add(ref totalProcessedLinesCount, TextHelper.GetLinesCount(sourceCodeFile.Code));
                Logger.LogInfo("File {0} has been read (Elapsed: {1}).", fileName, stopwatch.Elapsed.ToString());
                workflowResult.AddResultEntity(sourceCodeFile);

                file = sourceCodeFile.RelativePath;
                if (ContainsParsingStage)
                {
                    stopwatch.Restart();
                    Language? detectedLanguage = LanguageDetector.DetectIfRequired(sourceCodeFile.Name, sourceCodeFile.Code, Languages);
                    if (detectedLanguage == null)
                    {
                        Logger.LogInfo($"Input languages set is empty or {sourceCodeFile.Name} language has not been detected");
                        return result;
                    }
                    result = ParserConverterSets[(Language)detectedLanguage].Parser.Parse(sourceCodeFile);
                    stopwatch.Stop();

                    Interlocked.Add(ref totalParseTicks, stopwatch.ElapsedTicks);
                    Logger.LogInfo("File {0} has been parsed (Elapsed: {1}).", fileName, stopwatch.Elapsed.ToString());
                }

                var antlrParseTree = result as AntlrParseTree;
                if (antlrParseTree != null)
                {
                    Interlocked.Add(ref totalLexerTicks, antlrParseTree.LexerTimeSpan.Ticks);
                    Interlocked.Add(ref totalParserTicks, antlrParseTree.ParserTimeSpan.Ticks);
                }
            }
            return result;
        }

        protected abstract bool ContainsReadingStage { get; }

        protected abstract bool ContainsParsingStage { get; }
    }
}
