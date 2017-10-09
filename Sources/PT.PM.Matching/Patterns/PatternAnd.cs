using PT.PM.Common;
using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternAnd : PatternUst
    {
        public List<PatternUst> Patterns { get; set; } = new List<PatternUst>();

        public PatternAnd()
        {
        }

        public PatternAnd(IEnumerable<PatternUst> expressions, TextSpan textSpan) :
            base(textSpan)
        {
            Patterns = expressions?.ToList()
                ?? throw new ArgumentNullException(nameof(expressions));
        }

        public PatternAnd(params PatternUst[] expressions)
        {
            Patterns = expressions.ToList();
        }

        public override string ToString() => $"({(string.Join(" <&> ", Patterns))})";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext = context;

            foreach (PatternUst expression in Patterns)
            {
                newContext = expression.Match(ust, newContext);
                if (!newContext.Success)
                {
                    return newContext.Fail();
                }
            }

            return newContext.AddMatch(ust);
        }
    }
}
