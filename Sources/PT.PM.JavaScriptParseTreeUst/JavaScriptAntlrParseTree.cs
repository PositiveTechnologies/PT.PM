using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.JavaScriptParseTreeUst.Parser;

namespace PT.PM.JavaScriptParseTreeUst
{
    public class JavaScriptAntlrParseTree: AntlrParseTree
    {
        public override LanguageInfo SourceLanguage => JavaScript.Language;

        public JavaScriptAntlrParseTree()
        {
        }

        public JavaScriptAntlrParseTree(JavaScriptParser.ProgramContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
