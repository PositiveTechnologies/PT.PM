using CommandLine;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Matching.PatternsRepository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace PT.PM.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

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
                        });
                        thread.Start();
                        thread.Join();
                    }
                    return convertResult;
                },
                errors => ProcessErrors(errors));

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press Enter to exit");
                Console.ReadLine();
            }

            return result;
        }

        private static int Convert(string[] args, CliParameters parameters)
        {
            ILogger logger = new ConsoleFileLogger();

            try
            {
                if (parameters.ShowVersion)
                {
                    string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    logger.LogInfo($"PT.PM version: {version}");
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

                Stage stage = parameters.Stage;
                if (string.IsNullOrEmpty(parameters.InputFileNameOrDirectory))
                {
                    stage = Stage.Pattern;
                }

                HashSet<Language> languages = parameters.Languages.ParseLanguages();
                SourceCodeRepository sourceCodeRepository = RepositoryFactory.
                    CreateSourceCodeRepository(parameters.InputFileNameOrDirectory, languages, parameters.TempDir, parameters.StartStage == Stage.Ust);

                logger.SourceCodeRepository = sourceCodeRepository;

                IPatternsRepository patternsRepository = RepositoryFactory.CreatePatternsRepository(parameters.Patterns);

                HashSet<Stage> dumpStages = new HashSet<Stage>(parameters.DumpStages.ParseCollection<Stage>());
                var workflow = new Workflow(sourceCodeRepository, patternsRepository, stage)
                {
                    Logger = logger,
                    ThreadCount = parameters.ThreadCount,
                    MemoryConsumptionMb = parameters.Memory,
                    IsIncludePreprocessing = parameters.IsPreprocessUst,
                    LogsDir = parameters.LogsDir,
                    DumpDir = parameters.LogsDir,
                    StartStage = parameters.StartStage,
                    DumpStages = dumpStages,
                    IndentedDump = parameters.IndentedDump,
                    DumpWithTextSpans = parameters.IncludeTextSpansInDump
                };
                var stopwatch = Stopwatch.StartNew();
                WorkflowResult workflowResult = workflow.Process();
                stopwatch.Stop();

                if (parameters.Stage != Stage.Pattern)
                {
                    logger.LogInfo("Scan completed.");
                    if (parameters.Stage == Stage.Match)
                    {
                        logger.LogInfo($"{"Matches count: ",-22} {workflowResult.MatchingResults.Count()}");
                    }
                }
                else
                {
                    logger.LogInfo("Patterns checked.");
                }
                logger.LogInfo($"{"Errors count: ",-22} {workflowResult.ErrorCount}");
                var workflowLoggerHelper = new WorkflowLoggerHelper(logger, workflowResult);
                workflowLoggerHelper.LogStatistics();
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

        private static int ProcessErrors(IEnumerable<Error> errors)
        {
            return 1;
        }
    }
}
