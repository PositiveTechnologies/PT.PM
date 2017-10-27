using PT.PM.Common;
using System.Linq;

namespace PT.PM.Matching
{
    public class MatchingResultDto
    {
        public string MatchedCode { get; set; }

        public int BeginLine { get; set; }

        public int BeginColumn { get; set; }

        public int EndLine { get; set; }

        public int EndColumn { get; set; }

        public string PatternKey { get; set; }

        public string SourceFile { get; set; }

        public MatchingResultDto()
        {
        }

        public MatchingResultDto(MatchingResult matchingResult)
        {
            SourceCodeFile sourceCodeFile = matchingResult.SourceCodeFile;
            string code = sourceCodeFile.Code;
            TextSpan textSpan = matchingResult.TextSpans.Union();
            textSpan.ToLineColumn(sourceCodeFile.Code,
                out int beginLine, out int beginColumn,
                out int endLine, out int endColumn);

            BeginLine = beginLine;
            BeginColumn = beginColumn;
            EndLine = endLine;
            EndColumn = endColumn;
            PatternKey = matchingResult.Pattern.Key;
            SourceFile = sourceCodeFile.FullName;
            MatchedCode = code.Substring(textSpan);
        }

        public override string ToString()
        {
            return string.Format("{0} (key: {1})", MatchedCode, PatternKey);
        }
    }
}
