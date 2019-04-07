using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.SourceRepository;
using PT.PM.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Matching;

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

        public static bool IsDebug =>
#if DEBUG
            true;
#else
            false;
#endif

        static TestUtility()
        {
            GetRepositoryDirectory();

            PtPmExePath = Path.Combine(RepositoryDirectory, PtPmExePath);
            GrammarsDirectory = Path.Combine(RepositoryDirectory, GrammarsDirectory).NormalizeDirSeparator();
            TestsPath = Path.Combine(RepositoryDirectory, TestsPath).NormalizeDirSeparator();
            TestsDataPath = Path.Combine(RepositoryDirectory, TestsDataPath).NormalizeDirSeparator();
            TestsOutputPath = Path.Combine(RepositoryDirectory, TestsOutputPath).NormalizeDirSeparator();
            if (!DirectoryExt.Exists(TestsOutputPath))
            {
                DirectoryExt.CreateDirectory(TestsOutputPath);
            }
        }

        public static IReadOnlyList<IMatchResultBase> CheckFile(string fileName, Stage endStage,
            ILogger logger = null, bool shouldContainsErrors = false, bool isIgnoreFilenameWildcards = false,
            Language? language = null, int maxStackSize = 0)
        {
            return CheckFile(fileName, endStage, out RootUst _, logger, shouldContainsErrors, isIgnoreFilenameWildcards,
                language, maxStackSize);
        }

        public static IReadOnlyList<IMatchResultBase> CheckFile(string fileName, Stage endStage, out RootUst ust,
            ILogger logger = null, bool shouldContainsErrors = false, bool isIgnoreFilenameWildcards = false,
            Language? language = null, int maxStackSize = 0)
        {
            var codeRepository = new FileSourceRepository(Path.Combine(TestsDataPath, fileName.NormalizeDirSeparator()));
            if (language != null)
            {
                codeRepository.Languages = new HashSet<Language> { language.Value };
            }

            var log = logger ?? new TestLogger();
            var workflow = new Workflow(codeRepository, stage: endStage)
            {
                IsIgnoreFilenameWildcards = isIgnoreFilenameWildcards,
                Logger = log
            };

            RootUst tempUst = null;
            workflow.UstConverted += (sender, rootUst) => tempUst = rootUst;

            if (maxStackSize == 0)
            {
                workflow.Process();
            }
            else
            {
                var thread = new Thread(() => workflow.Process(), maxStackSize);
                thread.Start();
                thread.Join();
            }

            ust = tempUst;
            string errorString = string.Empty;
            IReadOnlyList<IMatchResultBase> matchResults;

            if (log is TestLogger loggerMessageCounter)
            {
                errorString = loggerMessageCounter.ErrorsString;
                matchResults = loggerMessageCounter.Matches;
            }
            else
            {
                matchResults = new List<IMatchResultBase>();
            }

            if (!shouldContainsErrors)
            {
                Assert.AreEqual(0, log.ErrorCount, errorString);
            }
            else
            {
                Assert.Greater(log.ErrorCount, 0);
            }

            return matchResults;
        }

        public static void CheckProject(string projectPath, Language language, Stage endStage,
            string searchPattern = "*.*", Func<string, bool> searchPredicate = null)
        {
            var logger = new TestLogger { LogToConsole = false };
            var repository = new DirectorySourceRepository(projectPath, language)
            {
                SearchPattern = searchPattern,
                SearchPredicate = searchPredicate
            };
            var workflow = new Workflow(repository, stage: endStage)
            {
                Logger = logger,
                ThreadCount = 1
            };
            workflow.Process();

            Assert.AreEqual(0, logger.ErrorCount, logger.ErrorsString);
        }

        public static string[] GetSerializedFileNames(this TestLogger logger)
        {
            return logger.ProgressEventArgses.Where(arg => arg.Message == Utils.FileSerializedMessage)
                .Select(arg => arg.CurrentFile).ToArray();
        }

        /// <summary>
        /// Returns path to the current source code file.
        /// </summary>
        private static void GetRepositoryDirectory([CallerFilePath]string thisFilePath = null)
        {
            RepositoryDirectory = Path.GetFullPath(Path.Combine(thisFilePath, "..", "..", ".."));
        }
    }
}
