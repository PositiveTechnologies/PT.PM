using PT.PM.AntlrUtils;
using PT.PM.Common;

namespace PT.PM.JavaParseTreeUst
{
    public class JavaAntlrParseTree : AntlrParseTree
    {
        public override Language SourceLanguage => Language.Java;

        public JavaAntlrParseTree()
        {
        }

        public JavaAntlrParseTree(JavaParser.CompilationUnitContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
