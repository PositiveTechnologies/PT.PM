namespace PT.PM.Common
{
    public abstract class ParseTree
    {
        public abstract Language SourceLanguage { get; }

        public SourceCodeFile SourceCodeFile { get; set; }
    }
}
