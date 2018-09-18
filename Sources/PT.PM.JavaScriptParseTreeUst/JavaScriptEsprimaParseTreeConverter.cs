using PT.PM.Common;
using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;

namespace PT.PM.JavaScriptParseTreeUst
{
    public class JavaScriptEsprimaParseTreeConverter : IParseTreeToUstConverter
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Language Language => JavaScript.Language;

        public bool IsActive => false;

        public HashSet<Language> AnalyzedLanguages { get; set; }

        public RootUst ParentRoot { get; set; }

        public RootUst Convert(ParseTree langParseTree)
        {
            var esprimParseTree = (JavaScriptEsprimaParseTree)langParseTree;

            throw new NotImplementedException();
        }
    }
}
