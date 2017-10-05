using AspxParser;
using PT.PM.Common;

namespace PT.PM.CSharpParseTreeUst
{
    public class AspxParseTree: ParseTree
    {
        public override LanguageInfo SourceLanguage => Aspx.Language;

        public AspxNode Root { get; set; }

        public AspxParseTree()
        {
        }

        public AspxParseTree(AspxNode root)
        {
            Root = root;
        }
    }
}
