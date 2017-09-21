using PT.PM.Common.Nodes.GeneralScope;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common.Nodes.Statements
{
    public class TypeDeclarationStatement : Statement
    {
        public override UstKind Kind => UstKind.TypeDeclarationStatement;

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
    }
}
