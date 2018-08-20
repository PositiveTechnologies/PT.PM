using Newtonsoft.Json;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PT.PM.Common.Utils
{
    public static class CommonUtils
    {
        private static bool? isWindows = null;
        private static bool? isCoreApp = null;

        public const string Prefix = "pt.pm_";

        public static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            string formattedJson = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
            return formattedJson;
        }

        public static bool TryConvertToInt64(this string value, int fromBase, out long result)
        {

            try
            {
                result = Convert.ToInt64(value, fromBase);
                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }

        public static readonly bool IsDebug =
#if DEBUG
            true;
#else
            false;
#endif

        public static bool IsWindows
        {
            get
            {
                if (!isWindows.HasValue)
                {
                    isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                }

                return isWindows.Value;
            }
        }

        public static bool IsCoreApp
        {
            get
            {
                if (!isCoreApp.HasValue)
                {
                    isCoreApp = RuntimeInformation.FrameworkDescription.Contains(".NET Core");
                }

                return isCoreApp.Value;
            }
        }

        public static bool TryCheckIdTokenValue(Expression expr, string value)
        {
            return expr is IdToken idToken && idToken.TextValue == value;
        }

        public static List<T> ParseEnums<T>(this IEnumerable<string> values, bool ignoreIncorrectValues, ILogger logger = null)
            where T :  Enum
        {
            var result = new List<T>();
            foreach (string value in values)
            {
                if (ParseEnum(value, ignoreIncorrectValues, out T parsed, logger: logger))
                {
                    result.Add(parsed);
                }
            }
            return result;
        }

        public static T ParseEnum<T>(this string str, bool ignoreIncorrectValue, T defaultValue = default, ILogger logger = null)
            where T : Enum
        {
            if (ParseEnum(str, ignoreIncorrectValue, out T parsed, defaultValue, logger))
            {
                return parsed;
            }
            return defaultValue;
        }

        public static bool ParseEnum<T>(this string str, bool ignoreIncorrectValue, out T result, T defaultValue = default, ILogger logger = null)
            where T : Enum
        {
            if (ignoreIncorrectValue)
            {
                try
                {
                    result = (T)Enum.Parse(typeof(T), str, true);
                }
                catch
                {
                    result = defaultValue;
                    logger?.LogError(new ArgumentException($"Incorrect enum value {str}"));
                    return false;
                }
            }
            else
            {
                result = (T)Enum.Parse(typeof(T), str, true);
            }
            return true;
        }

        public static int ConvertToInt32(this uint obj, bool ignoreIncorrectValue, int defaultValue = default, ILogger logger = null)
        {
            try
            {
                return Convert.ToInt32(obj);
            }
            catch
            {
                if (ignoreIncorrectValue)
                {
                    logger?.LogError(new ArgumentException($"Incorrect int value {obj}"));
                    return defaultValue;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
