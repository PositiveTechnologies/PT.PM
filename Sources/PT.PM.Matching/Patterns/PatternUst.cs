using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Reflection;
using System;
using System.Collections.Generic;

namespace PT.PM.Matching.Patterns
{
    public abstract class PatternUst : IComparable<PatternUst>, IEquatable<PatternUst>
    {
        private PropertyComparer<PatternUst> propertyComparer = new PropertyComparer<PatternUst>()
        {
            IgnoredProperties = new HashSet<string>() { nameof(Parent), nameof(Root), nameof(TextSpan) }
        };

        public int KindId => GetType().Name.GetHashCode();

        public PatternRoot Root { get; set; }

        public PatternUst Parent { get; set; }

        public TextSpan TextSpan { get; set; }

        public bool Equals(PatternUst other)
        {
            return CompareTo(other) == 0;
        }

        public int CompareTo(PatternUst other)
        {
            return propertyComparer.Compare(this, other);
        }

        public abstract MatchingContext MatchUst(Ust ust, MatchingContext context);
    }

    public abstract class PatternUst<TMatchUst> : PatternUst, IUst<PatternUst, PatternRoot>, IUst
        where TMatchUst : Ust
    {
        protected PatternUst()
        {
        }

        protected PatternUst(TextSpan textSpan = default(TextSpan))
        {
            TextSpan = textSpan;
        }

        public override MatchingContext MatchUst(Ust ust, MatchingContext context)
        {
            if (ust is TMatchUst matchUst)
            {
                return Match(matchUst, context);
            }

            return context.Fail();
        }

        public abstract MatchingContext Match(TMatchUst ust, MatchingContext context);
    }
}
