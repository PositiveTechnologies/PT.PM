using Newtonsoft.Json;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace PT.PM.Common
{
    public static class CommonUtils
    {
        private static bool? isSupportThreadAbort = null;

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

        public static bool IsRunningOnLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public static bool IsSupportThreadAbort
        {
            get
            {
                if (!isSupportThreadAbort.HasValue)
                {
                    Thread thread = new Thread(() => { });
                    try
                    {
                        thread.Abort();
                        isSupportThreadAbort = true;
                    }
                    catch (PlatformNotSupportedException)
                    {
                        isSupportThreadAbort = false;
                    }
                }

                return isSupportThreadAbort.Value;
            }
        }

        public static bool ExistsOnPath(string fileName)
        {
            return GetFullPath(fileName) != null;
        }

        public static string GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var values = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in values.Split(';'))
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }

        public static bool TryCheckIdTokenValue(Expression expr, string value)
        {
            return expr is IdToken idToken && idToken.TextValue == value;
        }

        public static List<T> ParseEnums<T>(this IEnumerable<string> values, bool ignoreIncorrectValues, ILogger logger = null)
            where T : struct, IConvertible
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

        public static T ParseEnum<T>(this string str, bool ignoreIncorrectValue, T defaultValue = default(T), ILogger logger = null)
            where T : struct, IConvertible
        {
            if (ParseEnum(str, ignoreIncorrectValue, out T parsed, defaultValue, logger))
            {
                return parsed;
            }
            return defaultValue;
        }

        public static bool ParseEnum<T>(this string str, bool ignoreIncorrectValue, out T result, T defaultValue = default(T), ILogger logger = null)
            where T : struct, IConvertible
        {
            if (ignoreIncorrectValue)
            {
                if (Enum.TryParse(str, true, out T enumValue))
                {
                    result = enumValue;
                }
                else
                {
                    result = defaultValue;
                    logger?.LogError(new ArgumentException($"Incorrect enum value {str}"));
                    return false;
                }
            }
            else
            {
                result = (T)Enum.Parse(typeof(T), str);
            }
            return true;
        }

        public static int ConvertToInt32(this object obj, bool ignoreIncorrectValue, int defaultValue = default(int), ILogger logger = null)
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
