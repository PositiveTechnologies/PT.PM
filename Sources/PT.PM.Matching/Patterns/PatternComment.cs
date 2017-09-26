using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Linq;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternComment : PatternBase
    {
        private Regex regex;

        public string Comment
        {
            get => regex.ToString();
            set => regex = new Regex(value, RegexOptions.Compiled);
        }

        public PatternComment()
        {
        }

        public PatternComment(string comment, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Comment = comment;
        }

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;

        public override string ToString() => $"</*{Comment}*/>";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext match;

            if (ust is CommentLiteral commentLiteral)
            {
                match = context.AddLocations(regex
                    .MatchRegex(commentLiteral.Comment)
                    .Select(location => location.AddOffset(ust.TextSpan.Start)));
            }
            else
            {
                match = context.Fail();
            }

            return match;
        }
    }
}
