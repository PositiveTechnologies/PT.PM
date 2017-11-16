using Newtonsoft.Json;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using System;
using System.Collections.Generic;

namespace PT.PM.Common
{
    public static class CommonUtils
    {
        private static readonly char[] Separators = new char[] { ',', ' ', '\t', '\r', '\n' };
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

        public static bool TryCheckIdTokenValue(Expression expr, string value)
        {
            return expr is IdToken idToken && idToken.TextValue == value;
        }

        public static TEnum[] ParseCollection<TEnum>(this string str)
            where TEnum : struct
        {
            if (!string.IsNullOrEmpty(str))
            {
                var collection = new List<TEnum>();
                string[] strings = str.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                foreach (string elemString in strings)
                {
                    if (Enum.TryParse(elemString, true, out TEnum element))
                    {
                        collection.Add(element);
                    }
                }
                return collection.ToArray();
            }
            else
            {
                return ArrayUtils<TEnum>.EmptyArray;
            }
        }

        public static T ParseEnum<T>(this string str, T defaultValue = default(T))
        {
            if (string.IsNullOrEmpty(str))
            {
                return defaultValue;
            }

            return (T)Enum.Parse(typeof(T), str, true);
        }
    }
}
