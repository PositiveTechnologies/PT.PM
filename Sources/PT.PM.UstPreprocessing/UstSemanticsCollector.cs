using PT.PM.Common.Ust;
using PT.PM.Common.Symbols;
using System.Collections.Generic;

namespace PT.PM.UstPreprocessing
{
    public class UstSemanticsCollector
    {
        public UstSemantics Collect(IEnumerable<Ust> usts)
        {
            Dictionary<string, TypeSymbol> typeSymbols = new Dictionary<string, TypeSymbol>();

            foreach (var ust in usts)
            {
                var walker = new UstSemanticsCollectorWalker();
                walker.Walk(ust.Root);
                
                foreach (var typeSymbol in walker.TypeSymbols)
                {
                    typeSymbols[typeSymbol.Key] = typeSymbol.Value;
                }
            }

            return new UstSemantics();
        }
    }
}
