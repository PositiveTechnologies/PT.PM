using System;

namespace PT.PM.Common
{
    public abstract class ParseTree
    {
        public abstract Language SourceLanguage { get; }

        public CodeFile SourceCodeFile { get; set; }

        public TimeSpan LexerTimeSpan { get; set; }

        public TimeSpan ParserTimeSpan { get; set; }
    }
}
