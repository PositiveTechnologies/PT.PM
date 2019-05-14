using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using PT.PM.Common.Files;

namespace PT.PM.AntlrUtils
{
    public class LightInputStream : ICharStream
    {
        private readonly char[] data;

        public IVocabulary Vocabulary { get; }

        public CaseInsensitiveType CaseInsensitiveType { get; }

        public TextFile TextFile { get; }

        /// <summary>
        /// Return the current input symbol index 0..n where n indicates the
        /// last symbol has been read.
        /// </summary>
        /// <remarks>
        /// Return the current input symbol index 0..n where n indicates the
        /// last symbol has been read.  The index is the index of char to
        /// be returned from LA(1).
        /// </remarks>
        public int Index { get; private set; }

        public int Size { get; }

        public LightInputStream(IVocabulary vocabulary, TextFile textFile, string input, CaseInsensitiveType caseInsensitiveType = CaseInsensitiveType.None)
        {
            Vocabulary = vocabulary ?? throw new ArgumentNullException(nameof(vocabulary));
            TextFile = textFile ?? throw new ArgumentNullException(nameof(textFile));

            CaseInsensitiveType = caseInsensitiveType;

            data = new char[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\r' &&
                    (i + 1 >= input.Length || input[i + 1] != '\n'))
                {
                    data[i] = '\n';
                }
                else
                {
                    data[i] = CaseInsensitiveType == CaseInsensitiveType.None
                              ? input[i]
                              : CaseInsensitiveType == CaseInsensitiveType.UPPER
                                  ? char.ToUpperInvariant(input[i])
                                  : char.ToLowerInvariant(input[i]);
                }
            }

            Size = input.Length;
        }

        public void Consume()
        {
            if (Index >= Size)
                throw new InvalidOperationException("cannot consume EOF");
            ++Index;
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
                if (Index + i - 1 < 0)
                {
                    return IntStreamConstants.Eof; // invalid; no char before first char
                }
            }

            if (Index + i - 1 >= Size)
            {
                return IntStreamConstants.Eof;
            }

            return data[Index + i - 1];
        }

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
            if (index <= Index)
            {
                Index = index;
            }
            else
            {
                index = Math.Min(index, Size);
                while (Index < index)
                    Consume();
            }
        }

        public string GetText(Interval interval)
        {
            int start = interval.a;
            int end = Math.Min(interval.b, Size - 1);
            int length = end - start + 1;
            if (start >= Size)
                return string.Empty;
            return TextFile.Data.Substring(start, length);
        }

        public override string ToString() => TextFile.Data;
    }
}