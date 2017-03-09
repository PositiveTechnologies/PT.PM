using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.TSqlUstConversion.Parser;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PT.PM.SqlUstConversion
{
    public partial class TSqlConverterVisitor
    {
        public override UstNode VisitTerminal(ITerminalNode node)
        {
            return ExtractLiteral(node.Symbol);
        }

        private InvocationExpression CreateSpecialInvocation(ITerminalNode name,
                ParserRuleContext context, Expression expression)
        {
            return new InvocationExpression(
                new IdToken(name.Symbol.Text.ToLowerInvariant(), name.GetTextSpan(), FileNode),
                new ArgsNode(expression), context.GetTextSpan(), FileNode);
        }

        private InvocationExpression CreateSpecialInvocation(ITerminalNode name,
                    ParserRuleContext context, IList<Expression> expressions)
        {
            return new InvocationExpression(
                new IdToken(name.Symbol.Text.ToLowerInvariant(), name.GetTextSpan(), FileNode),
                new ArgsNode(expressions), context.GetTextSpan(), FileNode);
        }

        private Statement[] GetStatements(tsqlParser.Sql_clausesContext context)
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

        private ArgsNode GetArgsNode(tsqlParser.Expression_listContext context)
        {
            ArgsNode result;
            if (context != null)
            {
                result = new ArgsNode(context.expression().Select(expr => (Expression)Visit(expr)).ToArray());
            }
            else
            {
                result = new ArgsNode();
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
            Token result;
            double floatValue;
            if (text.StartsWith("@"))
            {
                result = new IdToken(text.Substring(1), textSpan, FileNode);
            }
            else if (text.StartsWith("\"") || text.StartsWith("["))
            {
                result = new IdToken(text.Substring(1, text.Length - 2), textSpan, FileNode);
            }
            else if (text.EndsWith("'"))
            {
                if (text.StartsWith("N"))
                {
                    text = text.Substring(1);
                }
                text = text.Substring(1, text.Length - 2);
                result = new StringLiteral(text, textSpan, FileNode);
            }
            else if (text.All(c => char.IsDigit(c)))
            {
                result = new IntLiteral(long.Parse(text), textSpan, FileNode);
            }
            else if (text.StartsWith("0X") || text.StartsWith("0x"))
            {
                result = new IntLiteral(Convert.ToInt64(text.Substring(2), 16), textSpan, FileNode);
            }
            else if (double.TryParse(text, out floatValue))
            {
                result = new FloatLiteral(floatValue, textSpan, FileNode);
            }
            else
            {
                if (text.Any(c => char.IsLetterOrDigit(c) || c == '_'))
                {
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
