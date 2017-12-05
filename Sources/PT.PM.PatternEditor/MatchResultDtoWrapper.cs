using PT.PM.Common;
using PT.PM.Matching;

namespace PT.PM.PatternEditor
{
    public class MatchResultDtoWrapper
    {
        private PrettyPrinter textPrinter = new PrettyPrinter() { MaxMessageLength = 32 };

        public MatchResultDto MatchingResult { get; set; }

        public MatchResultDtoWrapper(MatchResultDto matchResult)
        {
            MatchingResult = matchResult;
        }

        public override string ToString()
        {
            var codeFragment = textPrinter.Print(MatchingResult.MatchedCode);
            return $"\"{codeFragment}\" matched at ({MatchingResult.BeginLine};{MatchingResult.BeginColumn})-({MatchingResult.EndLine};{MatchingResult.EndColumn})";
        }
    }
}
