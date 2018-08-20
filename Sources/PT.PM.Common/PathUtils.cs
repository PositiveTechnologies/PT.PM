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
            if (CommonUtils.IsWindows && !CommonUtils.IsCoreApp && !path.StartsWith(@"\\?\") &&
                (path.Length > (isDirectory ? MaxDirLength : MaxPathLength) || force))
            {
                if (path.StartsWith(@"\\"))
                {
                    return $@"\\?\UNC\{path.Remove(2)}";
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
}
