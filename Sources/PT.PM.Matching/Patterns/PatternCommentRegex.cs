using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternCommentRegex : PatternUst
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

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is CommentLiteral commentLiteral)
            {
                IEnumerable<TextSpan> matches = CommentRegex
                    .MatchRegex(commentLiteral.Comment)
                    .Select(location => location.AddOffset(ust.TextSpan.Start));
                if (matches.Count() > 0)
                {
                    newContext = context.AddMatches(matches);
                }
                else
                {
                    newContext = context.Fail();
                }
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext;
        }
    }
}
