using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.TSqlParseTreeUst;

namespace PT.PM.SqlParseTreeUst
{
    public partial class TSqlAntlrConverter
    {
        public override Ust VisitTerminal(ITerminalNode node)
        {
            return ExtractLiteral(node.Symbol);
        }

        private InvocationExpression CreateSpecialInvocation(ITerminalNode name,
                ParserRuleContext context, Expression expression)
        {
            return new InvocationExpression(
                new IdToken(name.Symbol.Text.ToLowerInvariant(), name.GetTextSpan()),
                new ArgsUst(expression), context.GetTextSpan());
        }

        private InvocationExpression CreateSpecialInvocation(ITerminalNode name,
                    ParserRuleContext context, List<Expression> expressions)
        {
            return new InvocationExpression(
                new IdToken(name.Symbol.Text.ToLowerInvariant(), name.GetTextSpan()),
                new ArgsUst(expressions), context.GetTextSpan());
        }

        private Statement[] GetStatements(TSqlParser.Sql_clausesContext context)
        {
            Statement[] result;
            if (context != null)
            {
                result = context.sql_clause().Select(clause => (Statement)Visit(clause)).ToArray();
            }
            else
            {
                result = ArrayUtils<Statement>.EmptyArray;
            }
            return result;
        }

        private ArgsUst GetArgsNode(TSqlParser.Expression_listContext context)
        {
            ArgsUst result;
            if (context != null)
            {
                result = new ArgsUst(context.expression().Select(expr => (Expression)Visit(expr)).ToArray());
            }
            else
            {
                result = new ArgsUst();
            }
            return result;
        }

        private static string RemoveSpaces(string str)
        {
            var result = new StringBuilder(str.Length);
            foreach (char c in str)
            {
                if (!char.IsWhiteSpace(c))
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        private Token ExtractLiteral(IToken token)
        {
            string text = token.Text;
            TextSpan textSpan = token.GetTextSpan();
            Token result = null;

            try
            {
                if (text.StartsWith("@"))
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
            }

            if (result == null && (text.Any(c => char.IsLetterOrDigit(c) || c == '_')))
            {
                result = new IdToken(text, textSpan);
            }

            return result;
        }
    }
}
