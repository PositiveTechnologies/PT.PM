using CommandLine;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Matching;
using PT.PM.Matching.PatternsRepository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace PT.PM.Cli
{
    public abstract class CliProcessorBase<TStage, TWorkflowResult, TPattern, TMatchingResult>
        where TStage : struct, IConvertible
        where TWorkflowResult : WorkflowResultBase<TStage, TPattern, TMatchingResult>
        where TMatchingResult : MatchingResultBase<TPattern>
    {
        public int ParseAndConvert(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            var parser = new Parser(config => config.HelpWriter = Console.Out);
            ParserResult<CliParameters> parserResult = parser.ParseArguments<CliParameters>(args);

            var result = parserResult.MapResult(
                cliParams => {
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

        public int Convert(string[] args, CliParameters parameters)
        {
            ILogger logger = new ConsoleFileLogger();

            try
            {
                if (parameters.ShowVersion)
                {
                    AssemblyName assemblyName = Assembly.GetEntryAssembly().GetName();
                    string name = assemblyName.Name.Replace(".Cli", "");
                    logger.LogInfo($"{name} version: {assemblyName.Version}");
                }

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

                if (string.IsNullOrEmpty(parameters.InputFileNameOrDirectory) && string.IsNullOrEmpty(parameters.Patterns))
                {
                    throw new ArgumentException("at least --files or --patterns parameter required");
                }

                if (!Enum.TryParse(parameters.Stage, true, out Stage pmStage))
                {
                    pmStage = Stage.Match;
                }
                if (!Enum.TryParse(parameters.StartStage, true, out Stage pmStartStage))
                {
                    pmStartStage = Stage.File;
                }

                HashSet<Language> languages = parameters.Languages.ParseLanguages();
                SourceCodeRepository sourceCodeRepository = RepositoryFactory.
                    CreateSourceCodeRepository(parameters.InputFileNameOrDirectory, languages, parameters.TempDir,
                    pmStartStage != Stage.File);

                logger.SourceCodeRepository = sourceCodeRepository;

                IPatternsRepository patternsRepository = RepositoryFactory.CreatePatternsRepository(parameters.Patterns);

                var stopwatch = Stopwatch.StartNew();
                TWorkflowResult workflowResult =
                    InitWorkflowAndProcess(parameters, logger, sourceCodeRepository, patternsRepository);
                stopwatch.Stop();

                if (pmStage != Stage.Pattern)
                {
                    logger.LogInfo("Scan completed.");
                    if (pmStage == Stage.Match)
                    {
                        logger.LogInfo($"{"Matches count: ",-22} {workflowResult.MatchingResults.Count()}");
                    }
                }
                else
                {
                    logger.LogInfo("Patterns checked.");
                }
                logger.LogInfo($"{"Errors count: ",-22} {workflowResult.ErrorCount}");
                LogStatistics(logger, workflowResult);
                logger.LogInfo($"{"Time elapsed:",-22} {stopwatch.Elapsed}");
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

        protected abstract TWorkflowResult InitWorkflowAndProcess(CliParameters parameters, ILogger logger, SourceCodeRepository sourceCodeRepository, IPatternsRepository patternsRepository);

        protected abstract void LogStatistics(ILogger logger, TWorkflowResult workflowResult);

        protected int ProcessErrors(IEnumerable<Error> errors)
        {
            return 1;
        }
    }
}
