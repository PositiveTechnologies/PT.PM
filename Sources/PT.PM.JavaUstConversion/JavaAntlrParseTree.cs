using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.JavaUstConversion.Parser;
using System.Collections.Generic;

namespace PT.PM.JavaUstConversion
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
