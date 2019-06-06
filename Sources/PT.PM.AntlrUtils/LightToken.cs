using System;
using Antlr4.Runtime;
using PT.PM.Common;
using PT.PM.Common.Files;

namespace PT.PM.AntlrUtils
{
    // TODO: should store TextSpan instead of StartIndex and StopIndex
    public readonly struct LightToken : IToken
    {
        private readonly LightInputStream inputStream;
        private readonly short type;
        private readonly short channel;

        public int StartIndex { get; }

        public int StopIndex { get; }

        public int TokenIndex { get; }

        public LightToken(LightInputStream inputStream, int type, int channel, int index, int start, int stop)
        {
            this.inputStream = inputStream;
            this.type = (short) type;
            this.channel = (short) channel;
            StartIndex = start;
            StopIndex = stop;
            TokenIndex = index;
        }

        public string Text => Type == Lexer.Eof
            ? "EOF"
            : inputStream.TextFile.Data.Substring(StartIndex, StopIndex - StartIndex);

        public ReadOnlySpan<char> Span => Type == Lexer.Eof
            ? ReadOnlySpan<char>.Empty
            : inputStream.TextFile.Data.AsSpan(StartIndex, StopIndex - StartIndex);

        public TextFile TextFile => inputStream.TextFile;

        public TextSpan TextSpan => new TextSpan(StartIndex, StopIndex - StartIndex);

        public LineColumnTextSpan LineColumnTextSpan => inputStream.TextFile.GetLineColumnTextSpan(TextSpan);

        public string TypeName => inputStream.Vocabulary.GetDisplayName(Type);

        public int Type => type;

        public int Channel => channel;

        public int Line
        {
            get
            {
                inputStream.TextFile.GetLineColumnFromLinear(StartIndex, out int line, out _);
                return line;
            }
        }

        public int Column
        {
            get
            {
                inputStream.TextFile.GetLineColumnFromLinear(StartIndex, out _, out int column);
                return column;
            }
        }

        public ICharStream InputStream => inputStream;

        public ITokenSource TokenSource => new LightTokenSource(inputStream);

        public override string ToString()
        {
            return $"Type: {TypeName}; Text: `{Text}`; Channel: {Channel}";
        }
    }
}