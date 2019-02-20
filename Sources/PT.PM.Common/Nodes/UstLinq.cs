using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;

namespace PT.PM.Common
{
    public static class UstLinq
    {
        public static void ApplyActionToDescendantsAndSelf(this Ust ust, Action<Ust> action)
        {
            // The root might be null itself
            if (ust == null)
            {
                return;
            }
            action(ust);

            foreach (Ust child in ust.Children)
            {
                child?.ApplyActionToDescendantsAndSelf(action);
            }
        }

        public static bool AnyDescendantOrSelf(this Ust ust, Func<Ust, bool> predicate)
        {
            if (predicate(ust))
            {
                return true;
            }

            foreach (Ust child in ust.Children)
            {
                if (child != null && child.AnyDescendantOrSelf(predicate))
                {
                    return true;
                }
            }

            return false;
        }

        public static List<Ust> DescendantsAndSelf(this Ust ust) => ust.WhereDescendantsOrSelf();

        public static List<Ust> WhereDescendantsOrSelf(this Ust ust, Func<Ust, bool> predicate = null)
        {
            var result = new List<Ust>();
            GetDescendantsAndSelf(ust);

            void GetDescendantsAndSelf(Ust localResult)
            {
                if (localResult != null)
                {
                    if (predicate == null || predicate(localResult))
                    {
                        result.Add(localResult);
                    }
                    foreach (Ust child in localResult.Children)
                    {
                        GetDescendantsAndSelf(child);
                    }
                }
            }

            return result;
        }
    }
}
