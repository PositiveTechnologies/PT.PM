using PT.PM.Common;
using PT.PM.Matching;

namespace PT.PM.PatternEditor
{
    public class MatchResultDtoWrapper
    {
        private PrettyPrinter textPrinter = new PrettyPrinter() { MaxMessageLength = 32 };

        public MatchResultDto MatchResult { get; set; }

        public MatchResultDtoWrapper(MatchResultDto matchResult)
        {
            MatchResult = matchResult;
        }

        public override string ToString()
        {
            var codeFragment = textPrinter.Print(MatchResult.MatchedCode);
            return $"\"{codeFragment}\" matched at {MatchResult.LineColumnTextSpan}";
        }
    }
}
