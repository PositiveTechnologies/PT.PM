using System.Linq;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using System.Collections.Generic;
using System;

namespace PT.PM.Matching
{
    public class MatchResult : MatchResultBase<PatternRoot>, IMatchResultBase
    {
        public string PatternKey => Pattern.Key;

        public TextSpan TextSpan => TextSpans.FirstOrDefault();

        public bool Suppressed { get; }

        public MatchResult()
        {
        }

        public MatchResult(CodeFile sourceCodeFile, PatternRoot pattern, IEnumerable<TextSpan> textSpans)
        {
            SourceCodeFile = sourceCodeFile ?? throw new ArgumentNullException(nameof(sourceCodeFile));
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
            TextSpans = textSpans?.ToArray() ?? throw new ArgumentNullException(nameof(textSpans));

            TextSpan lastTextSpan = textSpans.LastOrDefault();
            if (lastTextSpan != default)
            {
                Suppressed = PatternUtils.IsSuppressed(SourceCodeFile,
                    lastTextSpan.GetLineColumnTextSpan(SourceCodeFile));
            }
        }

        public override string ToString()
        {
            return $"Pattern {Pattern} mathched at {(string.Join(", ", TextSpans.Select(textSpan => SourceCodeFile.GetLineColumnTextSpan(textSpan))))}";
        }
    }
}
