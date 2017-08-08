using System;
using System.Collections.Generic;
using System.Text;
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

            return UstNodeHelper.CompareCollections(Children, other.Children);
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

    class UstNodeHelper
    {
        public static int CompareCollections(UstNode[] collection1, UstNode[] collection2)
        {
            if (collection1 == null && collection2 == null)
            {
                return 0;
            }

            if (collection1 != null && collection2 == null)
            {
                return collection1.Length;
            }

            if (collection1 == null && collection2 != null)
            {
                return -collection2.Length;
            }

            var collectionCountCompareResult = collection1.Length - collection2.Length;
            if (collectionCountCompareResult != 0)
            {
                return collectionCountCompareResult;
            }

            for (int i = 0; i < collection1.Length; i++)
            {
                var element = collection1[i];
                if (element == null)
                {
                    if (collection2[i] != null)
                    {
                        return -(int)collection2[i].NodeType;
                    }
                }
                else if (element.NodeType != NodeType.FileNode)
                {
                    var elementCompareResult = element.CompareTo(collection2[i]);
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
