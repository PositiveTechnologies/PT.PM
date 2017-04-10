using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using Antlr4.Runtime.Tree;
using System.Linq;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.SqlParseTreeUst
{
    public partial class PlSqlConverterVisitor
    {
        /// <returns><see cref="Token"/></returns>
        public override UstNode VisitTerminal(ITerminalNode node)
        {
            string text = node.GetText();
            TextSpan textSpan = node.GetTextSpan();
            Token result;
            double doubleResult;
            if (text.StartsWith("'"))
            {
                result = new StringLiteral(text.Substring(1, text.Length - 2), textSpan, FileNode);
            }
            else if (text.ToLowerInvariant().StartsWith("n'"))
            {
                result = new StringLiteral(text.Substring(2, text.Length - 3), textSpan, FileNode);
            }
            else if (text.All(c => char.IsDigit(c)))
            {
                result = new IntLiteral(long.Parse(text), textSpan, FileNode);
            }
            else if (double.TryParse(text, out doubleResult))
            {
                result = new FloatLiteral(doubleResult, textSpan, FileNode);
            }
            else if (text.All(c => char.IsLetterOrDigit(c) || c == '_'))
            {
                result = new IdToken(text, textSpan, FileNode);
            }
            else
            {
                if (text.Any(c => char.IsLetterOrDigit(c) || c == '_'))
                {
                    Logger.LogDebug($"{text} converter to IdToken");
                    result = new IdToken(text, textSpan, FileNode);
                }
                else
                {
                    result = null;
                }
            }
            return result;
        }
    }
}
