using PT.PM.Common.Ust;
using PT.PM.Common.Symbols;
using System.Collections.Generic;

namespace PT.PM.UstPreprocessing
{
    public class UstSemanticsCollector
    {
        public UstSemantics Collect(IEnumerable<Ust> asts)
        {
            Dictionary<string, TypeSymbol> typeSymbols = new Dictionary<string, TypeSymbol>();

            foreach (var ast in asts)
            {
                var walker = new UstSemanticsCollectorWalker();
                walker.Walk(ast.Root);
                
                foreach (var typeSymbol in walker.TypeSymbols)
                {
                    typeSymbols[typeSymbol.Key] = typeSymbol.Value;
                }
            }

            return new UstSemantics();
        }
    }
}
