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

                            string normRootPath = RootPath.NormalizeDirPath(true);
                            if (Directory.Exists(normRootPath))
                            {
                                Directory.Delete(normRootPath, true);
                            }

                            IArchiveExtractor extractor = SevenZipExtractor.Is7zInstalled
                                ? (IArchiveExtractor)new SevenZipExtractor()
                                : new StandardArchiveExtractor();

                            extractor.Logger = Logger;
                            extractor.Extract(ArchiveName, RootPath);

                            if (RemoveAfterExtraction)
                            {
                                File.Delete(ArchiveName.NormalizeFilePath());
                            }
                            Logger.LogInfo($"{Name} extracted.");

                            string[] directories = Directory.GetDirectories(normRootPath);
                            if (directories.Length == 1)
                            {
                                Directory.CreateDirectory(normRootPath);

                                foreach (string fileSystemEntry in Directory.EnumerateFileSystemEntries(directories[0].NormalizeDirPath()))
                                {
                                    string normFileSystemEntry = fileSystemEntry.NormalizeDirPath();
                                    string shortName = Path.GetFileName(normFileSystemEntry);
                                    string newName = Path.Combine(RootPath, shortName);
                                    if (File.Exists(normFileSystemEntry))
                                    {
                                        File.Move(normFileSystemEntry, newName);
                                    }
                                    else
                                    {
                                        try
                                        {
                                            Directory.Move(normFileSystemEntry, newName);
                                        }
                                        catch
                                        {
                                            Directory.Move(normFileSystemEntry, newName);
                                        }
                                    }
                                }

                                Directory.Delete(directories[0].NormalizeDirPath(true), true);
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
            string normDirPath = RootPath.NormalizeDirPath();

            if (!Directory.Exists(normDirPath))
            {
                return true;
            }

            IEnumerable<string> items = Directory.EnumerateFileSystemEntries(normDirPath);
            using (IEnumerator<string> en = items.GetEnumerator())
            {
                return !en.MoveNext();
            }
        }
    }
}
