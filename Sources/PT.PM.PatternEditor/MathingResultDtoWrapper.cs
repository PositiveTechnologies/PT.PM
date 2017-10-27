using PT.PM.Common;
using PT.PM.Matching;

namespace PT.PM.PatternEditor
{
    public class MathingResultDtoWrapper
    {
        private TextTruncater textTruncater = new TextTruncater() { MaxMessageLength = 32 };

        public MatchingResultDto MatchingResult { get; set; }

        public MathingResultDtoWrapper(MatchingResultDto matchingResult)
        {
            MatchingResult = matchingResult;
        }

        public override string ToString()
        {
            var codeFragment = textTruncater.Trunc(MatchingResult.MatchedCode);
            return $"\"{codeFragment}\" matched at ({MatchingResult.BeginLine};{MatchingResult.BeginColumn})-({MatchingResult.EndLine};{MatchingResult.EndColumn})";
        }
    }
}
