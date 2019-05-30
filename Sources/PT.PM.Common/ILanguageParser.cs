using System;

namespace PT.PM.Common
{
    public interface ILanguageParser<in T> : ILanguageParserBase
    {
        ParseTree Parse(T parseUnit, out TimeSpan timeSpan);
    }
}
