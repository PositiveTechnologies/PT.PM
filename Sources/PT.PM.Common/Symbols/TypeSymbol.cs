using PT.PM.Common;
using PT.PM.Common.Nodes.GeneralScope;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common.Symbols
{
    public class TypeSymbol : ISymbol
    {
        public string Name { get; set; }

        public TextSpan TextSpan { get; set; }

        public TypeDeclaration TypeDeclaration { get; set; }

        public Dictionary<string, MethodSymbol> MethodSymbols { get; set; } = new Dictionary<string, MethodSymbol>();

        public TypeSymbol(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
