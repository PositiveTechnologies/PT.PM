using PT.PM.Common.Nodes;
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

        public static string GetNodeName(Ust node)
        {
            return TrimAndEscapeString(node.ToString());
        }

        public static string TrimAndEscapeString(string str)
        {
            if (str.Length > TrimLength)
            {
                str = str.Remove(TrimLength);
            }
            str = str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "");
            return str;
        }
    }
}
