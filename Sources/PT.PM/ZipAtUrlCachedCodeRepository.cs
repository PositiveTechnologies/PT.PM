using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace PT.PM
{
    public class ZipAtUrlCachedCodeRepository : FilesAggregatorCodeRepository
    {
        private const int percentStep = 5;
        private const long bytesStep = 500000;

        public string RepositoryName { get; private set; }

        public string DownloadPath { get; set; } = Path.GetTempPath();

        public IEnumerable<string> IgnoredFiles { get; set; } = Enumerable.Empty<string>();

        public string Url { get; private set; }

        public ZipAtUrlCachedCodeRepository(string url, string repositoryName = null)
            : base("")
        {
            Url = url; // url of archive
            RepositoryName = string.IsNullOrEmpty(repositoryName)
                ? ConvertToValidFileName(url)
                : repositoryName;
            if (RepositoryName.EndsWith(".zip"))
                RepositoryName = RepositoryName.Remove(RepositoryName.Length - ".zip".Length);
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
                using (var zipFileNameMutex = new Mutex(false, ConvertToValidMutexName(zipFileName)))
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
                            RootPath = null;
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
            return !Directory.Exists(RootPath) || IsDirectoryEmpty(RootPath);
        }

        private void DownloadPack(string zipFileName)
        {
            bool fileDownloaded = false;
            object progressLockObj = new object();
            int previousPercent = 0;
            long previousBytes = 0;
            string currentFileName = RepositoryName;
            WebClient webClient = new WebClient();

            if (Logger != null)
            {
                webClient.DownloadProgressChanged += (sender, e) =>
                {
                    lock (progressLockObj)
                    {
                        if (e.ProgressPercentage == 0)
                        {
                            if (e.ProgressPercentage / percentStep > previousPercent / percentStep)
                            {
                                Logger.LogInfo($"{currentFileName}: {e.ProgressPercentage / percentStep * percentStep}% downloaded.");
                                previousPercent = e.ProgressPercentage;
                            }
                        }
                        else
                        {
                            if (e.BytesReceived / bytesStep > previousBytes / bytesStep)
                            {
                                Logger.LogInfo($"{currentFileName}: {e.BytesReceived} bytes received.");
                                previousBytes = e.BytesReceived;
                            }
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

            var sevenZipExtractor = new SevenZipExtractor();
            if (!CommonUtils.IsRunningOnLinux && File.Exists(sevenZipExtractor.SevenZipPath))
            {
                // Extract long paths with 7zip, also see here: http://stackoverflow.com/questions/5188527/how-to-deal-with-files-with-a-name-longer-than-259-characters
                sevenZipExtractor.Extract(zipFileName, testDir);
            }
            else
            {
                new StandartArchiveExtractor().Extract(zipFileName, testDir);
                Thread.Sleep(500);
            }

            File.Delete(zipFileName);
            Logger.LogInfo($"{RepositoryName} has been extracted.");

            string[] directories = Directory.GetDirectories(testDir);
            if (directories.Length == 1)
            {
                Directory.CreateDirectory(RootPath);
                foreach (string fileSystemEntry in Directory.EnumerateFileSystemEntries(directories[0]))
                {
                    string shortName = Path.GetFileName(fileSystemEntry);
                    string newName = Path.Combine(RootPath, shortName);
                    if (File.Exists(fileSystemEntry))
                    {
                        File.Move(fileSystemEntry, newName);
                    }
                    else
                    {
                        Directory.Move(fileSystemEntry, newName);
                    }
                }

                try
                {
                    Directory.Delete(directories[0], true);
                }
                catch (IOException ex)
                {
                    Logger.LogError(new ReadException("", ex, "Something going wrong during unpacking"));
                }
            }
            else
            {
                Directory.Move(testDir, RootPath);
            }
        }

        private static bool IsDirectoryEmpty(string path)
        {
            IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);
            using (IEnumerator<string> en = items.GetEnumerator())
            {
                return !en.MoveNext();
            }
        }

        private static string ConvertToValidMutexName(string name) => ConvertToValidFileName(name);

        private static string ConvertToValidFileName(string str)
        {
            StringBuilder result = new StringBuilder(str.Length);
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in str)
            {
                result.Append(!invalidChars.Contains(c) ? c : '-');
            }
            return result.ToString();
        }
    }
}
