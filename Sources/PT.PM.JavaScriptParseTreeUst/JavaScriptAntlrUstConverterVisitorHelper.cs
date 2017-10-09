using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.JavaScriptParseTreeUst
{
    public partial class JavaScriptParseTreeConverter
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
