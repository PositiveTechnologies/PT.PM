using PT.PM.AntlrUtils;
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
                        workflowResult.AddStageTime(Stage.Patterns, stopwatch.ElapsedTicks);
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
                workflowResult.AddProcessedFilesCount(fileNames.Count());
                if (ThreadCount == 1)
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                    foreach (var file in fileNames)
                    {
                        ProcessFile(file, convertPatternsTask, workflowResult);
                        Logger.LogInfo(new ProgressEventArgs((double)processedCount++ / workflowResult.TotalProcessedFilesCount, file));
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
                                var args = new ProgressEventArgs((double)processedCount / workflowResult.TotalProcessedFilesCount, fileName);
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
                    Logger.LogInfo("File {0} has been converted (Elapsed: {1}).", fileName, stopwatch.Elapsed.ToString());
                    workflowResult.AddStageTime(Stage.Convert, stopwatch.ElapsedTicks);
                    workflowResult.AddResultEntity(ust, true);

                    if (Stage >= Stage.Preprocess)
                    {
                        if (UstPreprocessor != null)
                        {
                            stopwatch.Restart();
                            ust = UstPreprocessor.Preprocess(ust);
                            stopwatch.Stop();
                            Logger.LogInfo("Ust of file {0} has been preprocessed (Elapsed: {1}).", fileName, stopwatch.Elapsed.ToString());
                            workflowResult.AddStageTime(Stage.Preprocess, stopwatch.ElapsedTicks);
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
                            Logger.LogInfo("File {0} has been matched with patterns (Elapsed: {1}).", fileName, stopwatch.Elapsed.ToString());
                            workflowResult.AddStageTime(Stage.Match, stopwatch.ElapsedTicks);
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

        protected override bool ContainsReadingStage => Stage >= Stage.Read;

        protected override bool ContainsParsingStage => Stage >= Stage.Parse;
    }
}
