using PT.PM.Common;
using System.IO;

namespace PT.PM.CSharpParseTreeUst
{
    public class RoslynDumper : ParseTreeDumper
    {
        public override void DumpTokens(ParseTree parseTree)
        {
            Dump("", parseTree.SourceCodeFile, true);
        }

        public override void DumpTree(ParseTree parseTree)
        {
            Dump("", parseTree.SourceCodeFile, false);
        }
    }
}
