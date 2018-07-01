using System.Text;

namespace PT.PM.Common
{
    public static class DotFormatUtils
    {
        private const string Separator = ", ";

        public static void AppendEdge(this StringBuilder builder, int startInd, int endInd, string color = "", string style = "", string label = "")
        {
            var advancedStringBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(color))
            {
                advancedStringBuilder.Append($"color=");
                advancedStringBuilder.Append(color);
                advancedStringBuilder.Append(Separator);
            }
            if (!string.IsNullOrEmpty(style))
            {
                advancedStringBuilder.Append($"style=");
                advancedStringBuilder.Append(style);
                advancedStringBuilder.Append(Separator);
            }
            if (!string.IsNullOrEmpty(label))
            {
                advancedStringBuilder.Append("label=\"");
                advancedStringBuilder.Append(label.Escape());
                advancedStringBuilder.Append("\"");
                advancedStringBuilder.Append(Separator);
            }

            string advancedString = "";
            if (advancedStringBuilder.Length > 0)
            {
                advancedStringBuilder = advancedStringBuilder.Remove(advancedStringBuilder.Length - Separator.Length, Separator.Length);
                advancedString = $" [{advancedStringBuilder}]";
            }

            builder.AppendLine($"{startInd}->{endInd}{advancedString};");
        }
    }
}
