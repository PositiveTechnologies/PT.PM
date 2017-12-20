using System.Text;

namespace PT.PM.Common
{
    public static class DotFormatUtils
    {
        public static void AppendEdge(this StringBuilder builder, int startInd, int endInd, string advanced = "")
        {
            builder.AppendLine($"{startInd}->{endInd}" + (advanced == "" ? "" : " " + advanced) + ";");
        }
    }
}
