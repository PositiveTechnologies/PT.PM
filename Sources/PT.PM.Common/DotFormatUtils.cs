using System.Text;

namespace PT.PM.Common
{
    public static class DotFormatUtils
    {
        private const int TrimLength = 20;

        public static void AppendEdge(this StringBuilder builder, int startInd, int endInd, string advanced = "")
        {
            builder.AppendLine($"{startInd}->{endInd}" + (advanced == "" ? "" : " " + advanced) + ";");
        }

        public static string TrimAndEscape(string str)
        {
            return EscapeString(TrimString(str));
        }

        public static string EscapeString(string str)
        {
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "");
        }

        public static string TrimString(string str)
        {
            if (str.Length > TrimLength)
            {
                str = str.Remove(TrimLength);
            }

            return str;
        }
    }
}
