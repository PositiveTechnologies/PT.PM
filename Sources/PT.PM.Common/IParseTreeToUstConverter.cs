using PT.PM.Common.Nodes;
using System.Collections.Generic;

namespace PT.PM.Common
{
    public interface IParseTreeToUstConverter : ILoggable
    {
        LanguageInfo Language { get; }

        HashSet<LanguageInfo> AnalyzedLanguages { get; set; }

        RootUst ParentRoot { get; set; }

        RootUst Convert(ParseTree langParseTree);
    }
}
