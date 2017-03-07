using PT.PM.Common;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.TypeMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common.Symbols
{
    public class MethodSymbol : ISymbol
    {
        public string Name { get; set; }

        public TextSpan TextSpan { get; set; }

        public MethodDeclaration MethodDeclaration { get; set; }

        public BlockStatement BlockStatement { get; set; }

        public MethodSymbol(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
