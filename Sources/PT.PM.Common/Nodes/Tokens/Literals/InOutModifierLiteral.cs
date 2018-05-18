namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public class InOutModifierLiteral : Token
    {
        public InOutModifier ModifierType { get; set; }

        public override string TextValue => ModifierType.ToString();

        public InOutModifierLiteral(InOutModifier modifierType, TextSpan textSpan)
            : base(textSpan)
        {
            ModifierType = modifierType;
        }

        public InOutModifierLiteral()
        {
        }

        public override int CompareTo(Ust other)
        {
            var baseCompareResult = base.CompareTo(other);
            if (baseCompareResult != 0)
            {
                return baseCompareResult;
            }

            var modifierCompareResult = ModifierType - ((InOutModifierLiteral)other).ModifierType;
            return modifierCompareResult;
        }
    }
}
