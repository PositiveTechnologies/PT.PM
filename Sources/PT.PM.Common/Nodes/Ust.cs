using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PT.PM.Common.Nodes
{
    public abstract class Ust : IComparable<Ust>, IEquatable<Ust>
    {
        public abstract UstKind Kind { get; }

        [JsonIgnore]
        public RootUst Root { get; set; }

        [JsonIgnore]
        public Ust Parent { get; set; }

        [JsonIgnore]
        public Ust[] Children => GetChildren(); // TODO: optimized performance

        [JsonIgnore]
        public virtual bool IsLiteral => false;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TextSpan TextSpan { get; set; }

        protected Ust(TextSpan textSpan)
            : this()
        {
            TextSpan = textSpan;
        }

        protected Ust()
        {
        }

        public abstract Ust[] GetChildren();

        public bool Equals(Ust other)
        {
            return CompareTo(other) == 0;
        }

        public virtual int CompareTo(Ust other)
        {
            if (other == null)
            {
                return (int)Kind;
            }

            var nodeTypeCompareResult = Kind - other.Kind;
            if (nodeTypeCompareResult != 0)
            {
                return nodeTypeCompareResult;
            }

            return Children.CompareTo(other.Children);
        }

        public bool DoesAnyDescendantMatchPredicate(Func<Ust, bool> predicate)
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

        public bool DoesAllDescendantsMatchPredicate(Func<Ust, bool> predicate)
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

        public void ApplyActionToDescendants(Action<Ust> action)
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

        public Ust[] GetAllDescendants()
        {
            return GetAllDescendants(node => true);
        }

        public Ust[] GetAllDescendants(Func<Ust, bool> predicate)
        {
            var result = new List<Ust>();
            GetAllDescendants(result, predicate);
            return result.ToArray();
        }

        protected void GetAllDescendants(List<Ust> result, Func<Ust, bool> predicate)
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
}
