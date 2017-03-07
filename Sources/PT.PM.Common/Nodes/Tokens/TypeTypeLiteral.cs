namespace PT.PM.Common.Nodes.Tokens
{
    public class TypeTypeLiteral : Token
    {
        public override NodeType NodeType => NodeType.TypeTypeLiteral;

        public TypeType TypeType { get; set; }

        public override string TextValue
        {
            get { return TypeType.ToString(); }
        }

        public TypeTypeLiteral(TypeType typeType, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
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
