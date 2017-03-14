using PT.PM.Common.CodeRepository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace PT.PM.Common.Tests
{
    public class ZipAtUrlCachedCodeRepository : ISourceCodeRepository
    {
        private const int percentStep = 5;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        /// <summary>
        /// Url of archive.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Name of repository.
        /// </summary>
        public string RepositoryName { get; set; }

        public string DownloadPath { get; set; } = TestHelper.TestsDownloadedPath;

        public IEnumerable<string> Extenstions { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<string> IgnoredFiles { get; set; } = Enumerable.Empty<string>();

        public string CachedSourceDir { get; private set; }

        public ZipAtUrlCachedCodeRepository(string url, string repositoryName = null)
        {
            Path = url;
            RepositoryName = string.IsNullOrEmpty(repositoryName)
                ? System.IO.Path.GetFileNameWithoutExtension(url)
                : repositoryName;
        }

        public IEnumerable<string> GetFileNames()
        {
            if (!Path.EndsWith(".zip"))
            {
                throw new NotSupportedException("Not zip archives are not supported");
            }
            
            CachedSourceDir = System.IO.Path.Combine(DownloadPath, RepositoryName);
            string zipFileName = CachedSourceDir + ".zip";

            if (IsDirectoryNotExistsOrEmpty(CachedSourceDir))
            {
                // Block another processes which try to use the same files.
                using (var zipFileNameMutex = new Mutex(false, TestHelper.ConvertToValidMutexName(zipFileName)))
                {
                    if (zipFileNameMutex.WaitOne())
                    {
                        try
                        {
                            if (IsDirectoryNotExistsOrEmpty(CachedSourceDir))
                            {
                                if (Directory.Exists(CachedSourceDir))
                                {
                                    Directory.Delete(CachedSourceDir);
                                }

                                if (!File.Exists(zipFileName))
                                {
                                    DownloadPack(zipFileName);
                                }

                                UnpackFiles(zipFileName);
                            }
                            else
                            {
                                Logger.LogInfo($"{RepositoryName} already downloaded and unpacked.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Something going wrong: " + ex.Message);
                        }
                        finally
                        {
                            zipFileNameMutex.ReleaseMutex();
                        }
                    }
                }
            }

            return GetFileNamesFromDownloadedAndUnpacked();
        }

        public string GetFullPath(string relativePath)
        {
            return System.IO.Path.Combine(System.IO.Path.GetFullPath(CachedSourceDir), relativePath);
        }

        public SourceCodeFile ReadFile(string file)
        {
            var removeBeginLength = CachedSourceDir.Length + (CachedSourceDir.EndsWith("\\") ? 0 : 1);
            var fileName = System.IO.Path.GetFileName(file);
            SourceCodeFile result = new SourceCodeFile(fileName);
            try
            {
                int removeEndLength = fileName.Length + 1;
                result.RelativePath = removeEndLength + removeBeginLength > file.Length
                        ? "" : file.Remove(file.Length - removeEndLength).Remove(0, removeBeginLength);
                result.Code = File.ReadAllText(file);
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError("File read error: " + file, ex);
            }
            return result;
        }

        private bool IsDirectoryNotExistsOrEmpty(string directoryName)
        {
            return !Directory.Exists(CachedSourceDir) || TestHelper.IsDirectoryEmpty(CachedSourceDir);
        }

        private void DownloadPack(string zipFileName)
        {
            bool fileDownloaded = false;
            object progressLockObj = new object();
            int previousPercent = 0;
            string currentFileName = RepositoryName;
            WebClient webClient = new WebClient();

            if (Logger != null)
            {
                webClient.DownloadProgressChanged += (sender, e) =>
                {
                    lock (progressLockObj)
                    {
                        if (e.ProgressPercentage / percentStep > previousPercent / percentStep)
                        {
                            Logger.LogInfo($"{currentFileName}: {e.ProgressPercentage / percentStep * percentStep}% downloaded.");
                            previousPercent = e.ProgressPercentage;
                        }
                    }
                };
            }
            webClient.DownloadFileCompleted += (sender, e) => fileDownloaded = true;
            Logger.LogInfo($"{RepositoryName} downloading...");
            if (!Directory.Exists(DownloadPath))
            {
                Directory.CreateDirectory(DownloadPath);
            }
            webClient.DownloadFileAsync(new Uri(Path), zipFileName);

            do
            {
                Thread.Sleep(100);
            }
            while (!fileDownloaded);

            Logger.LogInfo($"{RepositoryName} has been downloaded.");
        }

        private void UnpackFiles(string zipFileName)
        {
            Logger.LogInfo($"{RepositoryName} extraction...");
            string testDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), RepositoryName);
            Logger.LogInfo($"{RepositoryName} test directory: {testDir}");
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }

            if (!Helper.IsRunningOnLinux)
            {
                // Extract long paths with 7zip, also see here: http://stackoverflow.com/questions/5188527/how-to-deal-with-files-with-a-name-longer-than-259-characters
                SevenZipExtractor.Extract(zipFileName, testDir);
            }
            else
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(zipFileName, testDir);
                Thread.Sleep(500);
            }

            File.Delete(zipFileName);
            Logger.LogInfo($"{RepositoryName} has been extracted.");

            var fileSystemEntries = Directory.GetFileSystemEntries(testDir);
            if (fileSystemEntries.Length == 1)
            {
                if (Directory.GetFiles(testDir).Length == 1)
                {
                    Directory.CreateDirectory(CachedSourceDir);
                    File.Move(fileSystemEntries.First(), System.IO.Path.Combine(CachedSourceDir,
                        System.IO.Path.GetFileName(fileSystemEntries.First())));
                }
                else
                {
                    Directory.Move(fileSystemEntries.First(), CachedSourceDir);
                }
                try
                {
                    Directory.Delete(testDir, true);
                }
                catch (IOException ex)
                {
                    Logger.LogError("Something going wrong during unpacking: " + ex.Message);
                }
            }
            else
            {
                Directory.Move(testDir, CachedSourceDir);
            }
        }

        private IEnumerable<string> GetFileNamesFromDownloadedAndUnpacked()
        {
            IEnumerable<string> filesCollection = Directory
                .GetFiles(CachedSourceDir, "*.*", SearchOption.AllDirectories)
                .Where(s => Extenstions.Any(s.EndsWith));
            filesCollection = filesCollection.Where(fileName => IgnoredFiles.All(ignoreFile => !fileName.Contains(ignoreFile)));
            return filesCollection.ToArray();
        }
    }
}
