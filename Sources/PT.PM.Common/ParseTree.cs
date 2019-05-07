using PT.PM.Common.Files;

namespace PT.PM.Common
{
    public abstract class ParseTree
    {
        public abstract Language SourceLanguage { get; }

        public TextFile SourceFile { get; set; }
    }
}
