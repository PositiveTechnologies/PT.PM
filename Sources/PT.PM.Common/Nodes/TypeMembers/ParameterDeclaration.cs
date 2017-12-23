using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using System.Text;

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
            return new Ust[] { Type, Name, Initializer };
        }

        public override string ToString()
        {
            var result = new StringBuilder();

            if (Type != null)
            {
                result.Append(Type);
                result.Append(' ');
            }

            result.Append(Name);

            if (Initializer != null)
            {
                result.Append(' ');
                result.Append(Initializer);
            }

            return result.ToString();
        }
    }
}
