namespace PT.PM.Common.Nodes.Tokens
{
    public class IntLiteral : Token
    {
        public override NodeType NodeType => NodeType.IntLiteral;

        public long Value { get; set; }

        public override string TextValue { get { return Value.ToString(); } }

        public IntLiteral(long value)
            : this(value, default(TextSpan), null)
        {
        }

        public IntLiteral(long value, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Value = value;
        }

        public IntLiteral()
        {
        }

        public override int CompareTo(UstNode other)
        {
            var baseCompareResult = base.CompareTo(other);
            if (baseCompareResult != 0)
            {
                return baseCompareResult;
            }

            return Value.CompareTo(((IntLiteral)other).Value);
        }
    }
}
