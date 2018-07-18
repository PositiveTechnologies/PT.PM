using Newtonsoft.Json;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace PT.PM.Common
{
    public static class CommonUtils
    {
        private static bool? isRunningOnLinux = null;
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
                if (isRunningOnLinux == null)
                {
                    int p = (int)Environment.OSVersion.Platform;
                    isRunningOnLinux = (p == 4) || (p == 6) || (p == 128);
                }

                return isRunningOnLinux.Value;
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
