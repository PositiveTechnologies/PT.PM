using System;
using System.Collections.Generic;
using MessagePack;

namespace PT.PM.Common.Files
{
    [MessagePackObject]
    public class CodeFile : File<string>
    {
        private int[] lineIndexes;

        public const int StartLine = 1;

        public const int StartColumn = 1;

        public static CodeFile Empty => new CodeFile("");
        
        [IgnoreMember]
        public override FileType Type => FileType.CodeFile;

        [IgnoreMember]
        public override bool IsEmpty => Data.Length == 0;

        public CodeFile(string code)
            : base(code)
        {
            InitLineIndexes();
        }

        public override string ToString() => !string.IsNullOrEmpty(RelativeName)
            ? RelativeName
            : Data;

        public LineColumnTextSpan GetLineColumnTextSpan(TextSpan textSpan)
        {
            GetLineColumnFromLinear(textSpan.Start, out int beginLine, out int beginColumn);
            GetLineColumnFromLinear(textSpan.End, out int endLine, out int endColumn);

            return new LineColumnTextSpan(beginLine, beginColumn, endLine, endColumn, textSpan.CodeFile);
        }

        public void GetLineColumnFromLinear(int position, out int line, out int column)
        {
            line = Array.BinarySearch(lineIndexes, position);
            if (line < 0)
            {
                line = (line == -1) ? 0 : (~line - 1);
            }

            column = position - lineIndexes[line] + StartColumn;
            line += StartLine;
        }

        public TextSpan GetTextSpan(LineColumnTextSpan textSpan)
        {
            int start = GetLinearFromLineColumn(textSpan.BeginLine, textSpan.BeginColumn);
            int end = GetLinearFromLineColumn(textSpan.EndLine, textSpan.EndColumn);

            var result = TextSpan.FromBounds(start, end, textSpan.CodeFile);
            return result;
        }

        public int GetLinearFromLineColumn(int line, int column)
        {
            return lineIndexes[line - StartLine] + column - StartColumn;
        }

        public int GetLineLinearIndex(int lineIndex)
        {
            return lineIndexes[lineIndex];
        }

        public int GetLinesCount()
        {
            return lineIndexes.Length;
        }

        public string GetStringAtLine(int line)
        {
            line = line - StartLine;

            if (line < 0 || line >= lineIndexes.Length)
            {
               return string.Empty;
            }

            int endInd;
            if (line + 1 < lineIndexes.Length)
            {
                endInd = lineIndexes[line + 1] - 1;
                if (endInd - 1 > 0 && Data[endInd - 1] == '\r')
                {
                    endInd--;
                }
            }
            else
            {
                endInd = Data.Length;
            }

            return Data.Substring(lineIndexes[line], endInd - lineIndexes[line]);
        }

        public string GetSubstring(TextSpan textSpan)
        {
            if (textSpan.End > Data.Length)
                return "";

            return Data.Substring(textSpan.Start, textSpan.Length);
        }

        private void InitLineIndexes()
        {
            string text = Data;

            var lineIndexesBuffer = new List<int>(text.Length / 25) { 0 };
            int textIndex = 0;
            while (textIndex < text.Length)
            {
                char c = text[textIndex];
                if (c == '\r' || c == '\n')
                {
                    if (c == '\r' && textIndex + 1 < text.Length && text[textIndex + 1] == '\n')
                    {
                        textIndex++;
                    }
                    lineIndexesBuffer.Add(textIndex + 1);
                }
                textIndex++;
            }

            lineIndexes = lineIndexesBuffer.ToArray();
        }

        protected override int CompareData(string data1, string data2)
        {
            return String.Compare(data1, data2, StringComparison.Ordinal);
        }
    }
}
