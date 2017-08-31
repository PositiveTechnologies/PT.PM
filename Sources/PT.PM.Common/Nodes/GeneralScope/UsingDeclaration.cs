using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.GeneralScope
{
    public class UsingDeclaration : UstNode
    {
        public override NodeType NodeType => NodeType.UsingDeclaration;

        public StringLiteral Name { get; set; }

        public UsingDeclaration(StringLiteral name, TextSpan textSpan, RootNode fileNode)
            : base(textSpan, fileNode)
        {
            Name = name;
        }

        public UsingDeclaration()
        {
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] {Name};
        }
    }
}
