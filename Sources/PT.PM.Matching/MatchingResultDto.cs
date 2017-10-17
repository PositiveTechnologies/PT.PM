using PT.PM.Common;
using System.Linq;

namespace PT.PM.Matching
{
    public class MatchingResultDto
    {
        private static char[] newLineChars = new[] { '\n', '\r' };

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
            MatchedCode = string.Join(" ", matchingResult.TextSpans
                .Select(textSpan => code.Substring(textSpan.Start, textSpan.Length)));

            TextSpan lastTextSpan = matchingResult.TextSpans.Union();
            lastTextSpan.ToLineColumn(sourceCodeFile.Code,
                out int beginLine, out int beginColumn,
                out int endLine, out int endColumn);
            BeginLine = beginLine;
            BeginColumn = beginColumn;
            EndLine = endLine;
            EndColumn = endColumn;
            PatternKey = matchingResult.Pattern.Key;
            SourceFile = sourceCodeFile.FullName;
            
            var startLineIndex = sourceCodeFile.Code.LastIndexOfAny(newLineChars, lastTextSpan.Start) + 1;
            var endLineIndex = sourceCodeFile.Code.IndexOfAny(newLineChars, lastTextSpan.Start + lastTextSpan.Length);
            if (endLineIndex < 0)
            {
                endLineIndex = sourceCodeFile.Code.Length;
            }
            endLineIndex--;
        }

        public override string ToString()
        {
            return string.Format("{0} (key: {1})", MatchedCode, PatternKey);
        }
    }
}
