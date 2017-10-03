using System;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Reflection;
using System.Collections.Generic;

namespace PT.PM.Matching.Patterns
{
    public abstract class PatternBase : IComparable<PatternBase>, IEquatable<PatternBase>, IUst<PatternBase, PatternRoot>, IUst
    {
        private PropertyComparer<PatternBase> propertyComparer = new PropertyComparer<PatternBase>()
        {
            IgnoredProperties = new HashSet<string>() { nameof(Parent), nameof(Root), nameof(TextSpan) }
        };

        public int KindId => GetType().Name.GetHashCode();

        public PatternRoot Root { get; set; }

        public PatternBase Parent { get; set; }

        public TextSpan TextSpan { get; set; }

        protected PatternBase()
        {
        }

        protected PatternBase(TextSpan textSpan = default(TextSpan))
        {
            TextSpan = textSpan;
        }

        public MatchingContext Match(Ust ust)
        {
            return Match(ust, new MatchingContext(Root));
        }

        public abstract MatchingContext Match(Ust ust, MatchingContext context);

        public bool Equals(PatternBase other)
        {
            return CompareTo(other) == 0;
        }

        public int CompareTo(PatternBase other)
        {
            return propertyComparer.Compare(this, other);
        }
    }
}
