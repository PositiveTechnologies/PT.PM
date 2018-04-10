using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace PT.PM.TestUtils
{
    public static class TestUtility
    {
        public const string GithubUrlPrefix = "https://github.com/";
        public const string TooLongTestDurationMessage = "Too long test duration.";

        public static string RepositoryDirectory;
        public static string GrammarsDirectory = "Sources/antlr-grammars-v4";
        public static string TestsPath = $@"Tests/Unit/bin/{(IsDebug ? "Debug" : "Release")}";
        public static string TestsDataPath = $@"{TestsPath}/Data";
        public static string TestsOutputPath = $@"{TestsPath}/Output";
        public static string GraphvizPath = "Sources/packages/Graphviz.2.38.0.2/dot.exe";
        public static string SevenZipPath = "Sources/packages/7-Zip.x64.16.02.1/tools/7z.exe";

        internal static bool IsDebug =>
#if DEBUG
            true;
#else
            false;
#endif

        static TestUtility()
        {
            GetRepositoryDirectory();

            GrammarsDirectory = Path.Combine(RepositoryDirectory, GrammarsDirectory).NormDirSeparator();
            TestsPath = Path.Combine(RepositoryDirectory, TestsPath).NormDirSeparator();
            TestsDataPath = Path.Combine(RepositoryDirectory, TestsDataPath).NormDirSeparator();
            TestsOutputPath = Path.Combine(RepositoryDirectory, TestsOutputPath).NormDirSeparator();
            if (!Directory.Exists(TestsOutputPath))
            {
                Directory.CreateDirectory(TestsOutputPath);
            }
            GraphvizPath = CommonUtils.IsRunningOnLinux ? "dot" : Path.Combine(RepositoryDirectory, GraphvizPath).NormDirSeparator();
            SevenZipPath = CommonUtils.IsRunningOnLinux ? "7z" : Path.Combine(RepositoryDirectory, SevenZipPath).NormDirSeparator();
        }

        public static WorkflowResult CheckFile(string fileName, Stage endStage,
            ILogger logger = null, bool shouldContainsErrors = false, bool isIgnoreFilenameWildcards = false,
            Language language = null)
        {
            var codeRepository = new FileCodeRepository(Path.Combine(TestsDataPath, fileName.NormDirSeparator()));
            if (language != null)
            {
                codeRepository.Languages = new HashSet<Language>() { language };
            }

            var log = logger ?? new LoggerMessageCounter();
            var workflow = new Workflow(codeRepository, stage: endStage);
            workflow.IsIgnoreFilenameWildcards = isIgnoreFilenameWildcards;
            workflow.Logger = log;
            WorkflowResult workflowResult = workflow.Process();

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
