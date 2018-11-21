using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Nodes;
using PT.PM.Common.Utils;
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
    public class Workflow : WorkflowBase<Stage, WorkflowResult, PatternRoot, Stage>
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
                new WorkflowResult(AnalyzedLanguages.ToList(), ThreadCount, Stage);
            result.BaseLanguages = BaseLanguages.ToArray();
            result.RenderStages = RenderStages;
            result.IsFoldConstants = IsFoldConstants;

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
                List<PatternRoot> patterns = ConvertPatterns(result);

                var parallelOptions = PrepareParallelOptions(cancellationToken);
                Parallel.ForEach(
                    fileNames,
                    parallelOptions,
                    fileName =>
                    {
                        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                        var patternMatcher = new PatternMatcher
                        {
                            Logger = Logger,
                            Patterns = patterns,
                            IsIgnoreFilenameWildcards = IsIgnoreFilenameWildcards,
                            UstConstantFolder = IsFoldConstants ? new UstConstantFolder() : null
                        };

                        ProcessFileWithTimeout(fileName, patternMatcher, result, parallelOptions.CancellationToken);
                    });
            }
            catch (OperationCanceledException)
            {
                Logger.LogInfo("Scan cancelled");
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
                        if (!CommonUtils.IsCoreApp)
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
            RootUst rootUst = null;
            try
            {
                Logger.LogInfo(new MessageEventArgs(MessageType.ProcessingStarted, fileName));

                rootUst = ReadParseAndConvert(fileName, workflowResult);
                if (rootUst != null && Stage >= Stage.Match)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();

                    var matchResults = patternMatcher.Match(rootUst);

                    stopwatch.Stop();
                    Logger.LogInfo($"File {rootUst.SourceCodeFile.Name} matched with patterns {GetElapsedString(stopwatch)}.");
                    workflowResult.AddMatchTime(stopwatch.Elapsed);

                    foreach (IMatchResultBase matchResult in matchResults)
                    {
                        workflowResult.ProcessMatchResult(matchResult);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    RenderGraphs(rootUst, workflowResult.RenderStages);
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
                Logger.LogInfo(new OperationCanceledException($"Processing of {fileName} terminated due to depleted timeout ({FileTimeout})"));
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                workflowResult.AddTerminatedFilesCount(1);
                Logger.LogError(ex);
            }
            finally
            {
                double progress = workflowResult.TotalFilesCount == 0
                    ? workflowResult.TotalProcessedFilesCount
                    : (double)workflowResult.TotalProcessedFilesCount / workflowResult.TotalFilesCount;
                Logger.LogInfo(new ProgressEventArgs(progress, fileName));
                Logger.LogInfo(new MessageEventArgs(MessageType.ProcessingCompleted, fileName));

                if (rootUst == null)
                {
                    Logger.LogInfo(new MessageEventArgs(MessageType.ProcessingIgnored, fileName));
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private void RenderGraphs(RootUst rootUst, HashSet<Stage> renderStages)
        {
            if (renderStages.Any())
            {
                var renderer = new StageRenderer
                {
                    Logger = Logger,
                    DumpDir = DumpDir,
                    Stages = RenderStages,
                    RenderFormat = RenderFormat,
                    RenderDirection = RenderDirection
                };
                renderer.Render(rootUst);
            }
        }
    }
}