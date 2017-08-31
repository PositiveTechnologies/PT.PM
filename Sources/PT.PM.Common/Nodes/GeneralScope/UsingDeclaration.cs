using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.GeneralScope
{
    public class UsingDeclaration : UstNode
    {
        public override NodeType NodeType => NodeType.UsingDeclaration;

        public StringLiteral Name { get; set; }

        public UsingDeclaration(StringLiteral name, TextSpan textSpan)
            : base(textSpan)
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
