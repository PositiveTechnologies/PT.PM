using PT.PM.AntlrUtils;
using PT.PM.Common;

namespace PT.PM.JavaScriptParseTreeUst
{
    public class JavaScriptAntlrParseTree: AntlrParseTree
    {
        public override Language SourceLanguage => JavaScript.Language;

        public JavaScriptAntlrParseTree()
        {
        }

        public JavaScriptAntlrParseTree(JavaScriptParser.ProgramContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
