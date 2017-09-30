using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PT.PM.Common.Nodes
{
    public abstract class Ust : IComparable<Ust>, IEquatable<Ust>
    {
        [JsonIgnore]
        public int Kind => GetType().Name.GetHashCode();

        [JsonIgnore]
        public RootUst Root { get; set; }

        [JsonIgnore]
        public Ust Parent { get; set; }

        [JsonIgnore]
        public Ust[] Children => GetChildren(); // TODO: optimized performance

        [JsonIgnore]
        public virtual bool IsTerminal => false;

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
                return Kind;
            }

            var nodeTypeCompareResult = Kind - other.Kind;
            if (nodeTypeCompareResult != 0)
            {
                return nodeTypeCompareResult;
            }

            return Children.CompareTo(other.Children);
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
