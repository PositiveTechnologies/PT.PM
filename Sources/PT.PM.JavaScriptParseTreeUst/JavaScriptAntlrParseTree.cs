using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.JavaScriptParseTreeUst.Parser;

namespace PT.PM.JavaScriptParseTreeUst
{
    public class JavaScriptAntlrParseTree: AntlrParseTree
    {
        public override Language SourceLanguage => Language.JavaScript;

        public JavaScriptAntlrParseTree()
        {
        }

        public JavaScriptAntlrParseTree(ECMAScriptParser.ProgramContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
