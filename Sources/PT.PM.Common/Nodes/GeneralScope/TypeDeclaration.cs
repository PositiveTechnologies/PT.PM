using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace PT.PM.Common.Nodes.GeneralScope
{
    [MessagePackObject]
    public class TypeDeclaration : EntityDeclaration, IAttributable
    {
        [Key(EntityFieldOffset)]
        public TypeTypeLiteral Type { get; set; }

        [Key(EntityFieldOffset + 1)]
        public List<TypeToken> BaseTypes { get; set; } = new List<TypeToken>();

        [Key(EntityFieldOffset + 2)]
        public List<Ust> TypeMembers { get; set; } = new List<Ust>();

        [Key(EntityFieldOffset + 3)]
        public List<Attribute> Attributes { get; set; } = new List<Attribute>();

        public TypeDeclaration(TypeTypeLiteral type, IdToken name, IEnumerable<Ust> typeMembers,
             TextSpan textSpan) : base(name, textSpan)
        {
            Type = type;
            BaseTypes = new List<TypeToken>();
            TypeMembers = typeMembers as List<Ust> ?? typeMembers.ToList();
        }

        public TypeDeclaration(TypeTypeLiteral type, IdToken name, List<TypeToken> baseTypes,
            IEnumerable<Ust> typeMembers, TextSpan textSpan)
            : base(name, textSpan)
        {
            Type = type;
            BaseTypes = baseTypes;
            TypeMembers = typeMembers as List<Ust> ?? typeMembers.ToList();
        }

        public TypeDeclaration()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.AddRange(base.GetChildren());
            result.AddRange(Attributes);
            result.Add(Type);
            result.AddRange(BaseTypes);
            result.AddRange(TypeMembers);
            return result.ToArray();
        }

        public override string ToString()
        {
            string baseTypesString = BaseTypes.Count > 0
                ? " : " + string.Join(", ", BaseTypes) + " "
                : " ";
            return $"{string.Join(" ", Attributes)} {Name}{baseTypesString}{{{string.Join(" ", TypeMembers)}}}";
        }
    }
}
