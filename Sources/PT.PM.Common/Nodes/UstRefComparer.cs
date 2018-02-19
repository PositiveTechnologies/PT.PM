using PT.PM.Common.Nodes;
using System.Collections.Generic;

namespace PT.PM.Common
{
    public class UstRefComparer : IEqualityComparer<Ust>
    {
        public static UstRefComparer Instance = new UstRefComparer();

        public bool Equals(Ust x, Ust y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(Ust obj)
        {
            return obj.GetHashCode();
        }
    }
}
