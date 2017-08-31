using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.TypeMembers;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.Specific
{
    public class CSharpParameterDeclaration : ParameterDeclaration
    {
        public override NodeType NodeType => NodeType.CSharpParameterDeclaration;

        public ParameterModifierLiteral Modifier { get; set; }

        public Expression DefaultExpression { get; set; }

        public CSharpParameterDeclaration(TypeToken type, IdToken name, TextSpan textSpan)
            : base(type, name, textSpan)
        {
            Name = name;
        }

        public CSharpParameterDeclaration()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
            result.Add(Type);
            result.Add(Name);
            if (Modifier != null)
                result.Add(Modifier);
            if (DefaultExpression != null)
                result.Add(DefaultExpression);
            return result.ToArray();
        }
    }
}
