using PT.PM.Common;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PT.PM.Common
{
    public class WildcardConverter
    {
        public bool IsWholeWord { get; set; } = false;

        public bool IsCaseInsensitive { get; set; } = true;

        public Regex Convert(string wildcard)
        {
            string regex, escapedDirSeparatorChar;
            if (Path.DirectorySeparatorChar == '/')
            {
                regex = wildcard.Replace('\\', '/');
                escapedDirSeparatorChar = "/";
            }
            else
            {
                regex = wildcard.Replace("/", "\\").Replace("\\", "\\\\");
                escapedDirSeparatorChar = "\\\\";
            }
            regex = regex.Replace("**", "||");
            regex = regex.Replace("*", "|");
            regex = regex.Replace(".", "\\.");
            regex = regex.Replace("||", ".*");
            regex = regex.Replace("|", $"[^{escapedDirSeparatorChar}]*");
            if (IsWholeWord)
            {
                regex = $"^({regex})$";
            }
            if (IsCaseInsensitive)
            {
                regex = "(?i)" + regex;
            }
            return new Regex(regex, RegexOptions.Compiled);
        }
    }
}
