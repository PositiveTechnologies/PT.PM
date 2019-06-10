using System;
using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    public class ModifierLiteral : Literal
    {
        [Key(0)] public override UstType UstType => UstType.ModifierLiteral;

        [Key(UstFieldOffset)]
        public Modifier Modifier { get; set; }

        [Key(UstFieldOffset + 1)]
        public string ModifierName { get; private set; }

        [IgnoreMember]
        public override string TextValue => ModifierName;

        public ModifierLiteral(Modifier modifier, TextSpan textSpan)
            : base(textSpan)
        {
            InitModifierAndName(modifier);
        }

        public ModifierLiteral(string modifierName, TextSpan textSpan)
             : base(textSpan)
        {
            Modifier modifier;
            if (Enum.TryParse(modifierName, true, out modifier))
            {
                Modifier = modifier;
                ModifierName = Modifier.ToString().ToLowerInvariant();
            }
            else
            {
                Modifier = Modifier.Other;
                ModifierName = modifierName;
            }
        }

        public ModifierLiteral(Modifier modifier)
        {
            InitModifierAndName(modifier);
        }

        public ModifierLiteral()
        {
            Modifier = Modifier.None;
            ModifierName = "";
        }

        public override int CompareTo(Ust other)
        {
            var baseCompareResult = base.CompareTo(other);
            if (baseCompareResult != 0)
            {
                return baseCompareResult;
            }

            var modifierCompareResult = Modifier - ((ModifierLiteral)other).Modifier;
            return modifierCompareResult;
        }

        private void InitModifierAndName(Modifier modifier)
        {
            Modifier = modifier;
            if (Modifier == Modifier.Other)
            {
                throw new ArgumentException("modifier should not equal to the Other value");
            }
            ModifierName = Modifier.ToString().ToLowerInvariant();
        }
    }
}
