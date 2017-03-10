using PT.PM.Common;
using AspxParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.CSharpParseTreeUst
{
    public class AspxParseTree: ParseTree
    {
        public override Language SourceLanguage => Language.Aspx;

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
