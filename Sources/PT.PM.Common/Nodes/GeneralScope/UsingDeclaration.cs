using MessagePack;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.GeneralScope
{
    [MessagePackObject]
    public class UsingDeclaration : Ust
    {
        [Key(UstFieldOffset)]
        public StringLiteral Name { get; set; }

        public UsingDeclaration(StringLiteral name, TextSpan textSpan)
            : base(textSpan)
        {
            Name = name;
        }

        public UsingDeclaration()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] {Name};
        }
    }
}
