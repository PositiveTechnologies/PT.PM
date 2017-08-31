using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common
{
    public static class ConverterHelper
    {
        public static AssignmentExpression ConvertToAssignmentExpression(
             Expression left, BinaryOperator op, TextSpan opSpan, Expression right, TextSpan textSpan)
        {
            var opLiteral = new BinaryOperatorLiteral(op, opSpan);
            var expression = new BinaryOperatorExpression(left, opLiteral, right, textSpan);
            var result = new AssignmentExpression(left, expression, textSpan);
            return result;
        }
    }
}
