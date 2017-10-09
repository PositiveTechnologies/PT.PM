using PT.PM.Matching;

namespace PT.PM.PatternEditor
{
    public class MathingResultDtoWrapper
    {
        public MatchingResultDto MatchingResult { get; set; }

        public MathingResultDtoWrapper(MatchingResultDto matchingResult)
        {
            MatchingResult = matchingResult;
        }

        public override string ToString()
        {
            string codeFragment = MatchingResult.MatchedCode.Length > 32 ? MatchingResult.MatchedCode.Substring(0, 32) + "..." : MatchingResult.MatchedCode;
            return $"\"{codeFragment}\" matched at ({MatchingResult.BeginLine};{MatchingResult.BeginColumn})-({MatchingResult.EndLine};{MatchingResult.EndColumn})";
        }
    }
}
