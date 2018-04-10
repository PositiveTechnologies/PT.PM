using CommandLine;
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

namespace PT.PM.Cli
{
    public abstract class CliProcessorBase<TStage, TWorkflowResult, TPattern, TMatchResult, TParameters>
        where TStage : struct, IConvertible
        where TWorkflowResult : WorkflowResultBase<TStage, TPattern, TMatchResult>
        where TMatchResult : MatchResultBase<TPattern>
        where TParameters : CliParameters
    {
        public int ParseAndConvert(string[] args, string coreName)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            var parser = new Parser(config => config.HelpWriter = Console.Out);
            ParserResult<TParameters> parserResult = parser.ParseArguments<TParameters>(args);

            var result = parserResult.MapResult(
                cliParams => {
                    if (!string.IsNullOrEmpty(cliParams.LogsDir) && !cliParams.LogsDir.EndsWith(coreName, StringComparison.OrdinalIgnoreCase))
                    {
                        cliParams.LogsDir = Path.Combine(cliParams.LogsDir, coreName);
                    }

                    int convertResult = 0;
                    if (cliParams.MaxStackSize == 0)
                    {
                        convertResult = Convert(args, cliParams);
                    }
                    else
                    {
                        Thread thread = new Thread(() =>
                        {
                            convertResult = Convert(args, cliParams);
                        },
                        cliParams.MaxStackSize);
                        thread.Start();
                        thread.Join();
                    }
                    return convertResult;
                },
                errors => ProcessErrors(errors));

            return result;
        }

        public int Convert(string[] args, TParameters parameters)
        {
            ILogger logger = new ConsoleFileLogger();

            try
            {
                AssemblyName assemblyName = Assembly.GetEntryAssembly().GetName();
                string name = assemblyName.Name.Replace(".Cli", "");
                logger.LogInfo($"{name} version: {assemblyName.Version}");

                if (logger is FileLogger abstractLogger)
                {
                    string commandLineArguments = "Command line arguments" + (args.Length > 0
                       ? ": " + string.Join(" ", args)
                       : " are not defined.");
                    abstractLogger.LogsDir = parameters.LogsDir;
                    abstractLogger.IsLogErrors = parameters.IsLogErrors;
                    abstractLogger.IsLogDebugs = parameters.IsLogDebugs;
                    abstractLogger.LogInfo(commandLineArguments);
                }

                if (!Enum.TryParse(parameters.Stage, true, out Stage pmStage))
                {
                    pmStage = Stage.Match;
                }

                bool loadJson = !string.IsNullOrEmpty(parameters.StartStage) &&
                    !parameters.StartStage.EqualsIgnoreCase(Stage.File.ToString());

                HashSet<Language> languages = parameters.Languages.ParseLanguages();
                SourceCodeRepository sourceCodeRepository = RepositoryFactory.
                    CreateSourceCodeRepository(parameters.InputFileNameOrDirectory, languages, parameters.TempDir,
                    loadJson);

                IPatternsRepository patternsRepository = RepositoryFactory.CreatePatternsRepository(parameters.Patterns, logger);
                patternsRepository.Identifiers = parameters.PatternIds.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => id.Trim());

                var stopwatch = Stopwatch.StartNew();
                TWorkflowResult workflowResult =
                    InitWorkflowAndProcess(parameters, logger, sourceCodeRepository, patternsRepository);
                stopwatch.Stop();

                if (pmStage != Stage.Pattern)
                {
                    logger.LogInfo("Scan completed.");
                    if (pmStage == Stage.Match)
                    {
                        logger.LogInfo($"{"Matches count: ",WorkflowLoggerHelper.Align} {workflowResult.MatchResults.Count()}");
                    }
                }
                else
                {
                    logger.LogInfo("Patterns checked.");
                }

                if (workflowResult.ErrorCount > 0)
                {
                    logger.LogInfo($"{"Errors count: ",WorkflowLoggerHelper.Align} {workflowResult.ErrorCount}");
                }
                LogStatistics(logger, workflowResult);
                logger.LogInfo($"{"Time elapsed:",WorkflowLoggerHelper.Align} {stopwatch.Elapsed}");
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    if (logger is FileLogger abstractLogger)
                    {
                        abstractLogger.IsLogErrors = true;
                    }
                    logger.LogError(ex);
                }

                return 1;
            }
            finally
            {
                if (logger is IDisposable disposableLogger)
                {
                    disposableLogger.Dispose();
                }
            }

            return 0;
        }

        protected abstract TWorkflowResult InitWorkflowAndProcess(TParameters parameters, ILogger logger, SourceCodeRepository sourceCodeRepository, IPatternsRepository patternsRepository);

        protected abstract void LogStatistics(ILogger logger, TWorkflowResult workflowResult);

        protected int ProcessErrors(IEnumerable<Error> errors)
        {
            return 1;
        }
    }
}
