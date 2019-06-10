using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    public class UnaryOperatorLiteral : Literal
    {
        public static Dictionary<string, UnaryOperator> PrefixTextUnaryOperator = new Dictionary<string, UnaryOperator>
        {
            ["+"] = UnaryOperator.Plus,
            ["-"] = UnaryOperator.Minus,
            ["!"] = UnaryOperator.Not,
            ["~"] = UnaryOperator.BitwiseNot,
            ["++"] = UnaryOperator.Increment,
            ["--"] = UnaryOperator.Decrement,
            ["*"] = UnaryOperator.Dereference,
            ["&"] = UnaryOperator.AddressOf,
            ["delete"] = UnaryOperator.Delete,
            ["delete[]"] = UnaryOperator.DeleteArray,
            ["typeof"] = UnaryOperator.TypeOf,
            ["await"] = UnaryOperator.Await
        };

        public static Dictionary<string, UnaryOperator> PostfixTextUnaryOperator = new Dictionary<string, UnaryOperator>
        {
            ["++"] = UnaryOperator.PostIncrement,
            ["--"] = UnaryOperator.PostDecrement,
        };

        [Key(0)] public override UstType UstType => UstType.UnaryOperatorLiteral;

        [Key(UstFieldOffset)]
        public UnaryOperator UnaryOperator { get; set; }

        [IgnoreMember]
        public override string TextValue => UnaryOperator.ToString();

        public UnaryOperatorLiteral(UnaryOperator op)
            : this(op, default)
        {
        }

        public UnaryOperatorLiteral(bool prefix, string op, TextSpan textSpan)
            : this(prefix ? PrefixTextUnaryOperator[op] : PostfixTextUnaryOperator[op], textSpan)
        {
        }

        public UnaryOperatorLiteral(UnaryOperator op, TextSpan textSpan)
            : base(textSpan)
        {
            UnaryOperator = op;
        }

        public UnaryOperatorLiteral()
        {
        }

        public override int CompareTo(Ust other)
        {
            var baseCompareResult = base.CompareTo(other);
            if (baseCompareResult != 0)
            {
                return baseCompareResult;
            }

            var unaryOperatorResult = UnaryOperator - ((UnaryOperatorLiteral)other).UnaryOperator;
            return unaryOperatorResult;
        }

        public override string ToString()
        {
            if (PrefixTextUnaryOperator.ContainsValue(UnaryOperator))
            {
                return PrefixTextUnaryOperator.FirstOrDefault(pair => pair.Value == UnaryOperator).Key;
            }
            else if (PostfixTextUnaryOperator.ContainsValue(UnaryOperator))
            {
                return PostfixTextUnaryOperator.FirstOrDefault(pair => pair.Value == UnaryOperator).Key;
            }
            else
            {
                return UnaryOperator.ToString();
            }
        }
    }
}
