using System.Collections.Generic;
using System.IO;

namespace PT.PM.Common.Utils
{
    public static class PathUtils
    {
        public const int MaxDirLength = 248 - 1;
        public const int MaxPathLength = 260 - 1;

        public static string NormalizeFilePath(this string path) => NormalizePath(path, false);

        public static string NormalizeDirPath(this string path, bool force = false) => NormalizePath(path, true, force);

        private static string NormalizePath(this string path, bool isDirectory = true, bool force = false)
        {
            if ((path.Length > (isDirectory ? MaxDirLength : MaxPathLength) || force) &&
                CommonUtils.IsWindows && !CommonUtils.IsCoreApp && !path.StartsWith(@"\\?\"))
            {
                if (path.StartsWith(@"\\"))
                {
                    return $@"\\?\UNC\{path.Substring(2)}";
                }

                path = path.NormalizeDirSeparator();

                return $@"\\?\{path}";
            }

            return path.NormalizeDirSeparator();
        }

        public static string NormalizeDirSeparator(this string path)
        {
            string notPlatformSeparator = CommonUtils.IsWindows ? "/" : "\\";

            if (path.Contains(notPlatformSeparator))
            {
                return path.Replace(notPlatformSeparator, Path.DirectorySeparatorChar.ToString());
            }

            return path;
        }
    }

    public static class FileExt
    {
        public static string ReadAllText(string path) => File.ReadAllText(path.NormalizeFilePath());

        public static void WriteAllText(string path, string contents) => File.WriteAllText(path.NormalizeFilePath(), contents);

        public static void WriteAllLines(string path, IEnumerable<string> contents) => File.WriteAllLines(path.NormalizeFilePath(), contents);

        public static bool Exists(string path) => File.Exists(path.NormalizeFilePath());

        public static void Delete(string path) => File.Delete(path.NormalizeFilePath());

        public static void Move(string sourceFileName, string destFileName) => File.Move(sourceFileName.NormalizeFilePath(), destFileName.NormalizeFilePath());
    }

    public static class DirectoryExt
    {
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) => Directory.EnumerateFiles(path.NormalizeDirPath(true), searchPattern, searchOption);

        public static IEnumerable<string> EnumerateFileSystemEntries(string path) => Directory.EnumerateFileSystemEntries(path.NormalizeDirPath(true));

        public static string[] GetDirectories(string path) => Directory.GetDirectories(path.NormalizeDirPath(true));

        public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption) => Directory.GetFiles(path.NormalizeDirPath(true), searchPattern, searchOption);

        public static DirectoryInfo CreateDirectory(string path) => Directory.CreateDirectory(path.NormalizeDirPath());

        public static bool Exists(string path) => Directory.Exists(path.NormalizeDirPath());

        public static void Delete(string path) => Directory.Delete(path.NormalizeDirPath(true), true);

        public static void Move(string sourceDirName, string destDirName) => Directory.Move(sourceDirName.NormalizeDirPath(true), destDirName.NormalizeDirPath(true));
    }
}
