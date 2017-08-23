using System;
using PT.PM.Common.Ust;

namespace PT.PM.Common
{
    public interface IParseTreeToUstConverter : ILoggable
    {
        UstType UstType { get; set; }

        Language MainLanguage { get; }

        LanguageFlags ConvertedLanguages { get; set; }

        Ust.Ust Convert(ParseTree langParseTree);
    }
}
