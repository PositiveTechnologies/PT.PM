using PT.PM.Common;
using PT.PM.CSharpParseTreeUst;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.CSharpParseTreeUst
{
    public class CSharpRoslynSemanticsCollector : ISemanticsCollector
    {
        private CSharpCompilation compilation;
        private IEnumerable<CSharpRoslynParseTree> csharpRoslynParseTrees;

        public CSharpRoslynSemanticsCollector()
        {
        }

        public SemanticsInfo Collect(IEnumerable<ParseTree> parseTrees)
        {
            csharpRoslynParseTrees = parseTrees.Select(parseTree => (CSharpRoslynParseTree)parseTree);
            // TODO: assembly name
            
            var references = new[] {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
            };
            compilation = CSharpCompilation.Create("tempAssembly", csharpRoslynParseTrees.Select(ust => ust.SyntaxTree),
                references);

            var result = new CSharpRoslynSemanticsInfo(compilation);
            return result;
        }
    }
}
