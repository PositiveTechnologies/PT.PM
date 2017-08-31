using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;

namespace PT.PM.Common.Nodes
{
    public abstract class UstNode : IComparable<UstNode>, IEquatable<UstNode>
    {
        public abstract NodeType NodeType { get; }

        [JsonIgnore]
        public RootNode Root { get; set; }

        [JsonIgnore]
        public UstNode Parent { get; set; }

        [JsonIgnore]
        public UstNode[] Children => GetChildren(); // TODO: optimized performance

        [JsonIgnore]
        public virtual bool IsLiteral => false;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TextSpan TextSpan { get; set; }

        protected UstNode(TextSpan textSpan)
            : this()
        {
            TextSpan = textSpan;
        }

        protected UstNode()
        {
        }

        public abstract UstNode[] GetChildren();

        public bool Equals(UstNode other)
        {
            return CompareTo(other) == 0;
        }

        public virtual int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return (int)NodeType;
            }

            var nodeTypeCompareResult = NodeType - other.NodeType;
            if (nodeTypeCompareResult != 0)
            {
                return nodeTypeCompareResult;
            }

            return Children.CompareTo(other.Children);
        }

        public bool DoesAnyDescendantMatchPredicate(Func<UstNode, bool> predicate)
        {
            if (predicate(this))
            {
                return true;
            }
            foreach (var child in Children)
            {
                if (child != null && child.DoesAnyDescendantMatchPredicate(predicate))
                {
                    return true;
                }
            }
            return false;
        }

        public bool DoesAllDescendantsMatchPredicate(Func<UstNode, bool> predicate)
        {
            if (!predicate(this))
            {
                return false;
            }
            foreach (var child in Children)
            {
                if (child != null && !child.DoesAnyDescendantMatchPredicate(predicate))
                {
                    return false;
                }
            }
            return true;
        }

        public void ApplyActionToDescendants(Action<UstNode> action)
        {
            foreach (var child in Children)
            {
                if (child != null)
                {
                    action(child);
                    child.ApplyActionToDescendants(action);
                }
            }
        }

        public UstNode[] GetAllDescendants()
        {
            return GetAllDescendants(node => true);
        }

        public UstNode[] GetAllDescendants(Func<UstNode, bool> predicate)
        {
            var result = new List<UstNode>();
            GetAllDescendants(result, predicate);
            return result.ToArray();
        }

        protected void GetAllDescendants(List<UstNode> result, Func<UstNode, bool> predicate)
        {
            if (predicate(this))
            {
                result.Add(this);
            }
            foreach (var child in Children)
            {
                child?.GetAllDescendants(result, predicate);
            }
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            foreach (var child in Children)
            {
                result.Append(child);
                result.Append(" ");
            }
            return result.ToString();
        }
    }

    public static class UstNodeHelper
    {
        public static int CompareTo<T>(this IEnumerable<T> collection1, IEnumerable<T> collection2) where T : UstNode
        {
            var list1 = collection1 as IList<T> ?? new List<T>(collection1 ?? Enumerable.Empty<T>());
            var list2 = collection2 as IList<T> ?? new List<T>(collection2 ?? Enumerable.Empty<T>());

            var collectionCountCompareResult = list1.Count - list2.Count;
            if (collectionCountCompareResult != 0)
            {
                return collectionCountCompareResult;
            }

            for (int i = 0; i < list1.Count; i++)
            {
                var element = list1[i];
                if (element == null)
                {
                    if (list2[i] != null)
                    {
                        return -(int)list2[i].NodeType;
                    }
                }
                else if (element.NodeType != NodeType.RootNode)
                {
                    var elementCompareResult = element.CompareTo(list2[i]);
                    if (elementCompareResult != 0)
                    {
                        return elementCompareResult;
                    }
                }
            }

            return 0;
        }
    }
}
