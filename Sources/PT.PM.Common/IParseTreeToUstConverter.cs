using PT.PM.Common.Nodes;
using System.Collections.Generic;

namespace PT.PM.Common
{
    public interface IParseTreeToUstConverter : ILoggable
    {
        Language Language { get; }

        HashSet<Language> AnalyzedLanguages { get; set; }

        RootNode ParentRoot { get; set; }

        RootNode Convert(ParseTree langParseTree);
    }
}
