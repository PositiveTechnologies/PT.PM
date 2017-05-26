using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Matching;
using PT.PM.Patterns;
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
            : this(result.FileNode.FileName.Text, result.FileNode.FileData, result.Nodes.Last().TextSpan, result.Pattern)
        {
        }

        public MatchingResultDto(string fileName, string fileData, TextSpan textSpan, Pattern pattern)
        {
            MatchedCode = fileData.Substring(textSpan.Start, textSpan.Length);
            int beginLine, beginColumn, endLine, endColumn;
            textSpan.ToLineColumn(fileData, out beginLine, out beginColumn, out endLine, out endColumn);
            BeginLine = beginLine;
            BeginColumn = beginColumn;
            EndLine = endLine;
            EndColumn = endColumn;
            PatternKey = pattern.Key;
            SourceFile = fileName;
            
            var startLineIndex = fileData.LastIndexOfAny(newLineChars, textSpan.Start) + 1;
            var endLineIndex = fileData.IndexOfAny(newLineChars, textSpan.Start + textSpan.Length);
            if (endLineIndex < 0)
            {
                endLineIndex = fileData.Length;
            }
            endLineIndex--;
        }

        public static MatchingResultDto CreateFromMatchingResult(MatchingResult matchingResult, ISourceCodeRepository sourceCodeRepository)
        {
            var fileNode = matchingResult.Nodes.First().FileNode;
            var result = new MatchingResultDto(
                sourceCodeRepository.GetFullPath(fileNode.FileName.Text),
                fileNode.FileData,
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
