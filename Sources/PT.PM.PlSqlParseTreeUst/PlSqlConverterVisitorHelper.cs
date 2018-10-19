using Antlr4.Runtime.Tree;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;
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
            double doubleResult;
            if (text.StartsWith("'"))
            {
                result = new StringLiteral(text.Substring(1, text.Length - 2), textSpan);
            }
            else if (text.ToLowerInvariant().StartsWith("n'"))
            {
                result = new StringLiteral(text.Substring(2, text.Length - 3), textSpan);
            }
            else if (text.All(c => char.IsDigit(c)))
            {
                result = new IntLiteral(long.Parse(text), textSpan);
            }
            else if (double.TryParse(text, out doubleResult))
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

        public List<Expression> ExtractMultiChild(MultichildExpression multiChild, List<Expression> expressions)
        {
            expressions = expressions ?? new List<Expression>();

            for(int i = 0; i < multiChild.Expressions.Count; i++)
            {
                var current = multiChild.Expressions[i];
                if(current is MultichildExpression nested)
                {
                   expressions = ExtractMultiChild(nested, expressions);
                }
                else
                {
                    expressions.Add(current);
                }
            }
            return expressions;
        }
    }
}
