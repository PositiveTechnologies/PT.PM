namespace PT.PM.Common
{
    public interface ILanguageParser : ILoggable
    {
        Language Language { get; }

        ParseTree Parse(CodeFile sourceCodeFile);

        void ClearCache();
    }
}
