using System.Linq;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public class BinaryOperatorLiteral : Literal
    {
        public static Dictionary<string, BinaryOperator> TextBinaryOperator = new Dictionary<string, BinaryOperator>
        {
            {"+", BinaryOperator.Plus},
            {"-", BinaryOperator.Minus},
            {"*", BinaryOperator.Multiply},
            {"/", BinaryOperator.Divide},
            {"%", BinaryOperator.Mod},
            {"&", BinaryOperator.BitwiseAnd},
            {"|", BinaryOperator.BitwiseOr},
            {"&&", BinaryOperator.LogicalAnd},
            {"||", BinaryOperator.LogicalOr},
            {"^", BinaryOperator.BitwiseXor},
            {"<<", BinaryOperator.ShiftLeft},
            {">>", BinaryOperator.ShiftRight},
            
            {"==", BinaryOperator.Equal},
            {"!=", BinaryOperator.NotEqual},
            {">", BinaryOperator.Greater},
            {"<", BinaryOperator.Less},
            {">=", BinaryOperator.GreaterOrEqual},
            {"<=", BinaryOperator.LessOrEqual},

            {"??", BinaryOperator.NullCoalescing},
            {".", BinaryOperator.Plus }
        };

        public static List<string> TextBinaryAssignmentOperator = new List<string>
        {
            "+=",
            "-=",
            "*=",
            "/=",
            "%=",
            "&=",
            "|=",
            "^=",
            "<<=",
            ">>=",
        };

        public override NodeType NodeType => NodeType.BinaryOperatorLiteral;

        public override string TextValue => BinaryOperator.ToString();

        public BinaryOperator BinaryOperator { get; set; }

        public BinaryOperatorLiteral(BinaryOperator op)
            : this(op, default(TextSpan), null)
        {
        }

        public BinaryOperatorLiteral(string op, TextSpan textSpan, RootNode fileNode)
        {
            BinaryOperator binaryOperator;
            if (!TextBinaryOperator.TryGetValue(op, out binaryOperator))
            {
                binaryOperator = BinaryOperator.Equal;
            }
            BinaryOperator = binaryOperator;
            TextSpan = textSpan;
            Root = fileNode;
        }

        public BinaryOperatorLiteral(BinaryOperator op, TextSpan textSpan, RootNode fileNode)
            : base(textSpan, fileNode)
        {
            BinaryOperator = op;
        }

        public BinaryOperatorLiteral()
        {
        }

        public override int CompareTo(UstNode other)
        {
            var baseCompareResult = base.CompareTo(other);
            if (baseCompareResult != 0)
            {
                return baseCompareResult;
            }

            var binaryOperatorResult = BinaryOperator - ((BinaryOperatorLiteral)other).BinaryOperator;
            if (binaryOperatorResult != 0)
            {
                return binaryOperatorResult;
            }

            return 0;
        }

        public override string ToString()
        {
            if (TextBinaryOperator.ContainsValue(BinaryOperator))
            {
                return TextBinaryOperator.FirstOrDefault(pair => pair.Value == BinaryOperator).Key;
            }
            else
            {
                return BinaryOperator.ToString();
            }
        }
    }
}
