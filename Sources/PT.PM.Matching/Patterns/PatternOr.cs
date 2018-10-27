using PT.PM.Common;
using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternOr : PatternUst
    {
        public List<PatternUst> Patterns { get; set; }

        public PatternOr()
        {
            Patterns = new List<PatternUst>();
        }

        public PatternOr(IEnumerable<PatternUst> patterns, TextSpan textSpan = default)
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

        public override MatchContext Match(Ust ust, MatchContext context)
        {
            var matchedTextSpans = new List<TextSpan>();

            bool success = false;
            foreach (PatternUst pattern in Patterns)
            {
                var altContext = MatchContext.CreateWithInputParamsAndVars(context);
                MatchContext match = pattern.Match(ust, altContext);
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
