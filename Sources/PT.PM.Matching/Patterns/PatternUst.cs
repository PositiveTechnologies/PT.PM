using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Reflection;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PT.PM.Matching.Patterns
{
    public abstract class PatternUst : IComparable<PatternUst>, IEquatable<PatternUst>
    {
        private static readonly PropertyComparer<PatternUst> propertyComparer = new PropertyComparer<PatternUst>
        {
            IgnoredProperties = new HashSet<string> { nameof(Parent), nameof(Root), nameof(TextSpan) }
        };

        [JsonIgnore]
        public string Kind => GetType().Name;

        [JsonIgnore]
        public virtual bool Any => true;

        [JsonIgnore]
        public PatternRoot Root { get; set; }

        [JsonIgnore]
        public PatternUst Parent { get; set; }

        [JsonIgnore]
        public LineColumnTextSpan LineColumnTextSpan => Root?.File.GetLineColumnTextSpan(TextSpan);

        public TextSpan TextSpan { get; set; }

        protected PatternUst(TextSpan textSpan = default)
        {
            TextSpan = textSpan;
        }

        public bool Equals(PatternUst other)
        {
            return CompareTo(other) == 0;
        }

        public int CompareTo(PatternUst other)
        {
            return propertyComparer.Compare(this, other);
        }

        public MatchContext MatchUst(Ust ust, MatchContext context)
        {
            context.PushParent(ust);
            context = Match(ust, context);
            context.PopParent();
            return context;
        }

        protected abstract MatchContext Match(Ust ust, MatchContext context);
    }
}
