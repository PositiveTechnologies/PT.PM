using PT.PM.Common.Nodes;
using PT.PM.Matching.Patterns;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching
{
    public static class PatternDeclarationUtils
    {
        public static MatchingContext MatchSubset<T>(this IEnumerable<PatternBase> collection1, IEnumerable<T> collection2, MatchingContext context)
            where T : Ust
        {
            var sublistToMatch = new List<PatternBase>(collection1 ?? Enumerable.Empty<PatternBase>());
            var list = collection2 as IList<T> ?? new List<T>(collection2 ?? Enumerable.Empty<T>());

            if (sublistToMatch.Count == 0 && list.Count == 0)
            {
                return context.MakeSuccess();
            }

            var matches = new List<Ust>(list.Count);
            foreach (T element in list)
            {
                MatchingContext newContext = MatchingContext.CreateWithInputParams(context);

                int i = 0;
                while (i < sublistToMatch.Count)
                {
                    newContext = sublistToMatch[i].Match(element, newContext);
                    if (newContext.Success)
                    {
                        matches.Add(element);
                        sublistToMatch.RemoveAt(i);
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }

                if (!newContext.Success)
                {
                    return context.Fail();
                }
            }

            foreach (Ust match in matches)
            {
                context = context.AddMatch(match);
            }

            return context;
        }
    }
}
