using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.TypeMembers;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.PhpParseTreeUst
{
    public partial class PhpAntlrParseTreeConverterVisitor
    {
        private Expression CreateSpecialInvocation(ITerminalNode specialMethodTerminal, PHPParser.ExpressionContext expression, TextSpan contextTextSpan)
        {
            var expression0 = (Expression)VisitExpression(expression);
            var result = new InvocationExpression(
                new IdToken(specialMethodTerminal.GetText(), specialMethodTerminal.GetTextSpan(), FileNode),
                new ArgsNode(new List<Expression>() { expression0 }, expression.GetTextSpan(), FileNode),
                contextTextSpan, FileNode);
            return result;
        }

        private IdToken ConvertVar(ITerminalNode terminalNode)
        {
            var text = terminalNode.GetText().Substring(1);
            return new IdToken(text, terminalNode.GetTextSpan(), FileNode);
        }

        private ParameterDeclaration[] ConvertParameters(PHPParser.FormalParameterListContext parameters)
        {
            ParameterDeclaration[] result = parameters.formalParameter()
                .Select(p => (ParameterDeclaration)Visit(p)).ToArray();
            return result;
        }

        private List<Expression> ConvertSquareCurlyExpressions(PHPParser.SquareCurlyExpressionContext[] exprs)
        {
            List<Expression> expressions = exprs
                .Select(e => Visit(e))
                .Cast<Expression>()
                .ToList();
            return expressions;
        }

        protected override BinaryOperator CreateBinaryOperator(string binaryOperatorText)
        {
            string binaryOperatorTextNormalized = binaryOperatorText.ToLowerInvariant();
            switch (binaryOperatorTextNormalized)
            {
                case "**":
                    return BinaryOperator.Multiply;
                case "===":
                    return BinaryOperator.Equal;
                case "!==":
                case "<>":
                    return BinaryOperator.NotEqual;
                case ".":
                    return BinaryOperator.Plus;
                case "and":
                    return BinaryOperator.LogicalAnd;
                case "xor":
                    return BinaryOperator.BitwiseXor;
                case "or":
                    return BinaryOperator.LogicalOr;
                default:
                    return base.CreateBinaryOperator(binaryOperatorText);
            }
        }
    }
}
