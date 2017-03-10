using PT.PM.UstPreprocessing;
using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Common.CodeRepository;
using PT.PM.Patterns.PatternsRepository;
using Fclp;
using System;
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
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            var parser = new FluentCommandLineParser();
            
            string fileName = "";
            string escapedPatterns = "";
            int threadCount = 1;
            LanguageFlags languages = LanguageExt.AllLanguages;
            Stage stage = Stage.Match;
            int maxStackSize = 0;
            int maxTimespan = 0;
            int memoryConsumptionMb = 300;
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Application Inspector", "Logs", "pm");
            bool logErrors = false;
            bool logDebugs = false;

            parser.Setup<string>('f').Callback(f => fileName = f);
            parser.Setup<LanguageFlags>('l').Callback(l => languages = l);
            parser.Setup<string>("patterns").Callback(p => escapedPatterns = p);
            parser.Setup<int>("threads").Callback(t => threadCount = t);
            parser.Setup<Stage>("stage").Callback(s => stage = s);
            parser.Setup<int>("max-stack-size").Callback(mss => maxStackSize = mss);
            parser.Setup<int>("max-timespan").Callback(mt => maxTimespan = mt);
            parser.Setup<int>('m').Callback(m => memoryConsumptionMb = m);
            parser.Setup<string>("log-path").Callback(lp => logPath = lp);
            parser.Setup<bool>("log-errors").Callback(le => logErrors = le);
            parser.Setup<bool>("log-debugs").Callback(ld => logDebugs = ld);

            AbstractLogger logger = new ConsoleLogger();
            string commandLineArguments = "Command line arguments: " + (args.Length > 0 ? string.Join(" ", args) : "<empty>");
            var parsingResult = parser.Parse(args);

            if (!parsingResult.HasErrors)
            {
                try
                {
                    if (string.IsNullOrEmpty(fileName) && string.IsNullOrEmpty(escapedPatterns))
                    {
                        throw new ArgumentException("at least -f or --patterns parameter required");
                    }

                    if (string.IsNullOrEmpty(fileName))
                    {
                        stage = Stage.Patterns;
                    }

                    logger.LogPath = logPath;
                    logger.LogErrors = logErrors;
                    logger.LogDebugs = logDebugs;
                    logger.LogInfo(commandLineArguments);

                    ISourceCodeRepository sourceCodeRepository;
                    if (Directory.Exists(fileName))
                    {
                        sourceCodeRepository = new FilesAggregatorCodeRepository(fileName, LanguageExt.GetExtensions(languages));
                    }
                    else
                    {
                        sourceCodeRepository = new FileCodeRepository(fileName);
                    }
                    logger.SourceCodeRepository = sourceCodeRepository;

                    IPatternsRepository patternsRepository;
                    if (string.IsNullOrEmpty(escapedPatterns))
                    {
                        patternsRepository = new DefaultPatternRepository();
                    }
                    else
                    {
                        var patterns = StringCompressorEscaper.UnescapeDecompress(escapedPatterns);
                        patternsRepository = new StringPatternsRepository(patterns);
                    }

                    var workflow = new Workflow(sourceCodeRepository, languages, patternsRepository, stage)
                    {
                        Logger = logger,
                        ThreadCount = threadCount,
                        MaxStackSize = maxStackSize,
                        MaxTimespan = maxTimespan,
                        MemoryConsumptionMb = memoryConsumptionMb
                    };
                    var stopwatch = Stopwatch.StartNew();
                    var results = workflow.Process();
                    stopwatch.Stop();

                    if (stage != Stage.Patterns)
                    {
                        logger.LogInfo("Scan completed.");
                        if (stage == Stage.Match)
                        {
                            logger.LogInfo("{0,-22} {1}", "Matches count:", results.Count().ToString());
                        }
                    }
                    else
                    {
                        logger.LogInfo("Patterns checked.");
                    }
                    logger.LogInfo("{0,-22} {1}", "Errors count:", workflow.ErrorCount.ToString());
                    workflow.LogStatistics();
                    logger.LogInfo("{0,-22} {1}", "Time elapsed:", stopwatch.Elapsed.ToString());
                }
                catch (Exception ex)
                {
                    logger?.LogError("Error while processing", ex);
                }
                finally
                {
                    var disposableLogger = logger as IDisposable;
                    if (disposableLogger != null)
                    {
                        disposableLogger.Dispose();
                    }
                }
            }
            else
            {
                System.Console.WriteLine(commandLineArguments);
                System.Console.WriteLine("Command line arguments processing error: " + parsingResult.ErrorText);
            }

            if (logger is ConsoleLogger)
            {
                System.Console.WriteLine("Press Enter to exit...");
                System.Console.ReadLine();
            }
        }
    }
}
