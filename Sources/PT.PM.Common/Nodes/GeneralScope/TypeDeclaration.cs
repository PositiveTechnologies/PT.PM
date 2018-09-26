using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.GeneralScope
{
    public class TypeDeclaration : EntityDeclaration
    {
        public TypeTypeLiteral Type { get; set; }

        public List<TypeToken> BaseTypes { get; set; } = new List<TypeToken>();

        public List<EntityDeclaration> TypeMembers { get; set; } = new List<EntityDeclaration>();

        public TypeDeclaration(TypeTypeLiteral type, IdToken name, IEnumerable<EntityDeclaration> typeMembers,
             TextSpan textSpan) : base(name, textSpan)
        {
            Type = type;
            BaseTypes = new List<TypeToken>();
            TypeMembers = typeMembers as List<EntityDeclaration> ?? typeMembers.ToList();
        }

        public TypeDeclaration(TypeTypeLiteral type, IdToken name, List<TypeToken> baseTypes,
            IEnumerable<EntityDeclaration> typeMembers, TextSpan textSpan)
            : base(name, textSpan)
        {
            Type = type;
            BaseTypes = baseTypes;
            TypeMembers = typeMembers as List<EntityDeclaration> ?? typeMembers.ToList();
        }

        public TypeDeclaration()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.AddRange(base.GetChildren());
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
            return $"{Name}{baseTypesString}{{{string.Join(" ", TypeMembers)}}}";
        }
    }
}
