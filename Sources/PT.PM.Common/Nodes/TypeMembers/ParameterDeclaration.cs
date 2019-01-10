using MessagePack;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.TypeMembers
{
    [MessagePackObject]
    public class ParameterDeclaration : Ust
    {
        [Key(UstFieldOffset)]
        public InOutModifierLiteral Modifier { get; set; }

        [Key(UstFieldOffset + 1)]
        public TypeToken Type { get; set; }

        [Key(UstFieldOffset + 2)]
        public IdToken Name { get; set; }

        [Key(UstFieldOffset + 3)]
        public Expression Initializer { get; set; }

        public ParameterDeclaration(InOutModifierLiteral modifier, TypeToken type,
            IdToken name, TextSpan textSpan = default)
            : base(textSpan)
        {
            Modifier = modifier;
            Type = type;
            Name = name;
        }

        public ParameterDeclaration()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] { Modifier, Type, Name, Initializer };
        }

        public override string ToString()
        {
            string modifierString = Modifier != null ? Modifier + " " : "";
            string typeString = Type != null ? Type + " " : "";
            string initializerString = Initializer != null ? ": " + Initializer : "";

            return $"{modifierString}{typeString}{Name}{initializerString}";
        }
    }
}
