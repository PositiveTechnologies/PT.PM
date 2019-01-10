using System.Collections.Generic;
using System.IO;

namespace PT.PM.Common.Utils
{
    public static class PathUtils
    {
        public const string LongPrefix = @"\\?\";

        public const int MaxDirLength = 248 - 1;
        public const int MaxPathLength = 260 - 1;

        public static string NormalizeFilePath(this string path) => NormalizePath(path, false);

        public static string NormalizeDirPath(this string path, bool force = false) => NormalizePath(path, true, force);

        private static string NormalizePath(this string path, bool isDirectory = true, bool force = false)
        {
            path = Path.GetFullPath(path);

            if ((path.Length > (isDirectory ? MaxDirLength : MaxPathLength) || force) &&
                CommonUtils.IsWindows && !CommonUtils.IsCoreApp && !path.StartsWith(LongPrefix))
            {
                if (path.StartsWith(@"\\"))
                {
                    return $@"{LongPrefix}UNC\{path.Substring(2)}";
                }

                path = path.NormalizeDirSeparator();

                return $"{LongPrefix}{path}";
            }

            return path.NormalizeDirSeparator();
        }

        public static string NormalizeDirSeparator(this string path)
        {
            char notPlatformSeparator = CommonUtils.IsWindows ? '/' : '\\';

            foreach (char c in path)
            {
                if (c == notPlatformSeparator)
                {
                    return path.Replace(notPlatformSeparator, Path.DirectorySeparatorChar);
                }
            }

            return path;
        }

        public static string DenormalizePath(this string path)
        {
            if (path.StartsWith(LongPrefix))
            {
                return path.Substring(LongPrefix.Length);
            }

            return path;
        }
    }

    public static class FileExt
    {
        public static string ReadAllText(string path) => File.ReadAllText(path.NormalizeFilePath());

        public static byte[] ReadAllBytes(string path) => File.ReadAllBytes(path.NormalizeFilePath());

        public static void WriteAllText(string path, string contents) => File.WriteAllText(path.NormalizeFilePath(), contents);

        public static void WriteAllLines(string path, IEnumerable<string> contents) => File.WriteAllLines(path.NormalizeFilePath(), contents);

        public static void WriteAllBytes(string path, byte[] content) => File.WriteAllBytes(path.NormalizeDirPath(), content);

        public static bool Exists(string path) => File.Exists(path.NormalizeFilePath());

        public static void Delete(string path) => File.Delete(path.NormalizeFilePath());

        public static void Move(string sourceFileName, string destFileName) => File.Move(sourceFileName.NormalizeFilePath(), destFileName.NormalizeFilePath());
    }

    public static class DirectoryExt
    {
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            IEnumerable<string> files = Directory.EnumerateFiles(path.NormalizeDirPath(true), searchPattern, searchOption);

            if (!path.StartsWith(PathUtils.LongPrefix))
            {
                foreach (string file in files)
                {
                    yield return file.DenormalizePath();
                }
            }
            else
            {
                foreach (string file in files)
                {
                    yield return file;
                }
            }
        }

        public static IEnumerable<string> EnumerateFileSystemEntries(string path)
        {
            IEnumerable<string> fileSystemEntries = Directory.EnumerateFileSystemEntries(path.NormalizeDirPath(true));

            if (!path.StartsWith(PathUtils.LongPrefix))
            {
                foreach (string fileSystemEntry in fileSystemEntries)
                {
                    yield return fileSystemEntry.DenormalizePath();
                }
            }
            else
            {
                foreach (string fileSystemEntry in fileSystemEntries)
                {
                    yield return fileSystemEntry;
                }
            }
        }

        public static string[] GetDirectories(string path)
        {
            string[] dirs = Directory.GetDirectories(path.NormalizeDirPath(true));

            if (!path.StartsWith(PathUtils.LongPrefix))
            {
                for (int i = 0; i < dirs.Length; i++)
                {
                    dirs[i] = dirs[i].DenormalizePath();
                }
            }

            return dirs;
        }

        public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            string[] files = Directory.GetFiles(path.NormalizeDirPath(true), searchPattern, searchOption);

            if (!path.StartsWith(PathUtils.LongPrefix))
            {
                for (int i = 0; i < files.Length; i++)
                {
                    files[i] = files[i].DenormalizePath();
                }
            }

            return files;
        }

        public static DirectoryInfo CreateDirectory(string path) => Directory.CreateDirectory(path.NormalizeDirPath());

        public static bool Exists(string path) => Directory.Exists(path.NormalizeDirPath());

        public static void Delete(string path) => Directory.Delete(path.NormalizeDirPath(true), true);

        public static void Move(string sourceDirName, string destDirName) => Directory.Move(sourceDirName.NormalizeDirPath(true), destDirName.NormalizeDirPath(true));
    }
}
