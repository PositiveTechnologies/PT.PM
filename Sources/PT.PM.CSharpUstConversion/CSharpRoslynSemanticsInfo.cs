using PT.PM.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.CSharpUstConversion
{
    public class CSharpRoslynSemanticsInfo : SemanticsInfo
    {
        public override Language Language => Language.CSharp;

        public CSharpCompilation Compilation { get; set; }

        public CSharpRoslynSemanticsInfo(CSharpCompilation compilation)
        {
            Compilation = compilation;
        }
    }
}
