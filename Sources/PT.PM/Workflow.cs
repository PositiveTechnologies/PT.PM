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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PT.PM
{
    public class Workflow: WorkflowBase<RootUst, Stage, WorkflowResult, PatternRoot, MatchingResult>
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
            UstPatternMatcher = new PatternMatcher();
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

            StartConvertPatternsTaskIfRequired(result);
            if (Stage == Stage.Pattern)
            {
                WaitOrConverterPatterns(result);
            }
            else
            {
                IEnumerable<string> fileNames = SourceCodeRepository.GetFileNames();
                if (!(fileNames is IList<string>))
                {
                    filesCountTask = Task.Factory.StartNew(() => result.TotalFilesCount = fileNames.Count());
                }

                try
                {
                    if (ThreadCount == 1 || (fileNames is IList<string> fileNamesList && fileNamesList.Count == 1))
                    {
                        foreach (string fileName in fileNames)
                        {
                            ProcessFile(fileName, result, cancellationToken);
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
                                ProcessFile(fileName, result, parallelOptions.CancellationToken);
                            });
                    }
                }
                catch (OperationCanceledException)
                {
                    Logger.LogInfo("Scan has been cancelled");
                }

                /*foreach (var pair in ParserConverterSets) // TODO: cache clearint at the end.
                {
                    pair.Value?.Parser.ClearCache();
                }*/
            }

            result.ErrorCount = logger?.ErrorCount ?? 0;
            return result;
        }

        private void ProcessFile(string fileName, WorkflowResult workflowResult, CancellationToken cancellationToken = default(CancellationToken))
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

                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    if (Stage >= Stage.Match)
                    {
                        WaitOrConverterPatterns(workflowResult);

                        stopwatch.Restart();
                        IEnumerable<MatchingResult> matchingResults = UstPatternMatcher.Match(ust);
                        stopwatch.Stop();
                        Logger.LogInfo($"File {ust.SourceCodeFile.Name} has been matched with patterns (Elapsed: {stopwatch.Elapsed}).");
                        workflowResult.AddMatchTime(stopwatch.ElapsedTicks);
                        workflowResult.AddResultEntity(matchingResults);

                        cancellationToken.ThrowIfCancellationRequested();
                    }
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
            finally
            {
                string shortFileName = ust?.SourceCodeFile.Name ?? System.IO.Path.GetFileName(fileName);
                workflowResult.AddProcessedFilesCount(1);
                double progress = workflowResult.TotalFilesCount == 0
                    ? workflowResult.TotalProcessedFilesCount
                    : (double)workflowResult.TotalProcessedFilesCount / workflowResult.TotalFilesCount;
                Logger.LogInfo(new ProgressEventArgs(progress, shortFileName));
                Logger.LogInfo(new MessageEventArgs(MessageType.ProcessingCompleted, fileName));

                if (ust == null)
                {
                    Logger.LogInfo(new MessageEventArgs(MessageType.ProcessingIgnored, fileName));
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}