using Antlr4.Runtime.Tree;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Linq;

namespace PT.PM.SqlParseTreeUst
{
    public partial class PlSqlAntlrConverter
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
                result = new StringLiteral(text.Substring(1, text.Length - 2), textSpan, root);
            }
            else if (text.ToLowerInvariant().StartsWith("n'"))
            {
                result = new StringLiteral(text.Substring(2, text.Length - 3), textSpan, root);
            }
            else if (text.All(c => char.IsDigit(c)))
            {
                result = new IntLiteral(long.Parse(text), textSpan, root);
            }
            else if (double.TryParse(text, out doubleResult))
            {
                result = new FloatLiteral(doubleResult, textSpan, root);
            }
            else if (text.All(c => char.IsLetterOrDigit(c) || c == '_'))
            {
                result = new IdToken(text, textSpan, root);
            }
            else
            {
                if (text.Any(c => char.IsLetterOrDigit(c) || c == '_'))
                {
                    Logger.LogDebug($"{text} converter to IdToken");
                    result = new IdToken(text, textSpan, root);
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
