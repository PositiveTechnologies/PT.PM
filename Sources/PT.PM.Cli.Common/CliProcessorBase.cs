using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Utils;
using PT.PM.Matching;
using PT.PM.Matching.PatternsRepository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace PT.PM.Cli.Common
{
    public abstract class CliProcessorBase<TInputGraph, TStage, TWorkflowResult, TPattern, TMatchResult, TParameters, TRenderStage>
        where TStage : Enum
        where TWorkflowResult : WorkflowResultBase<TStage, TPattern, TMatchResult, TRenderStage>
        where TMatchResult : MatchResultBase<TPattern>
        where TParameters : CliParameters, new()
        where TRenderStage : Enum
    {
        public ILogger Logger { get; protected set; } = new NLogLogger();

        public virtual bool ContinueWithInvalidArgs => false;

        public virtual bool StopIfDebuggerAttached => true;

        public virtual int DefaultMaxStackSize => Utils.DefaultMaxStackSize;

        public abstract string CoreName { get; }

        public TWorkflowResult Process(string args) => Process(args.SplitArguments());

        public TWorkflowResult Process(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            var paramsNormalizer = new CliParametersNormalizer<TParameters>()
            {
                Logger = Logger
            };
            bool success = paramsNormalizer.Normalize(args, out string[] outArgs);

            var parser = new Parser(config =>
            {
                config.IgnoreUnknownArguments = ContinueWithInvalidArgs;
                config.CaseInsensitiveEnumValues = true;
            });

            ParserResult<TParameters> parserResult = parser.ParseArguments<TParameters>(outArgs);

            TWorkflowResult result = null;

            if (success || ContinueWithInvalidArgs)
            {
                parserResult.WithParsed(
                    parameters =>
                    {
                        result = ProcessJsonConfig(outArgs, parameters);
                    })
                    .WithNotParsed(errors =>
                    {
                        if (ContinueWithInvalidArgs)
                        {
                            result = ProcessJsonConfig(outArgs, null, errors);
                        }
                        else
                        {
                            LogInfoAndErrors(outArgs, errors);
                        }
                    });
            }

#if DEBUG
            if (StopIfDebuggerAttached && Debugger.IsAttached)
            {
                Console.WriteLine("Press Enter to exit");
                Console.ReadLine();
            }
#endif

            return result;
        }

        protected virtual TWorkflowResult ProcessParameters(TParameters parameters)
        {
            TWorkflowResult result = null;

            int maxStackSize = parameters.MaxStackSize.HasValue
                    ? parameters.MaxStackSize.Value.ConvertToInt32(ContinueWithInvalidArgs, DefaultMaxStackSize, Logger)
                    : DefaultMaxStackSize;

            if (maxStackSize == 0)
            {
                result = RunWorkflow(parameters);
            }
            else
            {
                Thread thread = new Thread(() => result = RunWorkflow(parameters), maxStackSize);
                thread.Start();
                thread.Join();
            }

            return result;
        }

        protected virtual WorkflowBase<TInputGraph, TStage, TWorkflowResult, TPattern, TMatchResult, TRenderStage>
            InitWorkflow(TParameters parameters)
        {
            var workflow = CreateWorkflow(parameters);

            workflow.SourceCodeRepository = CreateSourceCodeRepository(parameters);

            if (parameters.Languages?.Count() > 0)
            {
                workflow.SourceCodeRepository.Languages = parameters.Languages.ParseLanguages();
            }

            workflow.PatternsRepository = CreatePatternsRepository(parameters);
            workflow.Logger = Logger;
            NLogLogger nLogLogger = Logger as NLogLogger;

            if (parameters.Stage != null)
            {
                workflow.Stage = parameters.Stage.ParseEnum(ContinueWithInvalidArgs, workflow.Stage, Logger);
            }

            if (parameters.ThreadCount.HasValue)
            {
                workflow.ThreadCount = parameters.ThreadCount.Value;
            }
            if (parameters.NotFoldConstants.HasValue)
            {
                workflow.IsFoldConstants = !parameters.NotFoldConstants.Value;
            }
            if (parameters.MaxStackSize.HasValue)
            {
                workflow.MaxStackSize = parameters.MaxStackSize.Value.ConvertToInt32(ContinueWithInvalidArgs, workflow.MaxStackSize, Logger);
            }
            if (parameters.Memory.HasValue)
            {
                workflow.MemoryConsumptionMb = parameters.Memory.Value.ConvertToInt32(ContinueWithInvalidArgs, workflow.MemoryConsumptionMb, Logger);
            }
            if (parameters.FileTimeout.HasValue)
            {
                workflow.FileTimeout = TimeSpan.FromSeconds(parameters.FileTimeout.Value);
            }
            if (parameters.LogsDir != null)
            {
                workflow.LogsDir = NormalizeLogsDir(parameters.LogsDir);
                workflow.DumpDir = NormalizeLogsDir(parameters.LogsDir);
            }
            if (parameters.Silent != null && nLogLogger != null)
            {
                if (parameters.Silent.Value)
                {
                    Logger.LogInfo("Silent mode.");
                }
                nLogLogger.IsLogToConsole = !parameters.Silent.Value;
            }
            if (parameters.NoLog != null && nLogLogger != null)
            {
                if (parameters.NoLog.Value)
                {
                    Logger.LogInfo("File log disabled.");
                }
                nLogLogger.IsLogToFile = false;
            }
            if (parameters.TempDir != null)
            {
                workflow.TempDir = NormalizeLogsDir(parameters.TempDir);
            }
            if (parameters.IndentedDump.HasValue)
            {
                workflow.IndentedDump = parameters.IndentedDump.Value;
            }
            if (parameters.NotIncludeTextSpansInDump.HasValue)
            {
                workflow.DumpWithTextSpans = !parameters.NotIncludeTextSpansInDump.Value;
            }
            if (parameters.LineColumnTextSpans.HasValue)
            {
                workflow.LineColumnTextSpans = parameters.LineColumnTextSpans.Value;
            }
            if (parameters.IncludeCodeInDump.HasValue)
            {
                workflow.IncludeCodeInDump = parameters.IncludeCodeInDump.Value;
            }
            if (parameters.StrictJson.HasValue)
            {
                workflow.StrictJson = parameters.StrictJson.Value;
            }
            if (parameters.IsDumpJsonOutput.HasValue)
            {
                workflow.IsDumpJsonOutput = parameters.IsDumpJsonOutput.Value;
            }
            if (parameters.StartStage != null)
            {
                workflow.SourceCodeRepository.LoadJson = IsLoadJson(parameters.StartStage);
                workflow.StartStage = parameters.StartStage.ParseEnum(ContinueWithInvalidArgs, workflow.StartStage, Logger);
            }
            if (parameters.DumpStages?.Count() > 0)
            {
                workflow.DumpStages = new HashSet<TStage>(parameters.DumpStages.ParseEnums<TStage>(ContinueWithInvalidArgs, Logger));
            }
            if (parameters.DumpPatterns.HasValue)
            {
                workflow.IsDumpPatterns = parameters.DumpPatterns.Value;
            }
            if (parameters.RenderStages?.Count() > 0)
            {
                workflow.RenderStages = new HashSet<TRenderStage>(parameters.RenderStages.ParseEnums<TRenderStage>(ContinueWithInvalidArgs, Logger));
            }
            if (parameters.RenderFormat != null)
            {
                workflow.RenderFormat = parameters.RenderFormat.ParseEnum(ContinueWithInvalidArgs, workflow.RenderFormat, Logger);
            }
            if (parameters.RenderDirection != null)
            {
                workflow.RenderDirection = parameters.RenderDirection.ParseEnum(ContinueWithInvalidArgs, workflow.RenderDirection, Logger);
            }

            return workflow;
        }

        protected virtual SourceCodeRepository CreateSourceCodeRepository(TParameters parameters)
        {
            return RepositoryFactory
                .CreateSourceCodeRepository(parameters.InputFileNameOrDirectory, parameters.TempDir);
        }

        protected virtual IPatternsRepository CreatePatternsRepository(TParameters parameters)
        {
            return RepositoryFactory.CreatePatternsRepository(parameters.Patterns, parameters.PatternIds, Logger);
        }

        protected abstract WorkflowBase<TInputGraph, TStage, TWorkflowResult, TPattern, TMatchResult, TRenderStage> CreateWorkflow(TParameters parameters);

        protected abstract void LogStatistics(TWorkflowResult workflowResult);

        protected abstract bool IsLoadJson(string startStageString);

        private TWorkflowResult ProcessJsonConfig(string[] args, TParameters parameters, IEnumerable<Error> errors = null)
        {
            try
            {
                if (parameters != null)
                {
                    FillLoggerSettings(parameters);
                }
                else
                {
                    parameters = new TParameters();
                }

                bool error = false;
                string configFile = FileExt.Exists("config.json") ? "config.json" : parameters.ConfigFile;
                if (!string.IsNullOrEmpty(configFile))
                {
                    string content = null;
                    try
                    {
                        content = FileExt.ReadAllText(configFile);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex);
                        error = true;
                    }

                    if (content != null)
                    {
                        try
                        {
                            var settings = new JsonSerializerSettings
                            {
                                MissingMemberHandling = ContinueWithInvalidArgs
                                    ? MissingMemberHandling.Ignore
                                    : MissingMemberHandling.Error
                            };
                            JsonConvert.PopulateObject(content, parameters, settings);
                            FillLoggerSettings(parameters);
                            Logger.LogInfo($"Load settings from {configFile}...");
                            SplitOnLinesAndLog(content);
                        }
                        catch (JsonException ex)
                        {
                            FillLoggerSettings(parameters);
                            Logger.LogError(ex);
                            Logger.LogInfo("Ignored some parameters from json");
                            error = true;
                        }
                    }
                }

                LogInfoAndErrors(args, errors);
                if (errors != null)
                {
                    Logger.LogInfo("Ignored some cli parameters");
                }

                if (!error || ContinueWithInvalidArgs)
                {
                    return ProcessParameters(parameters);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);

                return null;
            }
        }

        private void FillLoggerSettings(TParameters parameters)
        {
            if (parameters.LogsDir != null)
            {
                Logger.LogsDir = NormalizeLogsDir(parameters.LogsDir);
            }
            Logger.IsLogDebugs = parameters.IsLogDebugs ?? false;
        }

        protected virtual TWorkflowResult RunWorkflow(TParameters parameters)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var workflow = InitWorkflow(parameters);
                TWorkflowResult workflowResult = workflow.Process();
                stopwatch.Stop();

                LogOutput(stopwatch.Elapsed, workflow, workflowResult);

                return workflowResult;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);

                return null;
            }
        }

        protected void LogOutput(TimeSpan totalElapsed, WorkflowBase<TInputGraph, TStage, TWorkflowResult, TPattern, TMatchResult, TRenderStage> workflow, TWorkflowResult workflowResult)
        {
            Logger.LogInfo("");

            LogExtraVersion();
            LoggerUtils.LogSystemInfo(Logger, CoreName);

            Logger.LogInfo("");

            if (!string.IsNullOrEmpty(workflowResult.RootPath))
            {
                Logger.LogInfo($"{"Scan path or file:",LoggerUtils.Align} {workflowResult.RootPath}");
            }
            string threadCountString = workflowResult.ThreadCount <= 0 ?
                "default" : workflowResult.ThreadCount.ToString();
            Logger.LogInfo($"{"Thread count:",LoggerUtils.Align} {threadCountString}");
            Logger.LogInfo($"{"Finish date:",LoggerUtils.Align} {DateTime.Now}");

            if (!workflow.Stage.Is(Stage.Match))
            {
                Logger.LogInfo($"{"Stage: ",LoggerUtils.Align} {workflow.Stage}");
            }

            LogMatchesCount(workflowResult);

            if (workflowResult.ErrorCount > 0)
            {
                Logger.LogInfo($"{"Errors count: ",LoggerUtils.Align} {workflowResult.ErrorCount}");
            }

            LogStatistics(workflowResult);

            Logger.LogInfo("");
            Logger.LogInfo($"{"Time elapsed:",LoggerUtils.Align} {totalElapsed.Format()}");
        }

        protected virtual void LogExtraVersion()
        {
        }

        protected virtual void LogMatchesCount(TWorkflowResult workflowResult)
        {
            int matchedResultCount = 0;
            int suppressedCount = 0;

            foreach (IMatchResultBase matchResult in workflowResult.MatchResults)
            {
                GetMatchesCount(matchResult, ref matchedResultCount, ref suppressedCount);
            }

            Logger.LogInfo($"{"Matches count: ",LoggerUtils.Align} {matchedResultCount} ({suppressedCount} suppressed)");
        }

        protected static void GetMatchesCount(IMatchResultBase matchResult, ref int matchedResultCount, ref int suppressedCount)
        {
            int patternsCount = ExtractKeys(matchResult.PatternKey).Length;
            if (patternsCount == 0)
            {
                patternsCount = 1;
            }

            matchedResultCount += patternsCount;
            if (matchResult.Suppressed)
            {
                suppressedCount += patternsCount;
            }
        }

        protected static string[] ExtractKeys(string keys)
        {
            return keys.Split(PatternRoot.KeySeparators, StringSplitOptions.RemoveEmptyEntries);
        }

        private void LogInfoAndErrors(string[] args, IEnumerable<Error> errors)
        {
            Logger.LogInfo($"{CoreName} started at {DateTime.Now}");

            if (errors == null || errors.FirstOrDefault() is VersionRequestedError)
            {
                Logger.LogInfo($"{CoreName} version: {Utils.GetVersionString()}");
            }

            string commandLineArguments = "Command line arguments: " +
                    (args.Length > 0 ? string.Join(" ", args) : "not defined.");
            Logger.LogInfo(commandLineArguments);

            if (errors != null)
            {
                LogParseErrors(errors);
            }
        }

        private void LogParseErrors(IEnumerable<Error> errors)
        {
            foreach (Error error in errors)
            {
                if (error is HelpRequestedError || error is VersionRequestedError)
                {
                    continue;
                }

                string parameter = "";
                if (error is NamedError namedError)
                {
                    parameter = $"({namedError.NameInfo.NameText})";
                }
                else if (error is TokenError tokenError)
                {
                    parameter = $"({tokenError.Token})";
                }
                Logger.LogError(new Exception($"Launch Parameter {parameter} Error: {error.Tag}"));
            }

            if (!(errors.First() is VersionRequestedError))
            {
                var paramsParseResult = new Parser().ParseArguments<TParameters>(new string[] { "--help" });
                string paramsInfo = HelpText.AutoBuild(paramsParseResult, 100);
                SplitOnLinesAndLog(paramsInfo);
            }
        }

        private void SplitOnLinesAndLog(string str)
        {
            string[] lines = str.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                Logger.LogInfo(line);
            }
        }

        private string NormalizeLogsDir(string logsDir)
        {
            string shortCoreName = CoreName.Split('.').Last().ToLowerInvariant();

            if (!Path.GetFileName(logsDir).ToLowerInvariant().Contains(shortCoreName))
            {
                return Path.Combine(logsDir, CoreName);
            }

            return logsDir;
        }
    }
}
