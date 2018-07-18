using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Text;

namespace PT.PM.Common.Nodes.TypeMembers
{
    public class ParameterDeclaration : Ust
    {
        public InOutModifierLiteral Modifier { get; set; }

        public TypeToken Type { get; set; }

        public IdToken Name { get; set; }

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
            string initializerString = Initializer != null ? " " + Initializer : "";

            return $"{modifierString}{typeString}{Name}{initializerString}";
        }
    }
}
