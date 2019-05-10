using System;
using System.Runtime.CompilerServices;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using PT.PM.Common.Files;

namespace PT.PM.AntlrUtils
{
    public class LightInputStream : ICharStream
    {
        private readonly char[] data;

        /// <summary>How many characters are actually in the buffer</summary>
        private readonly int size;

        /// <summary>0..n-1 index into string of next char</summary>
        private int position;

        public IVocabulary Vocabulary { get; }

        public CaseInsensitiveType CaseInsensitiveType { get; }

        public TextFile TextFile { get; }

        public LightInputStream(IVocabulary vocabulary, TextFile textFile, string input, CaseInsensitiveType caseInsensitiveType = CaseInsensitiveType.None)
        {
            Vocabulary = vocabulary ?? throw new ArgumentNullException(nameof(vocabulary));
            TextFile = textFile ?? throw new ArgumentNullException(nameof(textFile));

            CaseInsensitiveType = caseInsensitiveType;

            data = new char[input.Length];
            int i = 0;
            while (i < input.Length)
            {
                if (input[i] == '\r')
                {
                    if (i + 1 >= input.Length || input[i + 1] != '\n')
                    {
                        data[i] = '\n';
                    }
                    else
                    {
                        data[i] = ConvertCase(input[i]);
                    }
                }
                else
                {
                    data[i] = ConvertCase(input[i]);
                }

                i++;
            }

            size = input.Length;
        }

        public void Consume()
        {
            if (position >= size)
                throw new InvalidOperationException("cannot consume EOF");
            if (position >= size)
                return;
            ++position;
        }

        public int La(int i)
        {
            if (i == 0)
            {
                return 0; // undefined
            }
            if (i < 0)
            {
                i++; // e.g., translate LA(-1) to use offset i=0; then data[p+0-1]
                if (position + i - 1 < 0)
                {
                    return IntStreamConstants.Eof; // invalid; no char before first char
                }
            }

            if (position + i - 1 >= size)
            {
                return IntStreamConstants.Eof;
            }

            return data[position + i - 1];
        }

        /// <summary>
        /// Return the current input symbol index 0..n where n indicates the
        /// last symbol has been read.
        /// </summary>
        /// <remarks>
        /// Return the current input symbol index 0..n where n indicates the
        /// last symbol has been read.  The index is the index of char to
        /// be returned from LA(1).
        /// </remarks>
        public int Index => position;

        public int Size => size;

        /// <summary>mark/release do nothing; we have entire buffer</summary>
        public int Mark() => -1;

        public string SourceName => TextFile.RelativeName;

        public void Release(int marker)
        {
        }

        /// <summary>
        /// consume() ahead until p==index; can't just set p=index as we must
        /// update line and charPositionInLine.
        /// </summary>
        /// <remarks>
        /// consume() ahead until p==index; can't just set p=index as we must
        /// update line and charPositionInLine. If we seek backwards, just set p
        /// </remarks>
        public void Seek(int index)
        {
            if (index <= position)
            {
                position = index;
            }
            else
            {
                index = Math.Min(index, size);
                while (position < index)
                    Consume();
            }
        }

        public string GetText(Interval interval)
        {
            int a = interval.a;
            int num = interval.b;
            if (num >= size)
                num = size - 1;
            int length = num - a + 1;
            if (a >= size)
                return string.Empty;
            return TextFile.Data.Substring(a, length);
        }

        public override string ToString() => TextFile.Data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char ConvertCase(char result)
        {
            return CaseInsensitiveType == CaseInsensitiveType.None
                ? result
                : CaseInsensitiveType == CaseInsensitiveType.UPPER
                    ? char.ToUpperInvariant(result)
                    : char.ToLowerInvariant(result);
        }
    }
}