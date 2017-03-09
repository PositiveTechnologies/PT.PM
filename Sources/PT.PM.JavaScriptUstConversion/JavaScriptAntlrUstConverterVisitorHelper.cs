using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.JavaScriptAstConversion
{
    public partial class JavaScriptAntlrAstConverterVisitor
    {
        protected override BinaryOperator CreateBinaryOperator(string binaryOperatorText)
        {
            switch (binaryOperatorText)
            {
                case "===":
                    return BinaryOperator.Equal;
                case "!==":
                    return BinaryOperator.NotEqual;
                case ">>>":
                    return BinaryOperator.ShiftRight;
                default:
                    return base.CreateBinaryOperator(binaryOperatorText);
            }
        }
    }
}
