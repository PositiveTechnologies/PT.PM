using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Dsl;
using PT.PM.Matching;
using PT.PM.Matching.Json;
using PT.PM.Matching.PatternsRepository;
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
    public class Workflow: WorkflowBase<RootUst, Stage, WorkflowResult, PatternRoot, MatchResult>
    {
        public Workflow()
            : this(null)
        {
        }

        public Workflow(SourceCodeRepository sourceCodeRepository,
            IPatternsRepository patternsRepository = null, Stage stage = Stage.Match)
            : base(stage)
        {
            SourceCodeRepository = sourceCodeRepository;
            PatternsRepository = patternsRepository ?? new DefaultPatternRepository();
            IPatternSerializer jsonNodeSerializer = new JsonPatternSerializer();
            IPatternSerializer dslNodeSerializer = new DslProcessor();
            PatternConverter = new PatternConverter(jsonNodeSerializer, dslNodeSerializer);
            Stage = stage;
            ThreadCount = 1;
        }

        public override WorkflowResult Process(WorkflowResult workflowResult = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            BaseLanguages = GetBaseLanguages(AnalyzedLanguages);
            var result = workflowResult ??
                new WorkflowResult(AnalyzedLanguages.ToArray(), ThreadCount, Stage, IsIncludeIntermediateResult);
            result.BaseLanguages = BaseLanguages.ToArray();
            result.RenderStages = RenderStages;

            if (Stage == Stage.Pattern)
            {
                ConvertPatterns(result);
            }
            else
            {
                IEnumerable<string> fileNames = SourceCodeRepository.GetFileNames();
                if (fileNames is IList<string> fileNamesList)
                {
                    result.TotalFilesCount = fileNamesList.Count;
                }
                else
                {
                    filesCountTask = Task.Factory.StartNew(() => result.TotalFilesCount = fileNames.Count());
                }

                try
                {
                    var patternMatcher = new PatternMatcher
                    {
                        Logger = Logger,
                        Patterns = ConvertPatterns(result),
                        IsIgnoreFilenameWildcards = IsIgnoreFilenameWildcards
                    };

                    if (ThreadCount == 1 || (fileNames is IList<string> && result.TotalFilesCount == 1))
                    {
                        foreach (string fileName in fileNames)
                        {
                            ProcessFileWithTimeout(fileName, patternMatcher, result, cancellationToken);
                        }
                    }
                    else
                    {
                        var parallelOptions = PrepareParallelOptions(cancellationToken);
                        Parallel.ForEach(
                            fileNames,
                            parallelOptions,
                            fileName =>
                            {
                                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                                ProcessFileWithTimeout(fileName, patternMatcher, result, parallelOptions.CancellationToken);
                            });
                    }
                }
                catch (OperationCanceledException)
                {
                    Logger.LogInfo("Scan has been cancelled");
                }
            }

            if (result.TotalProcessedFilesCount > 1)
            {
                AntlrParser.ClearCacheIfRequired();
            }

            result.ErrorCount = logger?.ErrorCount ?? 0;
            return result;
        }

        private void ProcessFileWithTimeout(string fileName, PatternMatcher patternMatcher, WorkflowResult result, CancellationToken cancellationToken)
        {
            if (FileTimeout == default(TimeSpan) && MaxStackSize == 0)
            {
                ProcessFile(fileName, patternMatcher, result, cancellationToken);
            }
            else
            {
                Thread thread = new Thread(() =>
                    ProcessFile(fileName, patternMatcher, result, cancellationToken), MaxStackSize);
                thread.Start();

                if (FileTimeout == default(TimeSpan))
                {
                    thread.Join();
                }
                else
                {
                    if (!thread.Join((int)FileTimeout.TotalMilliseconds))
                    {
                        thread.Abort();

                        Logger.LogInfo(new OperationCanceledException($"Processing of {fileName} terimated due to depleted timeout ({FileTimeout})"));
                        FinalizeProcessing(fileName, result);

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }
        }

        private void ProcessFile(string fileName, PatternMatcher patternMatcher, WorkflowResult workflowResult, CancellationToken cancellationToken = default(CancellationToken))
        {
            RootUst ust = null;
            try
            {
                Logger.LogInfo(new MessageEventArgs(MessageType.ProcessingStarted, fileName));

                ust = ReadParseAndConvert(fileName, workflowResult);
                if (ust != null && Stage >= Stage.SimplifiedUst)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    if (IsIncludePreprocessing)
                    {
                        var simplifier = new UstSimplifier() { Logger = logger };
                        stopwatch.Restart();
                        ust = simplifier.Simplify(ust);
                        stopwatch.Stop();
                        Logger.LogInfo($"Ust of file {ust.SourceCodeFile.Name} has been preprocessed (Elapsed: {stopwatch.Elapsed}).");
                        workflowResult.AddSimplifyTime(stopwatch.ElapsedTicks);
                        workflowResult.AddResultEntity(ust, false);

                        DumpUst(ust, workflowResult.SourceCodeFiles);

                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    if (Stage >= Stage.Match)
                    {
                        stopwatch.Restart();
                        IEnumerable<MatchResult> matchResults = patternMatcher.Match(ust);
                        stopwatch.Stop();
                        Logger.LogInfo($"File {ust.SourceCodeFile.Name} has been matched with patterns (Elapsed: {stopwatch.Elapsed}).");
                        workflowResult.AddMatchTime(stopwatch.ElapsedTicks);
                        workflowResult.AddResultEntity(matchResults);

                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    RenderGraphs(workflowResult);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogInfo($"{fileName} processing has been cancelled");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

            FinalizeProcessing(fileName, workflowResult);

            if (ust == null)
            {
                Logger.LogInfo(new MessageEventArgs(MessageType.ProcessingIgnored, fileName));
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        private void FinalizeProcessing(string fileName, WorkflowResult workflowResult)
        {
            AntlrParser.ClearCacheIfRequired();

            workflowResult.AddProcessedFilesCount(1);
            double progress = workflowResult.TotalFilesCount == 0
                ? workflowResult.TotalProcessedFilesCount
                : (double)workflowResult.TotalProcessedFilesCount / workflowResult.TotalFilesCount;
            Logger.LogInfo(new ProgressEventArgs(progress, fileName));
            Logger.LogInfo(new MessageEventArgs(MessageType.ProcessingCompleted, fileName));
        }

        private void RenderGraphs(WorkflowResult result)
        {
            if (result.RenderStages.Any())
            {
                var renderer = new StageRenderer(result)
                {
                    Logger = Logger,
                    DumpDir = DumpDir,
                    Stages = RenderStages,
                    RenderFormat = RenderFormat,
                    RenderDirection = RenderDirection
                };
                renderer.Render();
            }
        }
    }
}