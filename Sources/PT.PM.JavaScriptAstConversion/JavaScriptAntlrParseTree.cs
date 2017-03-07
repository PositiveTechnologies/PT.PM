using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.JavaScriptAstConversion.Parser;

namespace PT.PM.JavaScriptAstConversion
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
