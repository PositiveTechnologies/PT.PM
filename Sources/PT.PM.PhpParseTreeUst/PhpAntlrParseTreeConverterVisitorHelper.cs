using Antlr4.Runtime.Tree;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.PhpParseTreeUst
{
    public partial class PhpAntlrParseTreeConverter
    {
        private Expression CreateSpecialInvocation(ITerminalNode specialMethodTerminal, PhpParser.ExpressionContext expression, TextSpan contextTextSpan)
        {
            var expression0 = (Expression)VisitExpression(expression);
            var result = new InvocationExpression(
                new IdToken(specialMethodTerminal.GetText(), specialMethodTerminal.GetTextSpan()),
                new ArgsUst(new List<Expression>() { expression0 }, expression.GetTextSpan()),
                contextTextSpan);
            return result;
        }

        private IdToken ConvertVar(ITerminalNode terminalNode)
        {
            var text = terminalNode.GetText().Substring(1);
            return new IdToken(text, terminalNode.GetTextSpan());
        }

        private ParameterDeclaration[] ConvertParameters(PhpParser.FormalParameterListContext parameters)
        {
            ParameterDeclaration[] result = parameters.formalParameter()
                .Select(p => (ParameterDeclaration)Visit(p)).ToArray();
            return result;
        }

        private List<Expression> ConvertSquareCurlyExpressions(PhpParser.SquareCurlyExpressionContext[] exprs)
        {
            List<Expression> expressions = exprs
                .Select(e => Visit(e))
                .Cast<Expression>()
                .ToList();
            return expressions;
        }
    }
}
