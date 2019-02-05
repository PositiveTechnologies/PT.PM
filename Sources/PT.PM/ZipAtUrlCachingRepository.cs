using PT.PM.Common;
using PT.PM.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace PT.PM
{
    public class ZipAtUrlCachingRepository : ZipCachingRepository
    {
        private const int percentStep = 5;
        private const long bytesStep = 500000;

        public string Url { get; private set; }

        public string DownloadPath { get; set; } = Path.GetTempPath();

        static ZipAtUrlCachingRepository()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public ZipAtUrlCachingRepository(string url, string name = null)
            : base("")
        {
            Url = url;
            Name = ConvertToValidFileName(string.IsNullOrEmpty(name) ? TextUtils.HttpRegex.Replace(url, "") : name);
            if (Name.EndsWith(".zip"))
                Name = Name.Remove(Name.Length - ".zip".Length);
            RemoveAfterExtraction = true;
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

            ExtractPath = DownloadPath;
            RootPath = Path.Combine(ExtractPath, Name);
            ArchiveName = RootPath + ".zip";

            if (Rewrite || IsDirectoryNotExistsOrEmpty())
            {
                // Block another processes which try to use the same files.
                using (var zipFileNameMutex = new Mutex(false, ConvertToValidMutexName(ArchiveName)))
                {
                    if (!zipFileNameMutex.WaitOne())
                    {
                        return;
                    }

                    try
                    {
                        if (Rewrite || IsDirectoryNotExistsOrEmpty())
                        {
                            if (DirectoryExt.Exists(RootPath))
                            {
                                DirectoryExt.Delete(RootPath);
                            }

                            if (FileExt.Exists(ArchiveName))
                            {
                                if (Rewrite)
                                {
                                    FileExt.Delete(ArchiveName);
                                }
                                DownloadArchive();
                            }
                            else
                            {
                                DownloadArchive();
                            }
                        }
                        else
                        {
                            Logger.LogInfo($"{Name} already downloaded and unpacked.");
                        }
                    }
                    catch (Exception ex) when (!(ex is ThreadAbortException))
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

        private void DownloadArchive()
        {
            bool fileDownloaded = false;
            object progressLockObj = new object();
            int previousPercent = 0;
            long previousBytes = 0;
            string currentFileName = Name;
            WebClient webClient = new WebClient();

            if (Logger != null)
            {
                webClient.DownloadProgressChanged += (sender, e) =>
                {
                    lock (progressLockObj)
                    {
                        if (e.ProgressPercentage != 0)
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
            Logger.LogInfo($"{Name} downloading...");

            if (!DirectoryExt.Exists(DownloadPath))
            {
                DirectoryExt.CreateDirectory(DownloadPath);
            }
            webClient.DownloadFileAsync(new Uri(Url), ArchiveName);

            do
            {
                Thread.Sleep(100);
            }
            while (!fileDownloaded);

            Logger.LogInfo($"{Name} downloaded.");
        }
    }
}
