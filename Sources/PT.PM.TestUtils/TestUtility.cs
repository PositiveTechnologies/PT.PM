using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PT.PM.TestUtils
{
    public static class TestUtility
    {
        public const string GithubUrlPrefix = "https://github.com/";
        public const string TooLongTestDurationMessage = "Too long test duration.";

        public static string RepositoryDirectory;
        public static string PtPmExePath = $@"bin/{(IsDebug ? "Debug" : "Release")}/netcoreapp2.1/PT.PM.Cli.dll";
        public static string GrammarsDirectory = "Sources/antlr-grammars-v4";
        public static string TestsPath = $@"Tests/{(IsDebug ? "Debug" : "Release")}/netcoreapp2.1";
        public static string TestsDataPath = $@"{TestsPath}/Data";
        public static string TestsOutputPath = $@"{TestsPath}/Output";

        internal static bool IsDebug =>
#if DEBUG
            true;
#else
            false;
#endif

        static TestUtility()
        {
            GetRepositoryDirectory();

            PtPmExePath = Path.Combine(RepositoryDirectory, PtPmExePath);
            GrammarsDirectory = Path.Combine(RepositoryDirectory, GrammarsDirectory).NormDirSeparator();
            TestsPath = Path.Combine(RepositoryDirectory, TestsPath).NormDirSeparator();
            TestsDataPath = Path.Combine(RepositoryDirectory, TestsDataPath).NormDirSeparator();
            TestsOutputPath = Path.Combine(RepositoryDirectory, TestsOutputPath).NormDirSeparator();
            if (!Directory.Exists(TestsOutputPath))
            {
                Directory.CreateDirectory(TestsOutputPath);
            }
        }

        public static WorkflowResult CheckFile(string fileName, Stage endStage,
            ILogger logger = null, bool shouldContainsErrors = false, bool isIgnoreFilenameWildcards = false,
            Language language = null, int maxStackSize = 0)
        {
            var codeRepository = new FileCodeRepository(Path.Combine(TestsDataPath, fileName.NormDirSeparator()));
            if (language != null)
            {
                codeRepository.Languages = new HashSet<Language>() { language };
            }

            var log = logger ?? new LoggerMessageCounter();
            var workflow = new Workflow(codeRepository, stage: endStage)
            {
                IsIgnoreFilenameWildcards = isIgnoreFilenameWildcards,
                Logger = log
            };

            WorkflowResult workflowResult = null;
            if (maxStackSize == 0)
            {
                workflowResult = workflow.Process();
            }
            else
            {
                var thread = new Thread(() => workflowResult = workflow.Process(), maxStackSize);
                thread.Start();
                thread.Join();
            }

            string errorString = string.Empty;
            if (log is LoggerMessageCounter loggerMessageCounter)
            {
                errorString = loggerMessageCounter.ErrorsString;
            }
            if (!shouldContainsErrors)
            {
                Assert.AreEqual(0, log.ErrorCount, errorString);
            }
            else
            {
                Assert.Greater(log.ErrorCount, 0);
            }

            return workflowResult;
        }

        public static WorkflowResult CheckProject(string projectPath, Language language, Stage endStage,
            string searchPattern = "*.*", Func<string, bool> searchPredicate = null)
        {
            var logger = new LoggerMessageCounter() { LogToConsole = false };
            var repository = new DirectoryCodeRepository(projectPath, language)
            {
                SearchPattern = searchPattern,
                SearchPredicate = searchPredicate
            };
            var workflow = new Workflow(repository, stage: endStage)
            {
                Logger = logger,
                ThreadCount = 1
            };
            WorkflowResult workflowResult = workflow.Process();

            Assert.AreEqual(0, logger.ErrorCount, logger.ErrorsString);

            return workflowResult;
        }

        /// <summary>
        /// Returns path to the current source code file.
        /// </summary>
        /// <param name="thisFilePath"></param>
        private static void GetRepositoryDirectory([CallerFilePath]string thisFilePath = null)
        {
            RepositoryDirectory = Path.GetFullPath(Path.Combine(thisFilePath, "..", "..", ".."));
        }
    }
}
