using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;

namespace PT.PM.Common
{
    public static class UstLinq
    {
        public static void ApplyActionToDescendants(this Ust ust, Action<Ust> action)
        {
            foreach (var child in ust.Children)
            {
                if (child != null)
                {
                    action(child);
                    child.ApplyActionToDescendants(action);
                }
            }
        }

        public static bool AnyDescendant(this Ust ust, Func<Ust, bool> predicate)
        {
            if (predicate(ust))
            {
                return true;
            }
            foreach (var child in ust.Children)
            {
                if (child != null && child.AnyDescendant(predicate))
                {
                    return true;
                }
            }
            return false;
        }

        public static List<Ust> Descendants(this Ust ust) => ust.WhereDescendants();

        public static List<Ust> WhereDescendants(this Ust ust, Func<Ust, bool> predicate = null)
        {
            var result = new List<Ust>();
            GetDescendants(ust);

            void GetDescendants(Ust localResult)
            {
                if (predicate == null || predicate(localResult))
                {
                    result.Add(localResult);
                }
                foreach (Ust child in localResult.Children)
                {
                    GetDescendants(child);
                }
            }

            return result;
        }
    }
}
