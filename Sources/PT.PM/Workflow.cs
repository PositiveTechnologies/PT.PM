using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Nodes;
using PT.PM.Dsl;
using PT.PM.Matching;
using PT.PM.Patterns;
using PT.PM.Patterns.Nodes;
using PT.PM.Patterns.PatternsRepository;
using PT.PM.UstPreprocessing;

namespace PT.PM
{
    public class Workflow: WorkflowBase<RootUst, Stage, WorkflowResult, PatternRootUst, MatchingResult>
    {
        public Workflow()
            : this(null, LanguageExt.AllLanguages)
        {
        }

        public Workflow(ISourceCodeRepository sourceCodeRepository, Language language,
            IPatternsRepository patternsRepository = null, Stage stage = Stage.Match)
            : this(sourceCodeRepository, new [] { language }, patternsRepository, stage)
        {
        }

        public Workflow(ISourceCodeRepository sourceCodeRepository,
            IPatternsRepository patternsRepository = null, Stage stage = Stage.Match)
            :this(sourceCodeRepository, LanguageExt.AllLanguages, patternsRepository, stage)
        {
        }

        public Workflow(ISourceCodeRepository sourceCodeRepository, IEnumerable<Language> languages,
            IPatternsRepository patternsRepository = null, Stage stage = Stage.Match)
            : base(stage, languages)
        {
            SourceCodeRepository = sourceCodeRepository;
            PatternsRepository = patternsRepository ?? new DefaultPatternRepository();
            UstPatternMatcher = new BruteForcePatternMatcher();
            IUstNodeSerializer jsonNodeSerializer = new JsonUstNodeSerializer(typeof(Ust), typeof(PatternVarDef));
            IUstNodeSerializer dslNodeSerializer = new DslProcessor();
            PatternConverter = new PatternConverter(new IUstNodeSerializer[] { jsonNodeSerializer, dslNodeSerializer });
            Stage = stage;
            ThreadCount = 1;
        }

        public override WorkflowResult Process(WorkflowResult workflowResult = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            BaseLanguages = GetBaseLanguages(AnalyzedLanguages);
            var result = workflowResult ?? new WorkflowResult(AnalyzedLanguages.ToArray(), ThreadCount, Stage, IsIncludeIntermediateResult);
            result.BaseLanguages = BaseLanguages.ToArray();

            StartConvertPatternsTaskIfRequired(result);
            if (Stage == Stage.Patterns)
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
                        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

                        foreach (string fileName in fileNames)
                        {
                            ProcessFile(fileName, result, cancellationToken);
                        }
                    }
                    else
                    {
                        var parallelOptions = new ParallelOptions
                        {
                            MaxDegreeOfParallelism = ThreadCount == 0 ? -1 : ThreadCount,
                            CancellationToken = cancellationToken
                        };

                        Parallel.ForEach(
                            fileNames,
                            parallelOptions,
                            fileName =>
                            {
                                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                                ProcessFile(fileName, result, parallelOptions.CancellationToken);
                            });
                    }
                }
                catch (OperationCanceledException)
                {
                    Logger.LogInfo("Scan has been cancelled by user");
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
                if (ust != null && Stage >= Stage.Preprocess)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    if (IsIncludePreprocessing)
                    {
                        var ustPreprocessor = new UstSimplifier() { Logger = logger };
                        stopwatch.Restart();
                        ust = ustPreprocessor.Preprocess(ust);
                        stopwatch.Stop();
                        Logger.LogInfo($"Ust of file {fileName} has been preprocessed (Elapsed: {stopwatch.Elapsed}).");
                        workflowResult.AddPreprocessTime(stopwatch.ElapsedTicks);
                        workflowResult.AddResultEntity(ust, false);

                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    if (Stage >= Stage.Match)
                    {
                        WaitOrConverterPatterns(workflowResult);

                        stopwatch.Restart();
                        IEnumerable<MatchingResult> matchingResults = UstPatternMatcher.Match(ust);
                        stopwatch.Stop();
                        Logger.LogInfo($"File {fileName} has been matched with patterns (Elapsed: {stopwatch.Elapsed}).");
                        workflowResult.AddMatchTime(stopwatch.ElapsedTicks);
                        workflowResult.AddResultEntity(matchingResults);

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            finally
            {
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
    }
}