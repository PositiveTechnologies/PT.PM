using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Exceptions;
using System;
using System.Linq;

namespace PT.PM.Common.Json
{
    public static class JsonUtils
    {
        public static JToken[] GetTokenOrTokensArray(this JToken jToken)
        {
            return jToken is JArray jArray ? jArray.Children().ToArray() : new JToken[] { jToken };
        }

        public static void LogError(this ILogger logger, CodeFile jsonFile, IJsonLineInfo jsonLineInfo, Exception ex, bool isError = true)
        {
            string errorMessage = GenerateErrorPositionMessage(jsonFile, jsonLineInfo, out TextSpan errorTextSpan);
            errorMessage += "; " + ex.FormatExceptionMessage();
            LogErrorOrWarning(logger, jsonFile, isError, errorMessage, errorTextSpan);
        }

        public static void LogError(this ILogger logger, CodeFile jsonFile, IJsonLineInfo jsonLineInfo, string message, bool isError = true)
        {
            string errorMessage = GenerateErrorPositionMessage(jsonFile, jsonLineInfo, out TextSpan errorTextSpan);
            errorMessage += "; " + message;
            LogErrorOrWarning(logger, jsonFile, isError, errorMessage, errorTextSpan);
        }

        private static string GenerateErrorPositionMessage(CodeFile jsonFile, IJsonLineInfo jsonLineInfo, out TextSpan errorTextSpan)
        {
            int errorLine = CodeFile.StartLine;
            int errorColumn = CodeFile.StartColumn;
            if (jsonLineInfo != null)
            {
                errorLine = jsonLineInfo.LineNumber;
                errorColumn = jsonLineInfo.LinePosition;
            }
            errorTextSpan = new TextSpan(jsonFile.GetLinearFromLineColumn(errorLine, errorColumn), 0);

            return jsonLineInfo != null
                ? $"File position: {new LineColumnTextSpan(errorLine, errorColumn)}"
                : "";
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
