using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Nodes;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using System.Linq;

namespace PT.PM
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

        public MatchingResultDto(MatchingResult result)
            : this(result.SourceCodeFile, result.Nodes.Last().TextSpan, result.Pattern)
        {
        }

        public MatchingResultDto(SourceCodeFile sourceCodeFile, TextSpan textSpan, PatternRootUst pattern)
        {
            MatchedCode = sourceCodeFile.Code.Substring(textSpan.Start, textSpan.Length);
            int beginLine, beginColumn, endLine, endColumn;
            textSpan.ToLineColumn(sourceCodeFile.Code, out beginLine, out beginColumn, out endLine, out endColumn);
            BeginLine = beginLine;
            BeginColumn = beginColumn;
            EndLine = endLine;
            EndColumn = endColumn;
            PatternKey = pattern.Key;
            SourceFile = sourceCodeFile.FullPath;
            
            var startLineIndex = sourceCodeFile.Code.LastIndexOfAny(newLineChars, textSpan.Start) + 1;
            var endLineIndex = sourceCodeFile.Code.IndexOfAny(newLineChars, textSpan.Start + textSpan.Length);
            if (endLineIndex < 0)
            {
                endLineIndex = sourceCodeFile.Code.Length;
            }
            endLineIndex--;
        }

        public static MatchingResultDto CreateFromMatchingResult(MatchingResult matchingResult, ISourceCodeRepository sourceCodeRepository)
        {
            var sourceCodeFile = matchingResult.Nodes.First() is RootUst rootNode
                ? rootNode.SourceCodeFile
                : matchingResult.Nodes.First().Root.SourceCodeFile;
            var result = new MatchingResultDto(
                sourceCodeFile,
                matchingResult.TextSpan,
                matchingResult.Pattern);
            return result;
        }

        public override string ToString()
        {
            return string.Format("{0} (key: {1})", MatchedCode, PatternKey);
        }
    }
}
