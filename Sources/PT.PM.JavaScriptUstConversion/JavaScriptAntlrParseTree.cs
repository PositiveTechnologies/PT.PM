using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.JavaScriptUstConversion.Parser;

namespace PT.PM.JavaScriptUstConversion
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
