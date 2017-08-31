namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public class ModifierLiteral : Literal
    {
        public override NodeType NodeType => NodeType.ModifierLiteral;

        public Modifier Modifier { get; set; }

        public override string TextValue => Modifier.ToString();

        public ModifierLiteral(Modifier modifier, TextSpan textSpan)
            : base(textSpan)
        {
            Modifier = modifier;
        }

        public ModifierLiteral()
        {
        }

        public override int CompareTo(UstNode other)
        {
            var baseCompareResult = base.CompareTo(other);
            if (baseCompareResult != 0)
            {
                return baseCompareResult;
            }

            var modifierCompareResult = Modifier - ((ModifierLiteral)other).Modifier;
            return modifierCompareResult;
        }
    }
}
