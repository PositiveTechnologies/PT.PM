using System;
using System.IO;

namespace PT.PM.Common.Exceptions
{
    public static class ExceptionUtils
    {
        public static string GetPrettyErrorMessage(this Exception ex, FileNameType fileNameType = FileNameType.None)
        {
            var pmException = ex as PMException;
            if (pmException != null)
            {
                return pmException.ToString(fileNameType);
            }

            return ex.FormatExceptionMessage();
        }

        public static string FormatExceptionMessage(this Exception ex)
        {
            if (ex == null)
            {
                return "";
            }

            string pathString = null;
            int line = 0;
            try
            {
                string stackTrace = ex.StackTrace;
                var index1 = stackTrace.IndexOf(":\\");
                var index2 = stackTrace.IndexOf(":", index1 + 1);
                index1--;
                pathString = stackTrace.Substring(index1, index2 - index1);
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
                line = int.Parse(stackTrace.Substring(digitIndex, index2 - digitIndex));
            }
            catch
            {
            }

            string result = pathString != null
                ? $"{ex.Message} at \"{pathString}\" (line: {line})"
                : ex.ToString();

            return result;
        }
    }
}
