using System.Collections.Generic;
using MessagePack;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.TypeMembers
{
    [MessagePackObject]
    public abstract class EntityDeclaration : Ust
    {
        internal const int EntityFieldOffset = UstFieldOffset + 2;

        [Key(UstFieldOffset)]
        public List<ModifierLiteral> Modifiers { get; set; }

        [Key(UstFieldOffset + 1)]
        public IdToken Name { get; set; }

        protected EntityDeclaration(IdToken name, TextSpan textSpan)
            : base(textSpan)
        {
            Name = name;
        }

        protected EntityDeclaration()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            if (Modifiers != null)
                result.AddRange(Modifiers);
            result.Add(Name);
            return result.ToArray();
        }
    }
}
