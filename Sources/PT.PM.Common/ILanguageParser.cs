namespace PT.PM.Common
{
    public interface ILanguageParser<T> : IBaseLanguageParser
    {
        ParseTree Parse(T parseUnit);
    }
}
