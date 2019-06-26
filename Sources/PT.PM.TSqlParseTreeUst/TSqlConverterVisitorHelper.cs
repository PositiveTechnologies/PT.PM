using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.TSqlParseTreeUst;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PT.PM.SqlParseTreeUst
{
    public partial class TSqlAntlrConverter
    {
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
    }
}
