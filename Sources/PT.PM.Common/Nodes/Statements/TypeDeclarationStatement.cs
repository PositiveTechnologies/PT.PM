using MessagePack;
using PT.PM.Common.Nodes.GeneralScope;

namespace PT.PM.Common.Nodes.Statements
{
    [MessagePackObject]
    public class TypeDeclarationStatement : Statement
    {
        [Key(UstFieldOffset)]
        public TypeDeclaration TypeDeclaration { get;set; }

        public TypeDeclarationStatement()
        {
        }

        public TypeDeclarationStatement(TypeDeclaration typeDeclaration, TextSpan textSpan)
            : base(textSpan)
        {
            TypeDeclaration = typeDeclaration;
        }

        public override Ust[] GetChildren()
        {
            var result = new[] { TypeDeclaration };
            return result;
        }

        public override string ToString()
        {
            return TypeDeclaration.ToString();
        }
    }
}
