using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.TypeMembers
{
    public class ParameterDeclaration : UstNode
    {
        public override NodeType NodeType => NodeType.ParameterDeclaration;

        public TypeToken Type { get; set; }

        public IdToken Name { get; set; }

        public Expression Initializer { get; set; }

        public ParameterDeclaration(TypeToken type, IdToken name, TextSpan textSpan, RootNode fileNode)
            : base(textSpan, fileNode)
        {
            Type = type;
            Name = name;
        }

        public ParameterDeclaration()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>{ Name };
            return result.ToArray();
        }

        public override string ToString()
        {
            return Type != null ? $"{Type} {Name}" : Name.ToString();
        }
    }
}
