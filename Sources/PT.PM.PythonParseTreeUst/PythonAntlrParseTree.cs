using PT.PM.AntlrUtils;
using PT.PM.Common;
using PythonParseTree;

namespace PT.PM.PythonParseTreeUst
{
    public class PythonAntlrParseTree : AntlrParseTree
    {
        public override Language SourceLanguage => Language.Python;
        
        public PythonAntlrParseTree()
        {
        }

        public PythonAntlrParseTree(PythonParser.RootContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}