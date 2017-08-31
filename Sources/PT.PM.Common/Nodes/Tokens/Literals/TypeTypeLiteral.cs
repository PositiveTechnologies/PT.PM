namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public class TypeTypeLiteral : Token
    {
        public override NodeType NodeType => NodeType.TypeTypeLiteral;

        public TypeType TypeType { get; set; }

        public override string TextValue => TypeType.ToString();

        public TypeTypeLiteral(TypeType typeType, TextSpan textSpan)
            : base(textSpan)
        {
            TypeType = typeType;
        }

        public TypeTypeLiteral()
        {
        }

        public override int CompareTo(UstNode other)
        {
            var baseCompareResult = base.CompareTo(other);
            if (baseCompareResult != 0)
            {
                return baseCompareResult;
            }

            var classTypeCompareResult = TypeType - ((TypeTypeLiteral) other).TypeType;
            return classTypeCompareResult;
        }
    }
}
