using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Matching;
using PT.PM.Matching.PatternsRepository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace PT.PM.Cli.Common
{
    public abstract class CliProcessorBase<TInputGraph, TStage, TWorkflowResult, TPattern, TMatchResult, TParameters>
        where TStage : struct, IConvertible
        where TWorkflowResult : WorkflowResultBase<TStage, TPattern, TMatchResult>
        where TMatchResult : MatchResultBase<TPattern>
        where TParameters : CliParameters, new()
    {
        public ILogger Logger { get; protected set; } = new ConsoleFileLogger();

        public virtual bool ContinueWithInvalidArgs => false;

        public virtual bool StopIfDebuggerAttached => true;

        public abstract string CoreName { get; }

        public bool Process(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            Logger.IsLogErrors = true;

            ParserResult<TParameters> parserResult = new Parser().ParseArguments<TParameters>(args);

            bool result = true;

            parserResult.WithParsed(
                parameters =>
                {
                    result = ProcessJsonConfig(args, parameters);
                })
                .WithNotParsed(errors =>
                {
                    LogParseErrors(errors);

                    if (ContinueWithInvalidArgs)
                    {
                        Logger.LogInfo("Ignore cli parameters, use default");
                        var parameters = new TParameters();
                        result = ProcessJsonConfig(args, parameters);
                    }
                    else
                    {
                        result = false;
                    }
                });

#if DEBUG
            if (StopIfDebuggerAttached && Debugger.IsAttached)
            {
                Console.WriteLine("Press Enter to exit");
                Console.ReadLine();
            }
#endif

            return result;
        }

        protected bool ProcessJsonConfig(string[] args, TParameters parameters)
        {
            bool result = false;
            bool populateResult = PopulateParamsFromJsonConfig(parameters);

            if (populateResult || ContinueWithInvalidArgs)
            {
                if (!populateResult)
                {
                    Logger.LogInfo("Ignore parameters from json, use default");
                }

                FillLoggerSettings(args, parameters);

                result = ProcessParameters(parameters);
            }

            return result;
        }

        protected virtual bool ProcessParameters(TParameters parameters)
        {
            bool result = false;

            if (!parameters.MaxStackSize.HasValue || parameters.MaxStackSize == 0)
            {
                result = RunWorkflow(parameters);
            }
            else
            {
                Thread thread = new Thread(() => result = RunWorkflow(parameters), parameters.MaxStackSize.Value);
                thread.Start();
                thread.Join();
            }

            return result;
        }

        protected bool RunWorkflow(TParameters parameters)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var workflow = InitWorkflow(parameters);
                TWorkflowResult workflowResult = workflow.Process();
                stopwatch.Stop();

                Logger.LogInfo($"Stage: {workflow.Stage}");
                if (!workflow.Stage.Is(Stage.Pattern))
                {
                    Logger.LogInfo("Scan completed.");
                    int matchedResultCount = workflowResult.MatchResults.Count();
                    if (workflow.Stage.Is(Stage.Match) && matchedResultCount > 0)
                    {
                        Logger.LogInfo($"{"Matches count: ",WorkflowLoggerHelper.Align} {matchedResultCount}");
                    }
                }
                else
                {
                    Logger.LogInfo("Patterns checked.");
                }

                if (workflowResult.ErrorCount > 0)
                {
                    Logger.LogInfo($"{"Errors count: ",WorkflowLoggerHelper.Align} {workflowResult.ErrorCount}");
                }
                LogStatistics(workflowResult);
                Logger.LogInfo($"{"Time elapsed:",WorkflowLoggerHelper.Align} {stopwatch.Elapsed}");
            }
            catch (Exception ex)
            {
                if (Logger != null)
                {
                    Logger.IsLogErrors = true;
                    Logger.LogError(ex);
                }

                return false;
            }

            return true;
        }

        protected virtual WorkflowBase<TInputGraph, TStage, TWorkflowResult, TPattern, TMatchResult>
            InitWorkflow(TParameters parameters)
        {
            var workflow = CreateWorkflow(parameters);

            workflow.SourceCodeRepository = CreateSourceCodeRepository(parameters);
            workflow.SourceCodeRepository.LoadJson = IsLoadJson(parameters.StartStage);

            if (parameters.Languages?.Count() > 0)
            {
                workflow.SourceCodeRepository.Languages = parameters.Languages.ParseLanguages();
            }

            workflow.PatternsRepository = CreatePatternsRepository(parameters);

            if (parameters.PatternIds?.Count() > 0)
            {
                workflow.PatternsRepository.Identifiers = parameters.PatternIds;
            }

            workflow.Logger = Logger;

            if (parameters.Stage != null)
            {
                workflow.Stage = parameters.Stage.ParseEnum<TStage>();
            }
            else if (string.IsNullOrEmpty(parameters.InputFileNameOrDirectory))
            {
                workflow.Stage = nameof(Stage.Pattern).ParseEnum<TStage>();
            }
            if (parameters.ThreadCount.HasValue)
            {
                workflow.ThreadCount = parameters.ThreadCount.Value;
            }
            if (parameters.NotPreprocessUst.HasValue)
            {
                workflow.IsIncludePreprocessing = !parameters.NotPreprocessUst.Value;
            }
            if (parameters.MaxStackSize.HasValue)
            {
                workflow.MaxStackSize = parameters.MaxStackSize.Value;
            }
            if (parameters.Memory.HasValue)
            {
                workflow.MemoryConsumptionMb = parameters.Memory.Value;
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
            if (parameters.NoIndentedDump.HasValue)
            {
                workflow.IndentedDump = !parameters.NoIndentedDump.Value;
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
            if (parameters.StartStage != null)
            {
                workflow.StartStage = parameters.StartStage.ParseEnum<TStage>();
            }
            if (parameters.DumpStages?.Count() > 0)
            {
                workflow.DumpStages = new HashSet<TStage>(parameters.DumpStages.Select(stage => stage.ParseEnum<TStage>()));
            }
            if (parameters.RenderStages?.Count() > 0)
            {
                workflow.RenderStages = new HashSet<TStage>(parameters.RenderStages.Select(stage => stage.ParseEnum<TStage>()));
            }
            if (parameters.RenderFormat != null)
            {
                workflow.RenderFormat = parameters.RenderFormat.ParseEnum<GraphvizOutputFormat>();
            }
            if (parameters.RenderDirection != null)
            {
                workflow.RenderDirection = parameters.RenderDirection.ParseEnum<GraphvizDirection>();
            }

            return workflow;
        }

        protected abstract WorkflowBase<TInputGraph, TStage, TWorkflowResult, TPattern, TMatchResult> CreateWorkflow(TParameters parameters);

        protected virtual SourceCodeRepository CreateSourceCodeRepository(TParameters parameters)
        {
            return RepositoryFactory
                .CreateSourceCodeRepository(parameters.InputFileNameOrDirectory, parameters.TempDir);
        }

        protected virtual IPatternsRepository CreatePatternsRepository(TParameters parameters)
        {
            return RepositoryFactory.CreatePatternsRepository(parameters.Patterns, Logger);
        }

        protected abstract void LogStatistics(TWorkflowResult workflowResult);

        protected abstract bool IsLoadJson(string startStageString);

        private bool PopulateParamsFromJsonConfig(TParameters cliParams)
        {
            bool result = true;

            string configFile = File.Exists("config.json") ? "config.json" : cliParams.ConfigFile;
            if (!string.IsNullOrEmpty(configFile))
            {
                Logger.LogInfo($"Load settings from {configFile}...");

                string content = null;
                try
                {
                    content = File.ReadAllText(configFile);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    result = false;
                }

                if (content != null)
                {
                    try
                    {
                        JsonConvert.PopulateObject(content, cliParams);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex);
                        result = false;
                    }
                }
            }

            return result;
        }

        private void FillLoggerSettings(string[] args, TParameters cliParams)
        {
            AssemblyName assemblyName = Assembly.GetEntryAssembly().GetName();

            string commandLineArguments = "Command line arguments: " +
                (args.Length > 0 ? string.Join(" ", args) : "not defined.");

            if (cliParams.LogsDir != null)
            {
                Logger.LogsDir = NormalizeLogsDir(cliParams.LogsDir);
            }
            Logger.LogInfo($"{CoreName} version: {assemblyName.Version}");
            Logger.IsLogErrors = cliParams.IsLogErrors.HasValue ? cliParams.IsLogErrors.Value : false;
            Logger.IsLogDebugs = cliParams.IsLogDebugs.HasValue ? cliParams.IsLogDebugs.Value : false;
            Logger.LogInfo(commandLineArguments);
        }

        private void LogParseErrors(IEnumerable<Error> errors)
        {
            foreach (Error error in errors)
            {
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

            Logger.LogInfo("Launch Parameters Description:");
            var paramsParseResult = new Parser().ParseArguments<TParameters>(new string[] { "--help" });
            string paramsInfo = HelpText.AutoBuild(paramsParseResult, 100);
            string[] cliParameterLines = paramsInfo.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (string line in cliParameterLines)
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
