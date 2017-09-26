using PT.PM.Common;
using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternOr : PatternBase
    {
        public List<PatternBase> Expressions { get; set; }

        public PatternOr()
        {
            Expressions = new List<PatternBase>();
        }

        public PatternOr(IEnumerable<PatternBase> expressions, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Expressions = expressions?.ToList()
                ?? throw new ArgumentNullException(nameof(expressions));
        }

        public override Ust[] GetChildren() => Expressions.ToArray();

        public override string ToString() => $"({(string.Join(" <|> ", Expressions))})";

        public override bool Match(Ust ust, MatchingContext context)
        {
            foreach (PatternBase expression in Expressions)
            {
                bool match = expression.Match(ust, context);
                if (match)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
