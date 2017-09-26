using PT.PM.Common;
using PT.PM.Matching.Patterns;
using System.Collections.Generic;

namespace PT.PM.Matching
{
    public class MatchingContext : ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public PatternRootUst PatternUst { get; }

        public List<TextSpan> Locations { get; set; } = new List<TextSpan>();

        public List<MatchingResult> Results { get; set; } = new List<MatchingResult>();

        public MatchingContext(PatternRootUst patternUst)
        {
            PatternUst = patternUst;
        }

        public void AddLocation(TextSpan textSpan)
        {
            Locations.Add(textSpan);
        }
    }
}
