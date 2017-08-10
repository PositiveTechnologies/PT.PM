using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;


namespace PT.PM.Common.Nodes
{
    public abstract class UstNode : Node, IComparable<UstNode>, IEquatable<UstNode>
    {
        private UstNode[] children;

        public abstract NodeType NodeType { get; }

        [JsonIgnore]
        public UstNode Parent { get; set; }

        [JsonIgnore]
        public UstNode[] Children => children ?? (children = GetChildren());

        [JsonIgnore]
        public virtual bool IsLiteral => false;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TextSpan TextSpan { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object CfgNode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object PdgNode { get; set; }

        protected UstNode(TextSpan textSpan, FileNode fileNode)
            : this(textSpan)
        {
            FileNode = fileNode;
        }

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

    static class UstNodeHelper
    {
        public static int CompareTo(this IEnumerable<UstNode> collection1, IEnumerable<UstNode> collection2)
        {
            var list1 = new List<UstNode>(collection1 ?? Enumerable.Empty<UstNode>());
            var list2 = new List<UstNode>(collection2 ?? Enumerable.Empty<UstNode>());

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
                else if (element.NodeType != NodeType.FileNode)
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
