using System.Linq;
using PT.PM.Common;
using System.Collections.Generic;
using System;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching
{
    public class MatchResult : MatchResultBase<PatternRoot>
    {
        public override string PatternKey => Pattern.Key;

        public Ust Ust { get; }

        public override TextFile SourceFile { get; }

        public TextSpan TextSpan => TextSpans.FirstOrDefault();

        private MatchResult(PatternRoot pattern, IEnumerable<TextSpan> textSpans)
        {
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
            TextSpans = textSpans?.ToArray() ?? throw new ArgumentNullException(nameof(textSpans));

            TextSpan lastTextSpan = textSpans.LastOrDefault();
            if (lastTextSpan != default)
            {
                Suppressed = MatchUtils.IsSuppressed(SourceFile, lastTextSpan.GetLineColumnTextSpan(SourceFile));
            }
        }

        public MatchResult(TextFile sourceFile, PatternRoot pattern, IEnumerable<TextSpan> textSpans) : this (pattern, textSpans)
        {
            Ust = null;
            SourceFile = sourceFile ?? throw new ArgumentNullException(nameof(sourceFile));
        }

        public MatchResult(Ust node, PatternRoot pattern, IEnumerable<TextSpan> textSpans) : this (pattern, textSpans)
        {
            Ust = node ?? throw new ArgumentNullException(nameof(node));
            SourceFile = Ust.CurrentSourceFile;
        }

        public override string ToString()
        {
            TextSpan lastTextSpan = TextSpans.Last();
            string patternMatch = SourceFile.GetSubstring(lastTextSpan) + " " + SourceFile.GetLineColumnTextSpan(lastTextSpan);
            return $"{SourceFile}: pattern {Pattern} match: {patternMatch}";
        }
    }
}
