namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public class ParameterModifierLiteral : Token
    {
        public override NodeType NodeType => NodeType.ParameterModifierLiteral;

        public ParameterModifier Modifier { get; set; }

        public override string TextValue => Modifier.ToString();

        public ParameterModifierLiteral(ParameterModifier modifier, TextSpan textSpan, RootNode fileNode)
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
