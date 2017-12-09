using PT.PM.Common;
using System.Linq;

namespace PT.PM.Matching
{
    public class MatchResultDto
    {
        public string MatchedCode { get; }

        public TextSpan TextSpan { get; }

        public LineColumnTextSpan LineColumnTextSpan { get; }

        public string PatternKey { get; }

        public string SourceFile { get; }

        public MatchResultDto(MatchResult matchResult)
        {
            SourceCodeFile sourceCodeFile = matchResult.SourceCodeFile;
            string code = sourceCodeFile.Code;
            TextSpan = matchResult.TextSpans.Union();
            LineColumnTextSpan = sourceCodeFile.GetLineColumnTextSpan(TextSpan);

            PatternKey = matchResult.Pattern.Key;
            SourceFile = sourceCodeFile.FullName;
            MatchedCode = code.Substring(TextSpan);
        }

        public override string ToString()
        {
            return string.Format("{0} (key: {1})", MatchedCode, PatternKey);
        }
    }
}
