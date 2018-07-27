using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PT.PM
{
    public class Workflow: WorkflowBase<RootUst, Stage, WorkflowResult, PatternRoot, MatchResult, Stage>
    {
        public Workflow()
            : this(null)
        {
        }

        public Workflow(SourceCodeRepository sourceCodeRepository = null,
            IPatternsRepository patternsRepository = null, Stage stage = Stage.Match)
            : base(stage)
        {
            LogsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PT.PM");
            DumpDir = LogsDir;
            TempDir = Path.Combine(Path.GetTempPath(), "PT.PM");
            SourceCodeRepository = sourceCodeRepository;
            PatternsRepository = patternsRepository ?? new DefaultPatternRepository();
            IPatternSerializer jsonNodeSerializer = new JsonPatternSerializer();
            IPatternSerializer dslNodeSerializer = new DslProcessor();
            PatternConverter = new PatternConverter(jsonNodeSerializer, dslNodeSerializer);
            Stage = stage;
        }

        public override WorkflowResult Process(WorkflowResult workflowResult = null,
            CancellationToken cancellationToken = default)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            BaseLanguages = GetBaseLanguages(AnalyzedLanguages);
            var result = workflowResult ??
                new WorkflowResult(AnalyzedLanguages.ToArray(), ThreadCount, Stage);
            result.BaseLanguages = BaseLanguages.ToArray();
            result.RenderStages = RenderStages;
            result.IsSimplifyUst = IsSimplifyUst;

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
            catch (OperationCanceledException)
            {
                Logger.LogInfo("Scan cancelled");
            }

            if (result.TotalProcessedFilesCount > 1)
            {
                AntlrParser.ClearCacheIfRequired();
            }

            DumpJsonOutput(result);
            result.ErrorCount = logger?.ErrorCount ?? 0;

            result.RootPath = SourceCodeRepository.RootPath;

            return result;
        }

        private void ProcessFileWithTimeout(string fileName, PatternMatcher patternMatcher, WorkflowResult result, CancellationToken cancellationToken)
        {
            if (FileTimeout == default && MaxStackSize == 0)
            {
                ProcessFile(fileName, patternMatcher, result, cancellationToken);
            }
            else
            {
                Thread thread = new Thread(() =>
                    ProcessFile(fileName, patternMatcher, result, cancellationToken), MaxStackSize)
                {
                    IsBackground = true
                };
                thread.Start();

                if (FileTimeout == default)
                {
                    thread.Join();
                }
                else
                {
                    if (!thread.Join(FileTimeout))
                    {
                        if (CommonUtils.IsSupportThreadAbort)
                        {
                            thread.Abort();
                        }
                        thread.Join((int)FileTimeout.TotalMilliseconds / 4);
                    }
                }
            }
        }

        private void ProcessFile(string fileName, PatternMatcher patternMatcher, WorkflowResult workflowResult, CancellationToken cancellationToken = default)
        {
            RootUst ust = null;
            try
            {
                Logger.LogInfo(new MessageEventArgs(MessageType.ProcessingStarted, fileName));

                ust = ReadParseAndConvert(fileName, workflowResult);
                if (ust != null && Stage >= Stage.Match)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();

                    IEnumerable<MatchResult> matchResults = patternMatcher.Match(ust);

                    stopwatch.Stop();
                    Logger.LogInfo($"File {ust.SourceCodeFile.Name} matched with patterns {GetElapsedString(stopwatch)}.");
                    workflowResult.AddMatchTime(stopwatch.Elapsed);
                    workflowResult.AddResultEntity(matchResults);

                    cancellationToken.ThrowIfCancellationRequested();

                    RenderGraphs(workflowResult);
                }
            }
            catch (OperationCanceledException)
            {
                workflowResult.AddTerminatedFilesCount(1);
                Logger.LogInfo($"{fileName} processing cancelled");
            }
            catch (ThreadAbortException)
            {
                workflowResult.AddTerminatedFilesCount(1);
                Logger.LogInfo(new OperationCanceledException($"Processing of {fileName} terimated due to depleted timeout ({FileTimeout})"));
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                workflowResult.AddTerminatedFilesCount(1);
                Logger.LogError(ex);
            }
            finally
            {
                AntlrParser.ClearCacheIfRequired();

                workflowResult.AddProcessedFilesCount(1);
                double progress = workflowResult.TotalFilesCount == 0
                    ? workflowResult.TotalProcessedFilesCount
                    : (double)workflowResult.TotalProcessedFilesCount / workflowResult.TotalFilesCount;
                Logger.LogInfo(new ProgressEventArgs(progress, fileName));
                Logger.LogInfo(new MessageEventArgs(MessageType.ProcessingCompleted, fileName));

                if (ust == null)
                {
                    Logger.LogInfo(new MessageEventArgs(MessageType.ProcessingIgnored, fileName));
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
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