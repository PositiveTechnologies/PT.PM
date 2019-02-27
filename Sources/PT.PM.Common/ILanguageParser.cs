using System;

namespace PT.PM.Common
{
    public interface ILanguageParser<T> : ILanguageParserBase
    {
        ParseTree Parse(T parseUnit, out TimeSpan parserTimeSpan);
    }
}
