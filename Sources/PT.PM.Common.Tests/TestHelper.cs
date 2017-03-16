using PT.PM.UstPreprocessing;
using PT.PM.Common.CodeRepository;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;

namespace PT.PM.Common.Tests
{
    public static class TestHelper
    {
        public const string GithubUrlPrefix = "https://github.com/";
        public const string TooLongTestDurationMessage = "Too long test duration.";

        private static string repositoryDirectory;
        public static string TestsPath = $@"Tests/Unit/bin/{(Debug ? "Debug" : "Release")}";
        public static string TestsDataPath = $@"{TestsPath}/Data";
        public static string TestsDownloadedPath = $@"{TestsPath}/Downloaded";
        public static string GraphvizPath = "Sources/packages/Graphviz.2.38.0.2/dot.exe";
        public static string SevenZipPath = "Sources/packages/7-Zip.x64.16.02.1/tools/7z.exe";

        public static bool AllTests =>
#if ALL_TESTS
            true;
#else
            false;
#endif

        internal static bool Debug =>
#if DEBUG
            true;
#else
            false;
#endif

        static TestHelper()
        {
            GetRepositoryDirectory();

            TestsPath = Path.Combine(repositoryDirectory, TestsPath).NormDirSeparator();
            TestsDataPath = Path.Combine(repositoryDirectory, TestsDataPath).NormDirSeparator();
            TestsDownloadedPath = Path.Combine(repositoryDirectory, TestsDownloadedPath).NormDirSeparator();
            GraphvizPath = Helper.IsRunningOnLinux ? "dot" : Path.Combine(repositoryDirectory, GraphvizPath).NormDirSeparator();
            SevenZipPath = Helper.IsRunningOnLinux ? "7z" : Path.Combine(repositoryDirectory, SevenZipPath).NormDirSeparator();
        }

        public static void CheckFile(string fileName, Language language, Stage endStage, ILogger logger = null, bool shouldContainsErrors = false)
        {
            var codeRep = new FileCodeRepository(System.IO.Path.Combine(TestsDataPath, fileName.NormDirSeparator()));

            var log = logger ?? new LoggerMessageCounter();
            var workflow = new Workflow(codeRep, language, stage: endStage);
            workflow.Logger = log;
            workflow.Process();

            if (!shouldContainsErrors)
            {
                Assert.AreEqual(0, log.ErrorCount);
            }
            else
            {
                Assert.Greater(log.ErrorCount, 0);
            }
        }

        public static WorkflowResult CheckProject(TestProject testProject, Language language, Stage endStage,
            IUstPreprocessor astPreprocessor = null, decimal fileSuccessRatio = 1.0m)
        {
            var logger = new LoggerMessageCounter()
            {
                LogToConsole = false
            };
            ZipAtUrlCachedCodeRepository codeRepository = null;
            foreach (var url in testProject.Urls)
            {
                if (FileAtUrlExists(url))
                {
                    codeRepository = new ZipAtUrlCachedCodeRepository(url, testProject.Key)
                    {
                        Extenstions = LanguageExt.GetExtensions(language),
                        IgnoredFiles = testProject.IgnoredFiles,
                        Logger = logger
                    };
                    break;
                }
            }
            if (codeRepository == null)
            {
                Assert.Ignore($@"Project {testProject.Key} has not been found at {(string.Join(", ", testProject.Urls))} or can not be downloaded.");
                return null;
            }

            if (!Directory.Exists(TestsDownloadedPath))
            {
                Directory.CreateDirectory(TestsDownloadedPath);
            }

            var workflow = new Workflow(codeRepository, language, stage: endStage)
            {
                UstPreprocessor = astPreprocessor
            };
            workflow.Logger = logger;
            workflow.ThreadCount = 1;
            WorkflowResult workflowResult = workflow.Process();

            if (fileSuccessRatio == 1.0m)
            {
                Assert.AreEqual(0, logger.ErrorCount, logger.ErrorsString);
            }
            else
            {
                var filesCount = codeRepository.GetFileNames().Count();
                if (filesCount == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Files not exist");
                }
                else
                {
                    decimal actualFileSuccessRatio = (decimal)(filesCount - logger.ErrorFilesCount) / filesCount;
                    System.Diagnostics.Debug.WriteLine($"Actual FileSuccessRatio: {actualFileSuccessRatio}");
                    Assert.GreaterOrEqual(actualFileSuccessRatio, fileSuccessRatio, logger.ErrorsString);
                }
            }

            return workflowResult;
        }

        public static WorkflowResult CheckProject(string projectPath, Language language, Stage endStage)
        {
            var logger = new LoggerMessageCounter() { LogToConsole = false };
            var repository = new FilesAggregatorCodeRepository(
                projectPath , LanguageExt.GetExtensions(language));
            var workflow = new Workflow(repository, language, stage: endStage);
            workflow.Logger = logger;
            workflow.ThreadCount = 1;
            WorkflowResult workflowResult = workflow.Process();

            Assert.AreEqual(0, logger.ErrorCount, logger.ErrorsString);

            return workflowResult;
        }

        public static bool FileAtUrlExists(string url)
        {
            bool result = false;

            WebRequest webRequest = WebRequest.Create(url);
            webRequest.Timeout = 10000;
            webRequest.Method = "HEAD";

            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)webRequest.GetResponse();
                result = true;
            }
            catch (WebException)
            {
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }

            return result;
        }

        public static bool IsDirectoryEmpty(string path)
        {
            IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);
            using (IEnumerator<string> en = items.GetEnumerator())
            {
                return !en.MoveNext();
            }
        }

        public static string ConvertToValidMutexName(string name) => name.Replace('/', ' ').Replace('\\', ' ');

        public static void RenderGraphvizGraph(string dotGraph, string filePath)
        {
            var graph = new GraphvizGraph(dotGraph) { GraphvizPath = GraphvizPath };
            graph.Dump(filePath);
        }

        /// <summary>
        /// Returns path to the current source code file.
        /// </summary>
        /// <param name="thisFilePath"></param>
        private static void GetRepositoryDirectory([CallerFilePath]string thisFilePath = null)
        {
            repositoryDirectory = Path.GetFullPath(Path.Combine(thisFilePath, "..", "..", ".."));
        }
    }
}
