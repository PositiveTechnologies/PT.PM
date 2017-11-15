using Fclp;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Json;
using PT.PM.Matching.PatternsRepository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace PT.PM.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var parser = new FluentCommandLineParser();

            string fileName = "";
            string patternsString = "";
            int threadCount = 1;
            string languagesString = "";
            Stage stage = Stage.Match;
            int maxStackSize = 0;
            int maxTimespan = 0;
            int memoryConsumptionMb = 300;
            string logsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PT.PM", "Logs");
            string tempDir = Path.GetTempPath();
            bool logErrors = false;
            bool logDebugs = false;
            bool showVersion = true;
            bool isDumpUst = false;
            bool isIndentedUst = false;
            bool isIncludeTextSpansInUst = true;
            bool isPreprocess = true;
            Stage startStage = Stage.File;

            parser.Setup<string>('f', "files").Callback(f => fileName = f.NormDirSeparator());
            parser.Setup<string>('l', "languages").Callback(l => languagesString = l);
            parser.Setup<string>('p', "patterns").Callback(param =>
                patternsString = param.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                    ? param.NormDirSeparator()
                    : param.Replace('\\', '/')
            );
            parser.Setup<int>('t', "threads").Callback(t => threadCount = t);
            parser.Setup<Stage>('s', "stage").Callback(s => stage = s);
            parser.Setup<int>("max-stack-size").Callback(mss => maxStackSize = mss);
            parser.Setup<int>("max-timespan").Callback(mt => maxTimespan = mt);
            parser.Setup<int>('m', "memory").Callback(m => memoryConsumptionMb = m);
            parser.Setup<string>("logs-dir").Callback(lp => logsDir = lp.NormDirSeparator());
            parser.Setup<string>("temp-dir").Callback(param => tempDir = param);
            parser.Setup<bool>("log-errors").Callback(le => logErrors = le);
            parser.Setup<bool>("log-debugs").Callback(ld => logDebugs = ld);
            parser.Setup<bool>('v', "version").Callback(v => showVersion = v);
            parser.Setup<bool>("dump-ust").Callback(param => isDumpUst = param);
            parser.Setup<bool>("indented-ust").Callback(param => isIndentedUst = param);
            parser.Setup<bool>("text-spans-ust").Callback(param => isIncludeTextSpansInUst = param);
            parser.Setup<bool>("preprocess-ust").Callback(param => isPreprocess = param);
            parser.Setup<Stage>("start-stage").Callback(param => startStage = param);

            ILogger logger = new ConsoleFileLogger();
            string commandLineArguments = "Command line arguments" + (args.Length > 0 
                ? ": " + string.Join(" ", args)
                : " are not defined.");

            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var argsWithUsualSlashes = args.Select(arg => arg.Replace('/', '\\')).ToArray(); // TODO: bug in FluentCommandLineParser.
            var parsingResult = parser.Parse(argsWithUsualSlashes);

            if (!parsingResult.HasErrors)
            {
                if (isDumpUst)
                {
                    stage = Stage.Ust;
                    logger = new DummyLogger();
                }

                try
                {
                    if (showVersion)
                    {
                        logger.LogInfo($"PT.PM version: {version}");
                    }

                    if (logger is FileLogger abstractLogger)
                    {
                        abstractLogger.LogsDir = logsDir;
                        abstractLogger.IsLogErrors = logErrors;
                        abstractLogger.IsLogDebugs = logDebugs;
                        abstractLogger.LogInfo(commandLineArguments);
                    }

                    if (string.IsNullOrEmpty(fileName) && string.IsNullOrEmpty(patternsString))
                    {
                        throw new ArgumentException("at least --files or --patterns parameter required");
                    }

                    if (string.IsNullOrEmpty(fileName))
                    {
                        stage = Stage.Pattern;
                    }

                    HashSet<Language> languages = languagesString.ParseLanguages();
                    SourceCodeRepository sourceCodeRepository = RepositoryFactory.CreateSourceCodeRepository(fileName, languages, tempDir, startStage);

                    logger.SourceCodeRepository = sourceCodeRepository;

                    IPatternsRepository patternsRepository = RepositoryFactory.CreatePatternsRepository(patternsString);

                    var workflow = new Workflow(sourceCodeRepository, patternsRepository, stage)
                    {
                        Logger = logger,
                        ThreadCount = threadCount,
                        MaxStackSize = maxStackSize,
                        MaxTimespan = maxTimespan,
                        MemoryConsumptionMb = memoryConsumptionMb,
                        IsIncludePreprocessing = isPreprocess,
                        LogsDir = logsDir,
                        DumpDir = tempDir,
                        StartStage = startStage
                    };
                    var stopwatch = Stopwatch.StartNew();
                    WorkflowResult workflowResult = workflow.Process();
                    stopwatch.Stop();

                    if (isDumpUst)
                    {
                        DumpUst(isIndentedUst, isIncludeTextSpansInUst, workflowResult);
                    }

                    if (stage != Stage.Pattern)
                    {
                        logger.LogInfo("Scan completed.");
                        if (stage == Stage.Match)
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
                }
                finally
                {
                    if (logger is IDisposable disposableLogger)
                    {
                        disposableLogger.Dispose();
                    }
                }
            }
            else
            {
                Console.WriteLine($"PT.PM version: {version}");
                Console.WriteLine(commandLineArguments);
                Console.WriteLine("Command line arguments processing error: " + parsingResult.ErrorText);
            }
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press Enter to exit");
                Console.ReadLine();
            }
        }

        private static void DumpUst(bool isIndentedUst, bool isIncludeTextSpansInUst, WorkflowResult workflowResult)
        {
            var serializer = new JsonUstSerializer
            {
                Indented = isIndentedUst,
                IncludeTextSpans = isIncludeTextSpansInUst
            };
            Console.Write(serializer.Serialize(workflowResult.Usts));
        }
    }
}
