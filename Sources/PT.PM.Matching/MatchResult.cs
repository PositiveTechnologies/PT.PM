using System.Linq;
using PT.PM.Common;
using System.Collections.Generic;
using System;
using PT.PM.Common.Files;

namespace PT.PM.Matching
{
    public class MatchResult : MatchResultBase<PatternRoot>
    {
        public override string PatternKey => Pattern.Key;

        public override TextFile SourceFile { get; }

        public TextSpan TextSpan => TextSpans.FirstOrDefault();

        public MatchResult(TextFile sourceFile, PatternRoot pattern, IEnumerable<TextSpan> textSpans)
        {
            SourceFile = sourceFile ?? throw new ArgumentNullException(nameof(sourceFile));
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
            TextSpans = textSpans?.ToArray() ?? throw new ArgumentNullException(nameof(textSpans));

            TextSpan lastTextSpan = textSpans.LastOrDefault();
            if (lastTextSpan != default)
            {
                Suppressed = PatternUtils.IsSuppressed(SourceFile, lastTextSpan.GetLineColumnTextSpan(SourceFile));
            }
        }

        public override string ToString()
        {
            TextSpan lastTextSpan = TextSpans.Last();
            string patternMatch = SourceFile.GetSubstring(lastTextSpan) + " " + SourceFile.GetLineColumnTextSpan(lastTextSpan);
            return $"{SourceFile}: pattern {Pattern} match: {patternMatch}";
        }
    }
}
