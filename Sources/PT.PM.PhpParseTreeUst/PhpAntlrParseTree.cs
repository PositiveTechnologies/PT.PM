using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.PhpParseTreeUst;

namespace PT.PM.PhpParseTreeUst
{
    public class PhpAntlrParseTree : AntlrParseTree
    {
        public override Language SourceLanguage => Language.Php;

        public PhpAntlrParseTree()
        {
        }

        public PhpAntlrParseTree(PhpParser.HtmlDocumentContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
