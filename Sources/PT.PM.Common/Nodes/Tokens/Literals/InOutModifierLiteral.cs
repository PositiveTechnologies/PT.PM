using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    public class InOutModifierLiteral : Token
    {
        [Key(0)] public override UstType UstType => UstType.InOutModifierLiteral;

        [Key(UstFieldOffset)]
        public InOutModifier ModifierType { get; set; }

        [IgnoreMember]
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
