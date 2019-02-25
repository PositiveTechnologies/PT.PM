namespace PT.PM.Common
{
    public interface ILanguageParser<T> : ILanguageParserBase
    {
        ParseTree Parse(T parseUnit);
    }
}
