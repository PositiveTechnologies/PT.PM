using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System;
using System.Linq;

namespace PT.PM.SqlParseTreeUst
{
    public partial class MySqlAntlrConverter
    {
        public override Ust VisitTerminal(ITerminalNode node)
        {
            return ExtractLiteral(node.Symbol);
        }

        private Ust ExtractLiteral(IToken token)
        {
            string text = token.Text;
            TextSpan textSpan = token.GetTextSpan();
            Token result = null;

            try
            {
                if (text == "*")
                {
                    result = new IdToken(text, textSpan);
                }
                else if (text.StartsWith("@"))
                {
                    result = new IdToken(text.Substring(1), textSpan);
                }
                else if (text.StartsWith("\"") || text.StartsWith("["))
                {
                    result = new IdToken(text.Substring(1, text.Length - 2), textSpan);
                }
                else if (text.EndsWith("'"))
                {
                    if (text.StartsWith("N"))
                    {
                        text = text.Substring(1);
                    }
                    text = text.Substring(1, text.Length - 2);
                    result = new StringLiteral(text, textSpan);
                }
                else if (text.All(c => char.IsDigit(c)))
                {
                    result = new IntLiteral(long.Parse(text), textSpan);
                }
                else if (text.StartsWith("0X", StringComparison.OrdinalIgnoreCase))
                {
                    result = new IntLiteral(System.Convert.ToInt64(text.Substring(2), 16), textSpan);
                }
                else if (double.TryParse(text, out double floatValue))
                {
                    result = new FloatLiteral(floatValue, textSpan);
                }
            }
            catch
            {
                Logger.LogDebug($"Literal cannot be extracted from {nameof(token)} with symbol {text}");
            }

            if (result == null && (text.Any(c => char.IsLetterOrDigit(c) || c == '_')))
            {
                result = new IdToken(text, textSpan);
            }

            return result;
        }
    }
}
