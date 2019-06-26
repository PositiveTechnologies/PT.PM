using System;
using System.Globalization;
using System.IO;

namespace PT.PM.Common.Exceptions
{
    public static class ExceptionUtils
    {
        public static string GetPrettyErrorMessage(this Exception ex, FileNameType fileNameType = FileNameType.None)
        {
            if (ex is PMException pmException)
            {
                return pmException.ToString(fileNameType, false);
            }

            return ex.FormatExceptionMessage();
        }

        public static string FormatExceptionMessage(this Exception ex)
        {
            if (ex == null)
            {
                return "";
            }

            string pathString = ExtractPathString(ex, out int line);

            string result = pathString != null
                ? $"{ex.Message} at \"{pathString}\" (line: {line})"
                : ex.ToString();

            return result;
        }

        private static string ExtractPathString(Exception ex, out int line)
        {
            line = 0;
            try
            {
                string stackTrace = ex.StackTrace;
                if (stackTrace == null)
                {
                    return null;
                }

                var index1 = stackTrace.IndexOf(":\\", StringComparison.Ordinal);
                if (index1 < 0)
                {
                    return null;
                }

                var index2 = stackTrace.IndexOf(":", index1 + 1, StringComparison.Ordinal);
                if (index2 < 0)
                {
                    return null;
                }

                index1--;
                string pathString = stackTrace.Substring(index1, index2 - index1);
                pathString = Path.GetFileName(pathString);

                while (index2 < stackTrace.Length && !char.IsDigit(stackTrace[index2]))
                {
                    index2++;
                }
                int digitIndex = index2;
                while (index2 < stackTrace.Length && char.IsDigit(stackTrace[index2]))
                {
                    index2++;
                }
                line = int.Parse(stackTrace.Substring(digitIndex, index2 - digitIndex), CultureInfo.InvariantCulture);

                return pathString;
            }
            catch
            {
            }

            return null;
        }
    }
}
