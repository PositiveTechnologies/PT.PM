using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.TypeMembers
{
    public class ParameterDeclaration : Ust
    {
        public TypeToken Type { get; set; }

        public IdToken Name { get; set; }

        public Expression Initializer { get; set; }

        public ParameterDeclaration()
        {
        }

        public ParameterDeclaration(TypeToken type, IdToken name, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Type = type;
            Name = name;
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>{ Name };
            return result.ToArray();
        }

        public override string ToString() => Type != null ? $"{Type} {Name}" : Name.ToString();
    }
}
