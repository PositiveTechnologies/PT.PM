using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternCommentRegex : PatternUst<CommentLiteral>
    {
        public Regex CommentRegex { get; set; }

        public PatternCommentRegex()
            : this("")
        {
        }

        public PatternCommentRegex(string comment, TextSpan textSpan = default(TextSpan))
            : this(new Regex(string.IsNullOrEmpty(comment) ? ".*" : comment, RegexOptions.Compiled), textSpan)
        {
        }

        public PatternCommentRegex(Regex commentRegex, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            CommentRegex = commentRegex;
        }

        public override string ToString() => $"</*{CommentRegex}*/>";

        public override MatchingContext Match(CommentLiteral commentLiteral, MatchingContext context)
        {
            MatchingContext newContext;

            IEnumerable<TextSpan> matches = CommentRegex
                .MatchRegex(commentLiteral.Comment)
                .Select(location => location.AddOffset(commentLiteral.TextSpan.Start));
            if (matches.Count() > 0)
            {
                newContext = context.AddMatches(matches);
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext;
        }
    }
}
