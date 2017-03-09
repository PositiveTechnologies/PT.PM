using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.PhpAstConversion;

namespace PT.PM.AstParsing
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
