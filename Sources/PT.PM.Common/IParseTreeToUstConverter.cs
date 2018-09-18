using PT.PM.Common.Nodes;
using System.Collections.Generic;

namespace PT.PM.Common
{
    public interface IParseTreeToUstConverter : ILoggable
    {
        Language Language { get; }

        HashSet<Language> AnalyzedLanguages { get; set; }

        RootUst ParentRoot { get; set; }

        bool IsActive { get; }

        RootUst Convert(ParseTree langParseTree);
    }
}
