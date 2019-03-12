using PT.PM.AntlrUtils;
using PT.PM.Common;

namespace PT.PM.Python3ParseTreeUst
{
    public class Python3AntlrParseTree : AntlrParseTree
    {
        public override Language SourceLanguage => Language.Python3;
        
        public Python3AntlrParseTree()
        {
        }

        public Python3AntlrParseTree(Python3Parser.RootContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}