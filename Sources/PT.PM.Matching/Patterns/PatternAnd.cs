using PT.PM.Common;
using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternAnd : PatternBase
    {
        public List<PatternBase> Expressions { get; set; } = new List<PatternBase>();

        public PatternAnd()
        {
        }

        public PatternAnd(IEnumerable<PatternBase> expressions, TextSpan textSpan) :
            base(textSpan)
        {
            Expressions = expressions?.ToList()
                ?? throw new ArgumentNullException(nameof(expressions));
        }

        public PatternAnd(params PatternBase[] expressions)
        {
            Expressions = expressions.ToList();
        }

        public override Ust[] GetChildren() => Expressions.ToArray();

        public override string ToString() => $"({(string.Join(" <&> ", Expressions))})";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext match = context;

            foreach (PatternBase expression in Expressions)
            {
                match = expression.Match(ust, match);
                if (!match.Success)
                {
                    return match;
                }
            }

            return match;
        }
    }
}
