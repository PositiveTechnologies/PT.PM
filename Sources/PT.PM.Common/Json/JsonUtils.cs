using Newtonsoft.Json;
using PT.PM.Common.Exceptions;
using System;

namespace PT.PM.Common.Json
{
    public static class JsonUtils
    {
        public static void LogError(this ILogger logger, CodeFile jsonFile, IJsonLineInfo jsonLineInfo, Exception ex, bool isError = true)
        {
            string errorMessage = GenerateErrorPositionMessage(jsonFile, jsonLineInfo, out TextSpan errorTextSpan) +
                                  "; " + ex.FormatExceptionMessage();
            LogErrorOrWarning(logger, jsonFile, isError, errorMessage, errorTextSpan);
        }

        public static void LogError(this ILogger logger, CodeFile jsonFile, IJsonLineInfo jsonLineInfo, string message, bool isError = true)
        {
            string errorMessage = GenerateErrorPositionMessage(jsonFile, jsonLineInfo, out TextSpan errorTextSpan) +
                                  "; " + message;
            LogErrorOrWarning(logger, jsonFile, isError, errorMessage, errorTextSpan);
        }

        private static string GenerateErrorPositionMessage(CodeFile jsonFile, IJsonLineInfo jsonLineInfo,
            out TextSpan errorTextSpan)
        {
            if (jsonLineInfo != null)
            {
                int errorLine = jsonLineInfo.LineNumber;
                int errorColumn = jsonLineInfo.LinePosition;
                errorTextSpan = new TextSpan(jsonFile.GetLinearFromLineColumn(errorLine, errorColumn), 0);
                return $"File position: {new LineColumnTextSpan(errorLine, errorColumn)}";
            }

            errorTextSpan = TextSpan.Zero;
            return "";
        }

        private static void LogErrorOrWarning(this ILogger logger, CodeFile jsonFile, bool isError, string errorMessage, TextSpan errorTextSpan)
        {
            var exception = new ConversionException(jsonFile, null, errorMessage) { TextSpan = errorTextSpan };

            if (isError)
            {
                logger.LogError(exception);
            }
            else
            {
                logger.LogInfo($"{jsonFile}: " + errorMessage);
            }
        }
    }
}
