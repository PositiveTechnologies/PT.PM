using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.TypeMembers
{
    public abstract class EntityDeclaration : UstNode
    {
        public List<ModifierLiteral> Modifiers { get; set; }

        public IdToken Name { get; set; }

        protected EntityDeclaration(IdToken name, TextSpan textSpan, RootNode fileNode)
            : base(textSpan, fileNode)
        {
            Name = name;
        }

        protected EntityDeclaration()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
            if (Modifiers != null)
                result.AddRange(Modifiers);
            result.Add(Name);
            return result.ToArray();
        }
    }
}
