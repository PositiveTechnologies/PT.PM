using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace PT.PM.TestUtils
{
    public class ZipAtUrlCachedCodeRepository : FilesAggregatorCodeRepository
    {
        private const int percentStep = 5;

        public string RepositoryName { get; private set; }

        public string DownloadPath { get; set; } = TestUtility.TestsDownloadedPath;

        public IEnumerable<string> IgnoredFiles { get; set; } = Enumerable.Empty<string>();

        public string Url { get; private set; }

        public ZipAtUrlCachedCodeRepository(string url, string repositoryName = null)
            : base("")
        {
            Url = url; // url of archive
            RepositoryName = string.IsNullOrEmpty(repositoryName)
                ? Path.GetFileNameWithoutExtension(url)
                : repositoryName;
        }

        public override bool IsFileIgnored(string fileName)
        {
            bool result = IgnoredFiles.Any(fileName.EndsWith);
            if (result)
            {
                return true;
            }

            return base.IsFileIgnored(fileName);
        }

        public override IEnumerable<string> GetFileNames()
        {
            DownloadIfRequired();
            return base.GetFileNames();
        }

        private void DownloadIfRequired()
        {
            if (!Url.EndsWith(".zip"))
            {
                throw new NotSupportedException("Not zip archives are not supported");
            }

            RootPath = Path.Combine(DownloadPath, RepositoryName);
            string zipFileName = RootPath + ".zip";

            if (IsDirectoryNotExistsOrEmpty(RootPath))
            {
                // Block another processes which try to use the same files.
                using (var zipFileNameMutex = new Mutex(false, TestUtility.ConvertToValidMutexName(zipFileName)))
                {
                    if (zipFileNameMutex.WaitOne())
                    {
                        try
                        {
                            if (IsDirectoryNotExistsOrEmpty(RootPath))
                            {
                                if (Directory.Exists(RootPath))
                                {
                                    Directory.Delete(RootPath);
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
                            Logger.LogError(ex);
                        }
                        finally
                        {
                            zipFileNameMutex.ReleaseMutex();
                        }
                    }
                }
            }
        }

        private bool IsDirectoryNotExistsOrEmpty(string directoryName)
        {
            return !Directory.Exists(RootPath) || TestUtility.IsDirectoryEmpty(RootPath);
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
            webClient.DownloadFileAsync(new Uri(Url), zipFileName);

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
            string testDir = Path.Combine(Path.GetTempPath(), RepositoryName);
            Logger.LogInfo($"{RepositoryName} test directory: {testDir}");
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }

            if (!CommonUtils.IsRunningOnLinux)
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

            string[] fileSystemEntries = Directory.GetFileSystemEntries(testDir);
            if (fileSystemEntries.Length == 1)
            {
                if (Directory.GetFiles(testDir).Length == 1)
                {
                    Directory.CreateDirectory(RootPath);
                    File.Move(fileSystemEntries[0], Path.Combine(RootPath, Path.GetFileName(fileSystemEntries[0])));
                }
                else
                {
                    Directory.Move(fileSystemEntries[0], RootPath);
                }
                try
                {
                    Directory.Delete(testDir, true);
                }
                catch (IOException ex)
                {
                    Logger.LogError(new IOException("Something going wrong during unpacking: ", ex));
                }
            }
            else
            {
                Directory.Move(testDir, RootPath);
            }
        }
    }
}
