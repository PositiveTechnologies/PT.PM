using PT.PM.Common.Nodes;
using System.Collections.Generic;

namespace PT.PM.Common
{
    public class UstNodeRefComparer : IEqualityComparer<Nodes.Ust>
    {
        public static UstNodeRefComparer Instance = new UstNodeRefComparer();

        public bool Equals(Nodes.Ust x, Nodes.Ust y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(Nodes.Ust obj)
        {
            return obj.GetHashCode();
        }
    }
}
