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

            // PHP
            {".", BinaryOperator.Concat },
            {"**", BinaryOperator.Power},
            {"<>", BinaryOperator.NotEqual },
            {"and", BinaryOperator.LogicalAnd },
            {"xor", BinaryOperator.BitwiseXor },
            {"or", BinaryOperator.LogicalOr },

            // C#
            {"??", BinaryOperator.NullCoalescing},

            // Java
            {">>>", BinaryOperator.LogicalShift },

            // PHP & JavaScript
            {"===", BinaryOperator.StrictEqual },
            {"!==", BinaryOperator.StrictNotEqual }
        };

        public override string TextValue => BinaryOperator.ToString();

        public BinaryOperator BinaryOperator { get; set; }

        public BinaryOperatorLiteral(BinaryOperator op)
            : this(op, default(TextSpan))
        {
        }

        public BinaryOperatorLiteral(string op, TextSpan textSpan)
        {
            BinaryOperator binaryOperator;
            if (!TextBinaryOperator.TryGetValue(op, out binaryOperator))
            {
                binaryOperator = BinaryOperator.Equal;
            }
            BinaryOperator = binaryOperator;
            TextSpan = textSpan;
        }

        public BinaryOperatorLiteral(BinaryOperator op, TextSpan textSpan)
            : base(textSpan)
        {
            BinaryOperator = op;
        }

        public BinaryOperatorLiteral()
        {
        }

        public override int CompareTo(Ust other)
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
