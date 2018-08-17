using System.IO;

namespace PT.PM.Common.Utils
{
    public static class PathUtils
    {
        public const int MaxDirLength = 248 - 1;
        public const int MaxPathLength = 260 - 1;

        public static string NormalizePath(this string path, bool isDirectory = true, bool force = true)
        {
            if (CommonUtils.IsWindows && !CommonUtils.IsCoreApp && !path.StartsWith(@"\\?\") &&
                (force || path.Length > (isDirectory ? MaxDirLength : MaxPathLength)))
            {
                if (path.StartsWith(@"\\"))
                {
                    return $@"\\?\UNC\{path.TrimStart('\\')}";
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
