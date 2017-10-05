using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.JavaParseTreeUst.Parser;

namespace PT.PM.JavaParseTreeUst
{
    public class JavaAntlrParseTree : AntlrParseTree
    {
        public override LanguageInfo SourceLanguage => Java.Language;

        public JavaAntlrParseTree()
        {
        }

        public JavaAntlrParseTree(JavaParser.CompilationUnitContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
