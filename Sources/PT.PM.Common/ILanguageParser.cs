namespace PT.PM.Common
{
    public interface ILanguageParser : ILoggable
    {
        LanguageInfo Language { get; }

        ParseTree Parse(SourceCodeFile sourceCodeFile);

        void ClearCache();
    }
}
