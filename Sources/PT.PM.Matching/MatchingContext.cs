using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
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

        public Dictionary<string, IdToken> Vars { get; } = new Dictionary<string, IdToken>();

        public bool Success { get; private set; } = true;

        public static MatchingContext CreateWithInputParamsAndVars(MatchingContext context)
        {
            return CreateWithInputParams(context, context.Vars);
        }

        public static MatchingContext CreateWithInputParams(MatchingContext context, Dictionary<string, IdToken> vars = null)
        {
            return new MatchingContext(context.PatternUst, vars)
            {
                Logger = context.Logger,
                FindAllAlternatives = context.FindAllAlternatives,
                IncludeNonterminalTextSpans = context.IncludeNonterminalTextSpans,
            };
        }

        public MatchingContext(PatternRootUst patternUst, Dictionary<string, IdToken> vars = null)
        {
            PatternUst = patternUst;
            if (vars != null)
                Vars = vars;
        }

        public MatchingContext AddMatchIfSuccess(Ust ust)
        {
            if (Success && (ust.IsTerminal || IncludeNonterminalTextSpans))
            {
                Locations.Add(ust.TextSpan);
            }
            return this;
        }

        public MatchingContext AddMatch(Ust ust)
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

        public MatchingContext AddMatches(IEnumerable<TextSpan> textSpans)
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

        public override string ToString()
        {
            string vars = string.Join(", ", Vars.Select(v => $"{v.Key}: {v.Value}"));
            return $"Status: {Success}; Vars: {vars}";
        }
    }
}
