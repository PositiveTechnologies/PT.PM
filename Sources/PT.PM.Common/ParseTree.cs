namespace PT.PM.Common
{
    public abstract class ParseTree
    {
        public abstract LanguageInfo SourceLanguage { get; }

        public SourceCodeFile SourceCodeFile { get; set; }
    }
}
