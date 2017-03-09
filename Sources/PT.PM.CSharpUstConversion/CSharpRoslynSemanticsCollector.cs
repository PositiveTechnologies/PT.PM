using PT.PM.Common;
using PT.PM.CSharpAstConversion;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.CSharpAstConversion
{
    public class CSharpRoslynSemanticsCollector : ISemanticsCollector
    {
        private CSharpCompilation compilation;
        private IEnumerable<CSharpRoslynParseTree> csharpRoslynAsts;

        public CSharpRoslynSemanticsCollector()
        {
        }

        public SemanticsInfo Collect(IEnumerable<ParseTree> asts)
        {
            csharpRoslynAsts = asts.Select(ast => (CSharpRoslynParseTree)ast);
            // TODO: assembly name
            
            var references = new[] {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
            };
            compilation = CSharpCompilation.Create("tempAssembly", csharpRoslynAsts.Select(ast => ast.SyntaxTree),
                references);

            var result = new CSharpRoslynSemanticsInfo(compilation);
            return result;
        }
    }
}
