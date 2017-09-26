using Newtonsoft.Json;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using System;

namespace PT.PM.Common
{
    public static class CommonUtils
    {
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

        static public bool TryCheckIdTokenValue(Expression expr, string value)
        {
            return expr is IdToken idToken && idToken.TextValue == value;
        }
    }
}
