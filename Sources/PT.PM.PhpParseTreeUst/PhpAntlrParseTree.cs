using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.PhpParseTreeUst;

namespace PT.PM.UstParsing
{
    public class PhpAntlrParseTree : AntlrParseTree
    {
        public override Language SourceLanguage => Language.Php;

        public PhpAntlrParseTree()
        {
        }

        public PhpAntlrParseTree(PHPParser.HtmlDocumentContext syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
