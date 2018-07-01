using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PT.PM.Common
{
    public class PrettyPrinter
    {
        private static readonly string[] newLines = new string[] { "\r\n", "\n" };
        private static readonly Regex wsRegex = new Regex(@"\s+", RegexOptions.Compiled);

        public int MaxMessageLength { get; set; } = 200;

        public double StartRatio { get; set; } = 0.5;

        public string Splitter { get; set; } = " ... ";

        public bool Trim { get; set; } = true;

        public bool CutWords { get; set; } = false;

        public bool ReduceWhitespaces { get; set; } = false;

        public bool TrimIndent { get; set; } = false;

        public bool Escape { get; set; } = false;

        public string Print(string message)
        {
            if (Trim)
            {
                message = message.Trim();
            }

            if (ReduceWhitespaces)
            {
                message = wsRegex.Replace(message, " ");
            }
            else if (TrimIndent)
            {
                string lastLine = message.Split(newLines, StringSplitOptions.None).Last();

                int firstNotWsIndex = 0;
                while (firstNotWsIndex < lastLine.Length && (lastLine[firstNotWsIndex] == ' ' || lastLine[firstNotWsIndex] == '\t'))
                    firstNotWsIndex++;
                
                if (firstNotWsIndex != 0)
                {
                    message = Regex.Replace(message, $@"(\r?\n)([ \t]{{{firstNotWsIndex}}})", "$1");
                }
            }

            if (MaxMessageLength != 0 && message.Length > MaxMessageLength)
            {
                int startLength = (int)Math.Round(MaxMessageLength * StartRatio);
                int endLength;
                if (!CutWords)
                {
                    int newStartLength = message.LastIndexOf(message.LastIndexOf(startLength - 1, false), true) + 1;
                    startLength = newStartLength == 0 && !char.IsWhiteSpace(message[0])
                        ? startLength
                        : newStartLength;

                    endLength = MaxMessageLength - startLength - Splitter.Length;
                    int endIndex = message.Length - endLength;
                    int newEndLength = message.Length - message.FirstIndexOf(message.FirstIndexOf(endIndex, false), true);
                    endLength = newEndLength == 0 && !char.IsWhiteSpace(message[message.Length - 1])
                        ? endLength
                        : newEndLength;
                }
                else
                {
                    endLength = MaxMessageLength - startLength - Splitter.Length;
                }
                if (endLength < 0)
                {
                    endLength = 0;
                }
                message = message.Substring(0, startLength) +
                       Splitter +
                       message.Substring(message.Length - endLength, endLength);
            }

            if (Escape)
            {
                message = message.Escape().Replace("\r", "").Replace("\n", "");
            }

            return message;
        }
    }
}
