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
        public override Ust VisitTerminal(ITerminalNode node)
        {
            string text = node.GetText();
            TextSpan textSpan = node.GetTextSpan();
            Token result;
            if (text.StartsWith("'"))
            {
                result = TextUtils.GetStringLiteralWithoutQuotes(node.GetTextSpan(), root);
            }
            else if (text.ToLowerInvariant().StartsWith("n'"))
            {
                result = new StringLiteral(new TextSpan(textSpan.Start + 2, textSpan.Length - 3, textSpan.File), root);
            }
            else if (text.All(char.IsDigit))
            {
                result = TextUtils.TryCreateNumericLiteral(text, textSpan);
            }
            else if (double.TryParse(text, out var doubleResult))
            {
                result = new FloatLiteral(doubleResult, textSpan);
            }
            else if (text.All(c => char.IsLetterOrDigit(c) || c == '_') || text == "*")
            {
                result = new IdToken(text, textSpan);
            }
            else
            {
                if (text.Any(c => char.IsLetterOrDigit(c) || c == '_'))
                {
                    Logger.LogDebug($"{text} converter to IdToken");
                    result = new IdToken(text, textSpan);
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
