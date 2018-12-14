using Antlr4.Runtime.Tree;
using PT.PM.Common.Nodes;

namespace PT.PM.SqlParseTreeUst
{
    public partial class MySqlAntlrConverter
    {
        public override Ust VisitTerminal(ITerminalNode node)
        {
            return ExtractLiteral(node.Symbol);
        }
    }
}
