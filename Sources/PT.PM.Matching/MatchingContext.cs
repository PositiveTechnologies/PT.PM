using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching
{
    public class MatchingContext : ILoggable
    {
        public PatternRoot PatternUst { get; }

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool FindAllAlternatives { get; set; } = false;

        public bool IncludeNonterminalTextSpans { get; set; } = true;

        public List<TextSpan> Locations { get; } = new List<TextSpan>();

        public Dictionary<string, IdToken> Vars { get; } = new Dictionary<string, IdToken>();

        public bool Success { get; private set; }

        public bool IgnoreLocations { get; set; }

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

        public static implicit operator bool(MatchingContext context)
        {
            return context.Success;
        }

        public MatchingContext(PatternRoot patternUst, Dictionary<string, IdToken> vars = null)
        {
            PatternUst = patternUst;
            if (vars != null)
                Vars = vars;
        }

        public MatchingContext AddUstIfSuccess(Ust ust)
        {
            if (Success && 
                !IgnoreLocations && (ust.IsTerminal || IncludeNonterminalTextSpans))
            {
                Locations.Add(ust.TextSpan);
            }
            return this;
        }

        public MatchingContext AddMatch(Ust ust)
        {
            Success = true;
            if (!IgnoreLocations &&
                !ust.TextSpan.IsEmpty && (ust.IsTerminal || IncludeNonterminalTextSpans))
            {
                Locations.Add(ust.TextSpan);
            }
            return this;
        }

        public MatchingContext AddMatches(IEnumerable<TextSpan> textSpans)
        {
            Success = true;
            if (!IgnoreLocations)
            {
                Locations.AddRange(textSpans);
            }
            return this;
        }

        public MatchingContext Fail()
        {
            Success = false;
            return this;
        }

        public MatchingContext MakeSuccess()
        {
            Success = true;
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
            if (vars != "")
            {
                vars = "; Vars: " + vars;
            }
            return $"Status: {Success}{vars}";
        }
    }
}
