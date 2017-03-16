﻿using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Nodes;
using PT.PM.Dsl;
using PT.PM.Matching;
using PT.PM.Patterns;
using PT.PM.Patterns.Nodes;
using PT.PM.Patterns.PatternsRepository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PT.PM
{
    public class Workflow : CommonWorkflow
    {
        private int maxTimespan;
        private int memoryConsumptionMb;

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

        public Stage Stage { get; set; } = Stage.Match;

        public Workflow()
            :this(null, LanguageExt.AllLanguages)
        {
        }

        public Workflow(ISourceCodeRepository sourceCodeRepository, Language language,
            IPatternsRepository patternsRepository = null, Stage stage = Stage.Match)
            : this(sourceCodeRepository, language.ToFlags(), patternsRepository, stage)
        {
        }

        public Workflow(ISourceCodeRepository sourceCodeRepository, LanguageFlags languages,
            IPatternsRepository patternsRepository = null, Stage stage = Stage.Match)
        {
            SourceCodeRepository = sourceCodeRepository;
            PatternsRepository = patternsRepository ?? new DefaultPatternRepository();
            ParserConverterSets = ParserConverterBuilder.GetParserConverterSets(languages);
            UstPatternMatcher = new BruteForcePatternMatcher();
            IUstNodeSerializer jsonNodeSerializer = new JsonUstNodeSerializer(typeof(UstNode), typeof(PatternVarDef));
            IUstNodeSerializer dslNodeSerializer = new DslProcessor();
            PatternConverter = new CommonPatternConverter(new IUstNodeSerializer[] { jsonNodeSerializer, dslNodeSerializer });
            Stage = stage;
            ThreadCount = 1;
        }

        public override WorkflowResult Process()
        {
            var workflowResult = new WorkflowResult(Stage, IsIncludeIntermediateResult);

            totalReadTicks = 0;
            totalParseTicks = 0;
            totalConvertTicks = 0;
            totalPreprocessTicks = 0;
            totalMatchTicks = 0;
            totalPatternsTicks = 0;

            Task convertPatternsTask = null;
            if (Stage == Stage.Patterns || Stage >= Stage.Match)
            {
                convertPatternsTask = new Task(() =>
                {
                    try
                    {
                        var stopwatch = Stopwatch.StartNew();
                        IEnumerable<PatternDto> patternDtos = PatternsRepository.GetAll();
                        UstPatternMatcher.PatternsData = PatternConverter.Convert(patternDtos);
                        stopwatch.Stop();
                        totalPatternsTicks = stopwatch.ElapsedTicks;
                        workflowResult.AddResultEntity(UstPatternMatcher.PatternsData.Patterns);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(new ParsingException("Patterns can not be deserialized due to the error: " + ex.Message));
                    }
                });
                convertPatternsTask.Start();
            }

            int processedCount = 0;
            if (Stage == Stage.Patterns)
            {
                if (!convertPatternsTask.IsCompleted)
                {
                    convertPatternsTask.Wait();
                }
            }
            else
            {
                var fileNames = SourceCodeRepository.GetFileNames();
                totalProcessedFileCount = fileNames.Count();
                if (ThreadCount == 1)
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                    foreach (var file in fileNames)
                    {
                        ProcessFile(file, convertPatternsTask, workflowResult);
                        Logger.LogInfo(new ProgressEventArgs((double)processedCount++ / totalProcessedFileCount, file));
                    }
                }
                else
                {
                    Parallel.ForEach(
                        fileNames,
                        new ParallelOptions { MaxDegreeOfParallelism = ThreadCount == 0 ? -1 : ThreadCount },
                        fileName =>
                        {
                            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

                            ProcessFile(fileName, convertPatternsTask, workflowResult);

                            if (Logger != null)
                            {
                                Interlocked.Increment(ref processedCount);
                                var args = new ProgressEventArgs((double)processedCount / totalProcessedFileCount, fileName);
                                Logger.LogInfo(args);
                            }
                        });
                }

                foreach (var pair in ParserConverterSets)
                {
                    pair.Value?.Parser.ClearCache();
                }
            }

            workflowResult.ErrorCount = logger == null ? 0 : logger.ErrorCount;
            return workflowResult;
        }

        public override void LogStatistics()
        {
            Logger.LogInfo("{0,-22} {1}", "Files count:", TotalProcessedFileCount.ToString());
            Logger.LogInfo("{0,-22} {1}", "Chars count:", TotalProcessedCharsCount.ToString());
            Logger.LogInfo("{0,-22} {1}", "Lines count:", TotalProcessedLinesCount.ToString());
            long totalTimeTicks = GetTotalTimeTicks();
            if (totalTimeTicks > 0)
            {
                if (Stage >= Stage.Read)
                {
                    LogStageTime(Stage.Read);
                    if (Stage >= Stage.Parse)
                    {
                        LogStageTime(Stage.Parse);
                        if (Stage >= Stage.Convert)
                        {
                            LogStageTime(Stage.Convert);
                            if (Stage >= Stage.Preprocess)
                            {
                                if (UstPreprocessor != null)
                                {
                                    LogStageTime(Stage.Preprocess);
                                }
                                if (Stage >= Stage.Match)
                                {
                                    LogStageTime(Stage.Match);
                                }
                            }
                        }
                    }
                }
                if (Stage >= Stage.Match || Stage == Stage.Patterns)
                {
                    LogStageTime(Stage.Patterns);
                }
            }
        }

        private void ProcessFile(string fileName, Task convertPatternsTask, WorkflowResult workflowResult)
        {
            try
            {
                ParseTree parseTree = ReadAndParse(fileName, workflowResult);
                if (parseTree == null)
                    return;
                workflowResult.AddResultEntity(parseTree);

                if (Stage >= Stage.Convert)
                {
                    var stopwatch = Stopwatch.StartNew();
                    IParseTreeToUstConverter converter = ParserConverterSets[parseTree.SourceLanguage].Converter;
                    Ust ust = converter.Convert(parseTree);
                    stopwatch.Stop();
                    Interlocked.Add(ref totalConvertTicks, stopwatch.ElapsedTicks);
                    Logger.LogInfo("File {0} has been converted (Elapsed: {1}).", fileName, stopwatch.Elapsed.ToString());
                    workflowResult.AddResultEntity(ust, true);

                    if (Stage >= Stage.Preprocess)
                    {
                        if (UstPreprocessor != null)
                        {
                            stopwatch.Restart();
                            ust = UstPreprocessor.Preprocess(ust);
                            stopwatch.Stop();
                            Interlocked.Add(ref totalPreprocessTicks, stopwatch.ElapsedTicks);
                            Logger.LogInfo("Ust of file {0} has been preprocessed (Elapsed: {1}).", fileName, stopwatch.Elapsed.ToString());
                            workflowResult.AddResultEntity(ust, false);
                        }

                        if (Stage >= Stage.Match)
                        {
                            if (!convertPatternsTask.IsCompleted)
                            {
                                convertPatternsTask.Wait();
                            }

                            stopwatch.Restart();
                            IEnumerable<MatchingResult> matchingResults = UstPatternMatcher.Match(ust);
                            stopwatch.Stop();
                            Interlocked.Add(ref totalMatchTicks, stopwatch.ElapsedTicks);
                            Logger.LogInfo("File {0} has been matched with patterns (Elapsed: {1}).", fileName, stopwatch.Elapsed.ToString());
                            workflowResult.AddResultEntity(matchingResults);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        protected void LogStageTime(Stage stage)
        {
            long totalTimeTicks = GetTotalTimeTicks();
            long ticks = 0;
            switch (stage)
            {
                case Stage.Read:
                    ticks = totalReadTicks;
                    break;
                case Stage.Parse:
                    ticks = totalParseTicks;
                    break;
                case Stage.Convert:
                    ticks = totalConvertTicks;
                    break;
                case Stage.Preprocess:
                    ticks = totalPreprocessTicks;
                    break;
                case Stage.Match:
                    ticks = totalMatchTicks;
                    break;
                case Stage.Patterns:
                    ticks = totalPatternsTicks;
                    break;
            }
            logger.LogInfo("{0,-22} {1} {2}%",
                "Total " + stage.ToString().ToLowerInvariant() + " time:",
                new TimeSpan(ticks).ToString(), CalculatePercent(ticks, totalTimeTicks).ToString("00.00"));

            if (stage == Stage.Parse)
            {
                LogAdditionalParserInfo();
            }
        }

        protected void LogAdditionalParserInfo()
        {
            if (ParserConverterSets.Any(pair => pair.Value.Parser is AntlrParser))
            {
                logger.LogInfo("{0,-22} {1} {2}%",
                    "  ANTLR lexing time:",
                    new TimeSpan(totalLexerTicks).ToString(), CalculatePercent(totalLexerTicks, totalParseTicks).ToString("00.00"));
                logger.LogInfo("{0,-22} {1} {2}%",
                    "  ANTLR parsing time:",
                    new TimeSpan(totalParserTicks).ToString(), CalculatePercent(totalParserTicks, totalParseTicks).ToString("00.00"));
            }
        }

        protected override bool ContainsReadingStage => Stage >= Stage.Read;

        protected override bool ContainsParsingStage => Stage >= Stage.Parse;

        protected double CalculatePercent(long part, long whole)
        {
            return whole == 0 ? 0 : ((double)part / whole * 100.0);
        }
    }
}
