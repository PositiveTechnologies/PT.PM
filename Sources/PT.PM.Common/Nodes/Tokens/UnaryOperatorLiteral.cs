using System.Linq;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Tokens
{
    public class UnaryOperatorLiteral : Token
    {
        public static Dictionary<string, UnaryOperator> PrefixTextUnaryOperator = new Dictionary<string, UnaryOperator>
        {
            { "+", UnaryOperator.Plus},
            { "-", UnaryOperator.Minus},
            { "!", UnaryOperator.Not},
            { "~", UnaryOperator.BitwiseNot},
            { "++", UnaryOperator.Increment},
            { "--", UnaryOperator.Decrement},
            { "*", UnaryOperator.Dereference},
            { "&", UnaryOperator.AddressOf},
            { "await", UnaryOperator.Await}
        };

        public static Dictionary<string, UnaryOperator> PostfixTextUnaryOperator = new Dictionary<string, UnaryOperator>
        {
            { "++", UnaryOperator.PostIncrement},
            { "--", UnaryOperator.PostDecrement},
        };

        public override NodeType NodeType => NodeType.UnaryOperatorLiteral;

        public override string TextValue
        {
            get { return UnaryOperator.ToString(); }
        }

        public UnaryOperator UnaryOperator { get; set; }

        public UnaryOperatorLiteral(UnaryOperator op)
            : this(op, default(TextSpan), null)
        {
        }

        public UnaryOperatorLiteral(bool prefix, string op, TextSpan textSpan, FileNode fileNode)
            : this(prefix ? PrefixTextUnaryOperator[op] : PostfixTextUnaryOperator[op], textSpan, fileNode)
        {
        }

        public UnaryOperatorLiteral(UnaryOperator op, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            UnaryOperator = op;
        }

        public UnaryOperatorLiteral()
        {
        }

        public override int CompareTo(UstNode other)
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
