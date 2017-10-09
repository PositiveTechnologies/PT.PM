using System;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Reflection;
using System.Collections.Generic;

namespace PT.PM.Matching.Patterns
{
    public abstract class PatternUst : IComparable<PatternUst>, IEquatable<PatternUst>, IUst<PatternUst, PatternRoot>, IUst
    {
        private PropertyComparer<PatternUst> propertyComparer = new PropertyComparer<PatternUst>()
        {
            IgnoredProperties = new HashSet<string>() { nameof(Parent), nameof(Root), nameof(TextSpan) }
        };

        public int KindId => GetType().Name.GetHashCode();

        public PatternRoot Root { get; set; }

        public PatternUst Parent { get; set; }

        public TextSpan TextSpan { get; set; }

        protected PatternUst()
        {
        }

        protected PatternUst(TextSpan textSpan = default(TextSpan))
        {
            TextSpan = textSpan;
        }

        public abstract MatchingContext Match(Ust ust, MatchingContext context);

        public bool Equals(PatternUst other)
        {
            return CompareTo(other) == 0;
        }

        public int CompareTo(PatternUst other)
        {
            return propertyComparer.Compare(this, other);
        }
    }
}
