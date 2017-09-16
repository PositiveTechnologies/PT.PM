using System.Collections.Generic;
using System.Linq;
using PT.PM.Common.Nodes;

namespace PT.PM.Patterns.Nodes
{
    public static class PatternDeclarationHelper
    {
        public static int CompareSubset<T>(this IEnumerable<T> collection1, IEnumerable<T> collection2) where T : UstNode
        {
            var sublistToMatch = new List<T>(collection1 ?? Enumerable.Empty<T>());
            var list = collection2 as IList<T> ?? new List<T>(collection2 ?? Enumerable.Empty<T>());

            foreach (var el in list)
            {
                sublistToMatch.Remove(sublistToMatch.Find(m => m.CompareTo(el) == 0));
            }

            return -sublistToMatch.Count();
        }
    }
}
