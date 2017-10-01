using PT.PM.Common;
using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternOr : PatternBase
    {
        public List<PatternBase> Patterns { get; set; }

        public PatternOr()
        {
            Patterns = new List<PatternBase>();
        }

        public PatternOr(IEnumerable<PatternBase> patterns, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Patterns = patterns?.ToList()
                ?? throw new ArgumentNullException(nameof(patterns));
        }

        public PatternOr(params PatternBase[] expressions)
        {
            Patterns = expressions?.ToList()
                ?? throw new ArgumentNullException(nameof(expressions));
        }

        public override string ToString() => $"({(string.Join(" <|> ", Patterns))})";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            var matchedTextSpans = new List<TextSpan>();

            foreach (PatternBase alt in Patterns)
            {
                var altContext = MatchingContext.CreateWithInputParamsAndVars(context);
                MatchingContext match = alt.Match(ust, altContext);
                if (match.Success)
                {
                    matchedTextSpans.AddRange(match.Locations);
                    if (!context.FindAllAlternatives)
                    {
                        break;
                    }
                }
            }

            return context.AddMatches(matchedTextSpans);
        }
    }
}
