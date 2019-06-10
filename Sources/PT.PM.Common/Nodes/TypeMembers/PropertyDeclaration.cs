using PT.PM.Common.Nodes.Tokens;
using System.Collections.Generic;
using MessagePack;

namespace PT.PM.Common.Nodes.TypeMembers
{
    [MessagePackObject]
    public class PropertyDeclaration : EntityDeclaration
    {
        [Key(0)] public override UstType UstType => UstType.PropertyDeclaration;

        [Key(EntityFieldOffset)]
        public TypeToken Type { get; set; }

        [Key(EntityFieldOffset + 1)]
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
