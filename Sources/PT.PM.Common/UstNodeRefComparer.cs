using PT.PM.Common.Nodes;
using System.Collections.Generic;

namespace PT.PM.Common
{
    public class UstNodeRefComparer : IEqualityComparer<UstNode>
    {
        public static UstNodeRefComparer Instance = new UstNodeRefComparer();

        public bool Equals(UstNode x, UstNode y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(UstNode obj)
        {
            return obj.GetHashCode();
        }
    }
}
