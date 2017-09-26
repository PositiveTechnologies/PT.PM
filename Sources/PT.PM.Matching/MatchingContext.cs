using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Matching.Patterns;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching
{
    public class MatchingContext : ILoggable
    {
        public PatternRootUst PatternUst { get; }

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool FindAllAlternatives { get; set; } = false;

        public bool IncludeNonterminalTextSpans { get; set; } = false;

        public List<TextSpan> Locations { get; } = new List<TextSpan>();

        public List<MatchingResult> Results { get; } = new List<MatchingResult>();

        public bool Success { get; private set; } = true;

        public MatchingContext(PatternRootUst patternUst)
        {
            PatternUst = patternUst;
        }

        public MatchingContext(PatternRootUst patternUst,
            List<TextSpan> locations,
            List<MatchingResult> results)
        {
            PatternUst = patternUst;
            Locations = locations;
            Results = results;
        }

        public MatchingContext AddUstIfSuccess(Ust ust)
        {
            if (Success && (ust.IsTerminal || IncludeNonterminalTextSpans))
            {
                Locations.Add(ust.TextSpan);
            }
            return this;
        }

        public MatchingContext AddUst(Ust ust)
        {
            if (ust.TextSpan.IsEmpty)
            {
                Success = false;
            }
            else
            {
                Success = true;
                if (ust.IsTerminal || IncludeNonterminalTextSpans)
                {
                    Locations.Add(ust.TextSpan);
                }
            }
            return this;
        }

        public MatchingContext AddLocations(IEnumerable<TextSpan> textSpans)
        {
            Success = textSpans.Count() > 0;
            Locations.AddRange(textSpans);
            return this;
        }

        public MatchingContext Fail()
        {
            Success = false;
            return this;
        }

        public MatchingContext Change(bool success)
        {
            Success = success;
            return this;
        }
    }
}
