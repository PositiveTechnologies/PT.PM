namespace PT.PM.Common.Nodes.Tokens
{
    public class ParameterModifierLiteral : Token
    {
        public override NodeType NodeType => NodeType.ParameterModifierLiteral;

        public ParameterModifier Modifier { get; set; }

        public override string TextValue
        {
            get { return Modifier.ToString(); }
        }

        public ParameterModifierLiteral(ParameterModifier modifier, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Modifier = modifier;
        }

        public ParameterModifierLiteral()
        {
        }

        public override int CompareTo(UstNode other)
        {
            var baseCompareResult = base.CompareTo(other);
            if (baseCompareResult != 0)
            {
                return baseCompareResult;
            }

            var modifierCompareResult = Modifier - ((ParameterModifierLiteral)other).Modifier;
            return modifierCompareResult;
        }
    }
}
