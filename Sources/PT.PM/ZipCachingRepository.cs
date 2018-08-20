using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace PT.PM
{
    public class ZipCachingRepository : DirectoryCodeRepository
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

                            if (DirectoryExt.Exists(RootPath))
                            {
                                DirectoryExt.Delete(RootPath);
                            }

                            IArchiveExtractor extractor = SevenZipExtractor.Is7zInstalled
                                ? (IArchiveExtractor)new SevenZipExtractor()
                                : new StandardArchiveExtractor();

                            extractor.Logger = Logger;
                            extractor.Extract(ArchiveName, RootPath);

                            if (RemoveAfterExtraction)
                            {
                                FileExt.Delete(ArchiveName);
                            }
                            Logger.LogInfo($"{Name} extracted.");

                            string[] directories = DirectoryExt.GetDirectories(RootPath);
                            if (directories.Length == 1)
                            {
                                DirectoryExt.CreateDirectory(RootPath);

                                foreach (string fileSystemEntry in DirectoryExt.EnumerateFileSystemEntries(directories[0]))
                                {
                                    string shortName = Path.GetFileName(fileSystemEntry);
                                    string newName = Path.Combine(RootPath, shortName);
                                    if (FileExt.Exists(fileSystemEntry))
                                    {
                                        FileExt.Move(fileSystemEntry, newName);
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

                                DirectoryExt.Delete(directories[0]);
                            }
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
            if (!DirectoryExt.Exists(RootPath))
            {
                return true;
            }

            IEnumerable<string> items = DirectoryExt.EnumerateFileSystemEntries(RootPath);
            using (IEnumerator<string> en = items.GetEnumerator())
            {
                return !en.MoveNext();
            }
        }
    }
}
