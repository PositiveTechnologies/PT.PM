using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Linq;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternCommentRegex : PatternBase
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

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;

        public override string ToString() => $"</*{CommentRegex}*/>";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is CommentLiteral commentLiteral)
            {
                newContext = context.AddMatches(CommentRegex
                    .MatchRegex(commentLiteral.Comment)
                    .Select(location => location.AddOffset(ust.TextSpan.Start)));
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext;
        }
    }
}
