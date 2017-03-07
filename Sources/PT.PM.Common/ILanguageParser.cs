namespace PT.PM.Common
{
    public interface ILanguageParser : ILoggable
    {
        Language Language { get; }

        ParseTree Parse(SourceCodeFile sourceCodeFile);

        void ClearCache();
    }
}
