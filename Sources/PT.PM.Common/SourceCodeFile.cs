using System;
using System.Collections.Generic;
using System.IO;

namespace PT.PM.Common
{
    public class SourceCodeFile
    {
        private readonly object lockObj = new object();
        private int[] lineIndexes;

        public const int StartLine = 1;

        public const int StartColumn = 1;

        public static SourceCodeFile Empty = new SourceCodeFile("");

        public string RootPath { get; set; } = "";

        public string RelativePath { get; set; } = "";

        public string Name { get; set; } = "";

        public string Code { get; }

        public string RelativeName => Path.Combine(RelativePath, Name);

        public string FullName => Path.Combine(RootPath, RelativePath, Name);

        public SourceCodeFile(string code)
        {
            Code = code ?? "";
        }

        public override string ToString() => RelativeName;

        public LineColumnTextSpan GetLineColumnTextSpan(TextSpan textSpan)
        {
            GetLineColumnFromLinear(textSpan.Start, out int beginLine, out int beginColumn);
            GetLineColumnFromLinear(textSpan.End, out int endLine, out int endColumn);

            return new LineColumnTextSpan(beginLine, beginColumn, endLine, endColumn);
        }

        public void GetLineColumnFromLinear(int position, out int line, out int column)
        {
            InitLineIndexesIfRequired();

            line = Array.BinarySearch(lineIndexes, position);
            if (line < 0)
            {
                line = (line == -1) ? 0 : (~line - 1);
            }

            column = position - lineIndexes[line] + StartColumn;
            line += StartLine;
        }

        public int GetLinearFromLineColumn(int line, int column)
        {
            InitLineIndexesIfRequired();

            return lineIndexes[line - StartLine] + column - StartColumn;
        }

        public int GetLineLinearIndex(int lineIndex)
        {
            InitLineIndexesIfRequired();

            return lineIndexes[lineIndex];
        }

        public int GetLinesCount()
        {
            InitLineIndexesIfRequired();

            return lineIndexes.Length;
        }

        private void InitLineIndexesIfRequired()
        {
            if (lineIndexes == null)
            {
                lock (lockObj)
                {
                    if (lineIndexes == null)
                    {
                        InitLineIndexes();
                    }
                }
            }
        }

        private void InitLineIndexes()
        {
            int currentLine = StartLine;
            int currentColumn = StartColumn;
            string text = Code;

            var lineIndexesBuffer = new List<int>(text.Length / 25) { 0 };
            int textIndex = 0;
            while (textIndex < text.Length)
            {
                char c = text[textIndex];
                if (c == '\r' || c == '\n')
                {
                    currentLine++;
                    currentColumn = StartColumn;
                    if (c == '\r' && textIndex + 1 < text.Length && text[textIndex + 1] == '\n')
                    {
                        textIndex++;
                    }
                    lineIndexesBuffer.Add(textIndex + 1);
                }
                else
                {
                    currentColumn++;
                }
                textIndex++;
            }

            lineIndexes = lineIndexesBuffer.ToArray();
        }
    }
}
