using PT.PM.Common;
using PT.PM.Common.Nodes;
using System;

namespace PT.PM.Matching.Patterns
{
    public class PatternVar : PatternBase
    {
        public string Id { get; set; }

        public PatternVar()
        {
        }

        public PatternVar(string id, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Id = id;
        }

        public PatternBase Value { get; set; } = new PatternIdRegexToken();

        public override Ust[] GetChildren() => new Ust[] { Value };

        public override string ToString() => $"{Id}: {Value}";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
