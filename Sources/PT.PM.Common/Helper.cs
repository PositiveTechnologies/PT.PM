using PT.PM.Common.Nodes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Common
{
    public static class Helper
    {
        public const string Prefix = "pt.pm_";

        public static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            string formattedJson = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
            return formattedJson;
        }

        public static TextSpan GetTextSpan(this IEnumerable<UstNode> nodes)
        {
            if (nodes.Count() == 0)
            {
                return default(TextSpan);
            }
            else
            {
                return nodes.First().TextSpan.Union(nodes.Last().TextSpan);
            }
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
            if (expr?.NodeType != NodeType.IdToken)
            {
                return false;
            }
            return ((IdToken)expr).TextValue == value;
        }
    }
}
