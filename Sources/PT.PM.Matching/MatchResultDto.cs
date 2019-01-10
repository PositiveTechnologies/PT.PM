using PT.PM.Common;
using System.Linq;
using PT.PM.Common.Files;

namespace PT.PM.Matching
{
    public class MatchResultDto
    {
        public string MatchedCode { get; }

        public TextSpan TextSpan { get; }

        public LineColumnTextSpan LineColumnTextSpan { get; }

        public string PatternKey { get; }

        public string SourceFile { get; }

        public bool Suppressed { get; }

        public MatchResultDto(MatchResult matchResult)
        {
            CodeFile sourceCodeFile = matchResult.SourceCodeFile;
            string code = sourceCodeFile.Data;
            TextSpan = matchResult.TextSpans.Where(textSpan =>
                textSpan.Start >= 0 &&
                textSpan.End <= textSpan.GetCodeFile(sourceCodeFile).Data.Length)
                .Union();
            LineColumnTextSpan = sourceCodeFile.GetLineColumnTextSpan(TextSpan);

            PatternKey = matchResult.Pattern.Key;
            SourceFile = sourceCodeFile.FullName;
            MatchedCode = code.Substring(TextSpan);
            Suppressed = matchResult.Suppressed;
        }

        public override string ToString()
        {
            return string.Format("{0} (key: {1})", MatchedCode, PatternKey);
        }
    }
}
