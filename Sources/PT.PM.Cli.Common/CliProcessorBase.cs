using Newtonsoft.Json;
using PT.PM.Common;
using PT.PM.Common.SourceRepository;
using PT.PM.Common.Utils;
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
    public abstract class CliProcessorBase<TStage, TWorkflowResult, TPattern, TParameters, TRenderStage>
        where TStage : Enum
        where TWorkflowResult : WorkflowResultBase<TStage, TPattern, TRenderStage>
        where TParameters : CliParameters, new()
        where TRenderStage : Enum
    {
        public ILogger Logger { get; protected set; } = new NLogLogger();

        public TParameters Parameters { get; protected set; }

        public virtual bool ContinueWithInvalidArgs => false;

        public virtual int DefaultMaxStackSize => Utils.DefaultMaxStackSize;

        public abstract string CoreName { get; }

        public TWorkflowResult Process(string args) => Process(args.SplitArguments());

        public TWorkflowResult Process(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            TWorkflowResult result = null;

            var paramsParser = new CliParametersParser<TParameters>();
            var parseResult = paramsParser.Parse(args);
            Parameters = parseResult.Parameters;

            FillLoggerSettings();

            if (parseResult.Errors.Count == 0 || ContinueWithInvalidArgs)
            {
                result = ProcessJsonConfig(args, parseResult.Errors, parseResult.ShowHelp, parseResult.ShowVersion);
            }
            else
            {
                LogInfoAndErrors(args, parseResult.Errors, parseResult.ShowHelp, parseResult.ShowVersion);
            }

            return result;
        }

        protected virtual TWorkflowResult ProcessParameters()
        {
            TWorkflowResult result = null;

            int maxStackSize = Parameters.MaxStackSize?.ConvertToInt32(ContinueWithInvalidArgs, DefaultMaxStackSize, Logger)
                               ?? DefaultMaxStackSize;

            if (maxStackSize == 0)
            {
                result = RunWorkflow();
            }
            else
            {
                Thread thread = new Thread(() => result = RunWorkflow(), maxStackSize);
                thread.Start();
                thread.Join();
            }

            return result;
        }

        protected virtual WorkflowBase<TStage, TWorkflowResult, TPattern, TRenderStage>
            InitWorkflow()
        {
            TParameters parameters = Parameters;

            var workflow = CreateWorkflow();

            workflow.SourceRepository = CreateSourceRepository(parameters);

            if (parameters.Languages?.Length > 0)
            {
                workflow.SourceRepository.Languages = parameters.Languages.ParseLanguages();
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
            if (parameters.LogLevel != null)
            {
                Logger.LogLevel = parameters.LogLevel.ParseEnum(ContinueWithInvalidArgs,
                    CommonUtils.IsDebug ? LogLevel.Info : LogLevel.Error, Logger);
            }
            if (parameters.NoLogToFile.HasValue && nLogLogger != null)
            {
                nLogLogger.IsLogToFile = !parameters.NoLogToFile.Value;
                if (parameters.NoLogToFile.Value)
                {
                    Logger.LogInfo("File log disabled.");
                }
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
            if (parameters.DumpStages?.Length > 0)
            {
                workflow.DumpStages = new HashSet<TStage>(parameters.DumpStages.ParseEnums<TStage>(ContinueWithInvalidArgs, Logger));
            }
            if (parameters.DumpPatterns.HasValue)
            {
                workflow.IsDumpPatterns = parameters.DumpPatterns.Value;
            }
            if (parameters.RenderStages?.Length > 0)
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
            if (parameters.SerializationFormat != null)
            {
                workflow.SerializationFormat =
                    parameters.SerializationFormat.ParseEnum(ContinueWithInvalidArgs, workflow.SerializationFormat, Logger);
            }

            return workflow;
        }

        protected virtual SourceRepository CreateSourceRepository(TParameters parameters)
        {
            return RepositoryFactory
                .CreateSourceRepository(parameters.InputFileNameOrDirectory, parameters.TempDir, parameters);
        }

        protected virtual IPatternsRepository CreatePatternsRepository(TParameters parameters)
        {
            return RepositoryFactory.CreatePatternsRepository(parameters.Patterns, parameters.PatternIds, Logger);
        }

        protected abstract WorkflowBase<TStage, TWorkflowResult, TPattern, TRenderStage> CreateWorkflow();

        protected abstract void LogStatistics(TWorkflowResult workflowResult);

        private TWorkflowResult ProcessJsonConfig(string[] args, List<Exception> errors = null,
            bool showHelp = false, bool showVersion = false)
        {
            try
            {
                var parameters = Parameters ?? new TParameters();

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
                            FillLoggerSettings();
                            Logger.LogInfo($"Load settings from {configFile}...");

                            string[] lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                            foreach (string line in lines)
                            {
                                Logger.LogInfo(line);
                            }
                        }
                        catch (JsonException ex)
                        {
                            FillLoggerSettings();
                            Logger.LogError(ex);
                            Logger.LogInfo("Ignored some parameters from json");
                            error = true;
                        }
                    }
                }

                if (errors != null)
                {
                    LogInfoAndErrors(args, errors, showHelp, showVersion);
                }

                if (!error || ContinueWithInvalidArgs)
                {
                    return ProcessParameters();
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        private void FillLoggerSettings()
        {
            if (Parameters.LogsDir != null)
            {
                Logger.LogsDir = NormalizeLogsDir(Parameters.LogsDir);
            }

            if (Parameters.LogLevel != null)
            {
                LogLevel logLevel = Parameters.LogLevel.ParseEnum(true, Logger.LogLevel);
                if (logLevel == LogLevel.Off)
                {
                    Logger.LogLevel = LogLevel.Off;
                }
            }
            else
            {
                Logger.LogLevel = CommonUtils.IsDebug ? LogLevel.Info : LogLevel.Error;
            }

            if (Parameters.NoLogToFile.HasValue && Logger is NLogLogger nLogLogger)
            {
                nLogLogger.IsLogToFile = !Parameters.NoLogToFile.Value;
            }
        }

        private void LogInfoAndErrors(string[] args, List<Exception> errors, bool showHelp, bool showVersion)
        {
            if (showVersion)
            {
                Logger.LogInfo($"{CoreName} version: {Utils.GetVersionString()}");
            }

            Logger.LogInfo($"{CoreName} started at {DateTime.Now}");

            string commandLineParameters = "Command line parameters: " +
                                          (args.Length > 0 ? string.Join(" ", args) : "not defined.");
            Logger.LogInfo(commandLineParameters);

            if (errors.Count > 0)
            {
                foreach (Exception ex in errors)
                {
                    Logger.LogError(ex);
                }

                Logger.LogInfo("Some cli parameters has been ignored due to errors");
            }

            if (showHelp)
            {
                var lines = CliParametersParser<TParameters>.GenerateHelpText();

                foreach (string line in lines)
                {
                    Logger.LogInfo(line);
                }
            }
        }

        protected virtual TWorkflowResult RunWorkflow()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var workflow = InitWorkflow();
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

        protected void LogOutput(TimeSpan totalElapsed, WorkflowBase<TStage, TWorkflowResult, TPattern, TRenderStage> workflow, TWorkflowResult workflowResult)
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
            Logger.LogInfo($"{"Matches count: ",LoggerUtils.Align} {workflowResult.TotalMatchesCount} ({workflowResult.TotalSuppressedCount} suppressed)");
        }

        private void LogInfo(string[] args)
        {
            Logger.LogInfo($"{CoreName} started at {DateTime.Now}");
            Logger.LogInfo($"{CoreName} version: {Utils.GetVersionString()}");

            string commandLineArguments = "Command line parameters: " +
                                          (args.Length > 0 ? string.Join(" ", args) : "not defined.");
            Logger.LogInfo(commandLineArguments);
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
