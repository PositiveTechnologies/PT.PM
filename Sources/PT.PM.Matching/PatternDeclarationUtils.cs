using System.Collections.Generic;
using System.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Matching.Patterns;

namespace PT.PM.Matching
{
    public static class PatternDeclarationUtils
    {
        public static MatchingContext MatchSubset<T>(this IEnumerable<IPatternUst> collection1, IEnumerable<T> collection2, MatchingContext context)
            where T : Ust
        {
            var sublistToMatch = new List<IPatternUst>(collection1 ?? Enumerable.Empty<IPatternUst>());
            var list = collection2 as IList<T> ?? new List<T>(collection2 ?? Enumerable.Empty<T>());

            foreach (T element in list)
            {
                sublistToMatch.Remove(sublistToMatch.Find(m => m.Match(element, context).Success));
            }

            return context.Fail();
        }
    }
}
