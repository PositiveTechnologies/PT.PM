namespace PT.PM.Common.Nodes.Tokens
{
    public class BooleanLiteral : Token
    {
        public override NodeType NodeType => NodeType.BooleanLiteral;

        public bool Value { get; set; }

        public BooleanLiteral(bool value)
            : this(value, default(TextSpan), null)
        {
        }

        public BooleanLiteral(bool value, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Value = value;
        }

        public BooleanLiteral()
        {
        }

        public override string TextValue
        {
            get { return Value.ToString(); }
        }

        public override int CompareTo(UstNode other)
        {
            var baseCompareResult = base.CompareTo(other);
            if (baseCompareResult != 0)
            {
                return baseCompareResult;
            }

            var result = Value ^ ((BooleanLiteral)other).Value;
            if (result != false)
            {
                return Value ? 1 : -1;
            }

            return 0;
        }
    }
}
