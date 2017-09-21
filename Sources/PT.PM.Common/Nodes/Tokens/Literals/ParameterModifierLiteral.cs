namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public class ParameterModifierLiteral : Token
    {
        public override UstKind Kind => UstKind.ParameterModifierLiteral;

        public ParameterModifier Modifier { get; set; }

        public override string TextValue => Modifier.ToString();

        public ParameterModifierLiteral(ParameterModifier modifier, TextSpan textSpan)
            : base(textSpan)
        {
            Modifier = modifier;
        }

        public ParameterModifierLiteral()
        {
        }

        public override int CompareTo(Ust other)
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
