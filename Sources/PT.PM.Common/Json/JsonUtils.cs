using Newtonsoft.Json;
using PT.PM.Common.Exceptions;
using System;
using PT.PM.Common.Files;

namespace PT.PM.Common.Json
{
    public static class JsonUtils
    {
        public static void LogError(this ILogger logger, TextFile serializedFile, IJsonLineInfo jsonLineInfo, Exception ex, bool isError = true)
        {
            string errorMessage = GenerateErrorPositionMessage(serializedFile, jsonLineInfo, out TextSpan errorTextSpan) +
                                  "; " + ex.FormatExceptionMessage();
            LogErrorOrWarning(logger, serializedFile, isError, errorMessage, errorTextSpan);
        }

        public static void LogError(this ILogger logger, TextFile serializedFile, IJsonLineInfo jsonLineInfo, string message, bool isError = true)
        {
            string errorMessage = GenerateErrorPositionMessage(serializedFile, jsonLineInfo, out TextSpan errorTextSpan) +
                                  "; " + message;
            LogErrorOrWarning(logger, serializedFile, isError, errorMessage, errorTextSpan);
        }

        private static string GenerateErrorPositionMessage(TextFile serializedFile, IJsonLineInfo jsonLineInfo,
            out TextSpan errorTextSpan)
        {
            if (jsonLineInfo != null)
            {
                int errorLine = jsonLineInfo.LineNumber;
                int errorColumn = jsonLineInfo.LinePosition;
                errorTextSpan = new TextSpan(serializedFile.GetLinearFromLineColumn(errorLine, errorColumn), 0);
                return $"File position: {new LineColumnTextSpan(errorLine, errorColumn)}";
            }

            errorTextSpan = TextSpan.Zero;
            return "";
        }

        private static void LogErrorOrWarning(this ILogger logger, TextFile serializedFile, bool isError, string errorMessage, TextSpan errorTextSpan)
        {
            var exception = new ConversionException(serializedFile, null, errorMessage) { TextSpan = errorTextSpan };

            if (isError)
            {
                logger.LogError(exception);
            }
            else
            {
                logger.LogInfo($"{serializedFile}: " + errorMessage);
            }
        }
    }
}
