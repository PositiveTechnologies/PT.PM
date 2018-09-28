using PT.PM.Common.Nodes.Tokens;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.TypeMembers
{
    public class PropertyDeclaration : EntityDeclaration
    {
        public TypeToken Type { get; set; }

        public Ust Body { get; set; }

        public PropertyDeclaration(TypeToken type, IdToken name, Ust body, TextSpan textSpan)
            : base(name, textSpan)
        {
            Type = type;
            Body = body;
        }

        public PropertyDeclaration()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.AddRange(base.GetChildren());
            result.Add(Type);
            result.Add(Body);
            return result.ToArray();
        }

        public override string ToString()
        {
            string modifierString = Modifiers != null ? string.Join(" ", Modifiers) + " " : "";
            string typeString = Type != null ? Type + " " : "";
            string bodyString = Body != null ? ": " + Body : "";

            return $"{modifierString}{typeString}{Name}{bodyString}";
        }
    }
}
