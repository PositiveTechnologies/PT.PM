using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PT.PM.Common
{
    public class TextTruncater
    {
        private static string[] newLines = new string[] { "\r\n", "\n" };

        public int MaxMessageLength { get; set; } = 200;

        public double StartRatio { get; set; } = 0.5;

        public string Splitter { get; set; } = " ... ";

        public bool CutWords { get; set; } = false;

        public bool TrimIndent { get; set; } = false;

        public string Trunc(string message)
        {
            if (TrimIndent)
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

            if (message.Length > MaxMessageLength)
            {
                int startLength = (int)Math.Round(MaxMessageLength * StartRatio);
                int endLength;
                if (!CutWords)
                {
                    int newStartLength = message.LastIndexOf(message.LastIndexOf(startLength - 1, false), true) + 1;
                    startLength = newStartLength == 0 && !char.IsWhiteSpace(message[0])
                        ? startLength
                        : newStartLength;

                    int endIndex = message.Length - (MaxMessageLength - startLength - Splitter.Length);
                    int newEndLength = message.Length - message.FirstIndexOf(message.FirstIndexOf(endIndex, false), true);
                    endLength = newEndLength == 0 && !char.IsWhiteSpace(message[message.Length - 1])
                        ? MaxMessageLength - startLength - Splitter.Length
                        : newEndLength;
                }
                else
                {
                    endLength = MaxMessageLength - startLength - Splitter.Length;
                }
                return message.Substring(0, startLength) +
                       Splitter +
                       message.Substring(message.Length - endLength, endLength);
            }
            return message;
        }
    }
}
