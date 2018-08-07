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

        public CodeFile SourceCodeFile => RootUst.SourceCodeFile;

        public TextSpan TextSpan => TextSpans.FirstOrDefault();

        public bool Suppressed { get; }

        public MatchResult()
        {
        }

        public MatchResult(RootUst rootUst, PatternRoot pattern, IEnumerable<TextSpan> textSpans)
        {
            RootUst = rootUst ?? throw new ArgumentNullException(nameof(rootUst));
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
            TextSpans = textSpans?.ToArray() ?? throw new ArgumentNullException(nameof(textSpans));

            TextSpan lastTextSpan = textSpans.LastOrDefault();
            if (lastTextSpan != default)
            {
                Suppressed = PatternUtils.IsSuppressed(rootUst.SourceCodeFile,
                    lastTextSpan.GetLineColumnTextSpan(rootUst.SourceCodeFile));
            }
        }

        public override string ToString()
        {
            return $"Pattern {Pattern} mathched at {(string.Join(", ", TextSpans.Select(textSpan => SourceCodeFile.GetLineColumnTextSpan(textSpan))))}";
        }
    }
}
