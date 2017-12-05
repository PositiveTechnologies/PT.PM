using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace PT.PM
{
    public class ZipCachingRepository : FilesAggregatorCodeRepository
    {
        public bool Rewrite { get; set; } = false;

        public bool RemoveAfterExtraction { get; set; } = false;

        public string Name { get; protected set; }

        public string ArchiveName { get; protected set; }

        public string ExtractPath { get; set; } = Path.GetTempPath();

        public ZipCachingRepository(string archivePath, params Language[] languages)
            : base(archivePath, languages)
        {
            ArchiveName = archivePath;
            Name = Path.GetFileNameWithoutExtension(archivePath);
        }

        public override IEnumerable<string> GetFileNames()
        {
            UnpackIfRequired();
            return base.GetFileNames();
        }

        private void UnpackIfRequired()
        {
            RootPath = Path.Combine(ExtractPath, Name);

            if (Rewrite || IsDirectoryNotExistsOrEmpty(RootPath))
            {
                using (var zipFileNameMutex = new Mutex(false, ConvertToValidMutexName(ArchiveName)))
                {
                    if (!zipFileNameMutex.WaitOne())
                    {
                        return;
                    }

                    try
                    {
                        if (Rewrite || IsDirectoryNotExistsOrEmpty(RootPath))
                        {
                            Logger.LogInfo($"{Name} extraction...");

                            if (Directory.Exists(RootPath))
                            {
                                Directory.Delete(RootPath, true);
                            }

                            var sevenZipExtractor = new SevenZipExtractor();
                            if (!CommonUtils.IsRunningOnLinux && File.Exists(sevenZipExtractor.SevenZipPath))
                            {
                                sevenZipExtractor.Extract(ArchiveName, RootPath);
                            }
                            else
                            {
                                new StandardArchiveExtractor().Extract(ArchiveName, RootPath);
                            }

                            if (RemoveAfterExtraction)
                            {
                                File.Delete(ArchiveName);
                            }
                            Logger.LogInfo($"{Name} has been extracted.");

                            string[] directories = Directory.GetDirectories(RootPath);
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
                                        try
                                        {
                                            Directory.Move(fileSystemEntry, newName);
                                        }
                                        catch
                                        {
                                            Directory.Move(fileSystemEntry, newName);
                                        }
                                    }
                                }

                                Directory.Delete(directories[0], true);
                            }
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

        protected static string ConvertToValidMutexName(string name)
        {
            return ConvertToValidFileName(name);
        }

        protected static string ConvertToValidFileName(string str)
        {
            StringBuilder result = new StringBuilder(str.Length);
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in str)
            {
                result.Append(!invalidChars.Contains(c) ? c : '-');
            }
            return result.ToString();
        }

        protected bool IsDirectoryNotExistsOrEmpty(string directoryName)
        {
            return !Directory.Exists(RootPath) || IsDirectoryEmpty(RootPath);
        }

        private static bool IsDirectoryEmpty(string path)
        {
            IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);
            using (IEnumerator<string> en = items.GetEnumerator())
            {
                return !en.MoveNext();
            }
        }
    }
}
