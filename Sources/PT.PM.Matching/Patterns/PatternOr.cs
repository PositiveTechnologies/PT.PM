using PT.PM.Common;
using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternOr : PatternBase
    {
        public List<PatternBase> Alternatives { get; set; }

        public PatternOr()
        {
            Alternatives = new List<PatternBase>();
        }

        public PatternOr(IEnumerable<PatternBase> expressions, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Alternatives = expressions?.ToList()
                ?? throw new ArgumentNullException(nameof(expressions));
        }

        public PatternOr(params PatternBase[] expressions)
        {
            Alternatives = expressions?.ToList()
                ?? throw new ArgumentNullException(nameof(expressions));
        }

        public override Ust[] GetChildren() => Alternatives.ToArray();

        public override string ToString() => $"({(string.Join(" <|> ", Alternatives))})";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            var textSpans = new List<TextSpan>();
            foreach (PatternBase expression in Alternatives)
            {
                var newContext = new MatchingContext(context.PatternUst)
                {
                    Logger = context.Logger,
                    FindAllAlternatives = context.FindAllAlternatives
                };
                MatchingContext match = expression.Match(ust, newContext);
                if (match.Success)
                {
                    textSpans.AddRange(match.Locations);
                    if (!context.FindAllAlternatives)
                    {
                        break;
                    }
                }
            }

            return context.AddLocations(textSpans);
        }
    }
}
