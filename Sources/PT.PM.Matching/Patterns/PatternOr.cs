using PT.PM.Common;
using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternOr : PatternUst<Ust>
    {
        public List<PatternUst> Patterns { get; set; }

        public PatternOr()
        {
            Patterns = new List<PatternUst>();
        }

        public PatternOr(IEnumerable<PatternUst> patterns, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Patterns = patterns?.ToList()
                ?? throw new ArgumentNullException(nameof(patterns));
        }

        public PatternOr(params PatternUst[] expressions)
        {
            Patterns = expressions?.ToList()
                ?? throw new ArgumentNullException(nameof(expressions));
        }

        public override string ToString() => $"({(string.Join(" <|> ", Patterns))})";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            var matchedTextSpans = new List<TextSpan>();

            bool success = false;
            foreach (PatternUst pattern in Patterns)
            {
                var altContext = MatchingContext.CreateWithInputParamsAndVars(context);
                MatchingContext match = pattern.MatchUst(ust, altContext);
                if (match.Success)
                {
                    success = true;
                    matchedTextSpans.AddRange(match.Locations);
                    if (!context.FindAllAlternatives)
                    {
                        break;
                    }
                }
            }

            return success
                ? context.AddMatches(matchedTextSpans)
                : context.Fail();
        }
    }
}
