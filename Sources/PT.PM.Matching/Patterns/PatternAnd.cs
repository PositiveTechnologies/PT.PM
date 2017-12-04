using PT.PM.Common;
using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternAnd : PatternUst<Ust>
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

        public override MatchContext Match(Ust ust, MatchContext context)
        {
            MatchContext newContext = context;

            foreach (PatternUst pattern in Patterns)
            {
                newContext = pattern.MatchUst(ust, newContext);
                if (!newContext.Success)
                {
                    return newContext.Fail();
                }
            }

            return newContext.AddMatch(ust);
        }
    }
}
