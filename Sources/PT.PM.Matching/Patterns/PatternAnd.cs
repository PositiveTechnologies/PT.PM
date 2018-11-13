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
            Patterns = expressions as List<PatternUst> ?? expressions?.ToList()
                ?? throw new ArgumentNullException(nameof(expressions));
        }

        public PatternAnd(params PatternUst[] expressions)
        {
            Patterns = expressions.ToList();
        }

        public override string ToString() => $"({(string.Join(" <&> ", Patterns))})";

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            foreach (PatternUst pattern in Patterns)
            {
                context = pattern.MatchUst(ust, context);
                if (!context.Success)
                {
                    return context.Fail();
                }
            }

            return context.AddMatch(ust);
        }
    }
}
