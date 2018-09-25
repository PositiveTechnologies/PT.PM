using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternCommentRegex : PatternUst<CommentLiteral>, IRegexPattern
    {
        public string Default => @".*";

        public string RegexString
        {
            get => Regex.ToString();
            set => Regex = new Regex(string.IsNullOrEmpty(value) ? Default : value, RegexOptions.Compiled);
        }

        public Regex Regex { get; private set; }

        public PatternCommentRegex()
           : this("")
        {
        }

        public PatternCommentRegex(string patternRegex, TextSpan textSpan = default)
            : base(textSpan)
        {
            RegexString = patternRegex;
        }

        public override bool Any => Regex.ToString() == Default;

        public override string ToString() => $"</*{(Any ? "" : Regex.ToString())}*/>";

        public override MatchContext Match(CommentLiteral commentLiteral, MatchContext context)
        {
            // TODO: fix with Memory<string> instead of string
            IEnumerable<TextSpan> matches = Regex
                .MatchRegex(commentLiteral.Comment.ToString())
                .Select(location => location.AddOffset(commentLiteral.TextSpan.Start));

            return matches.Count() > 0
                ? context.AddMatches(matches)
                : context.Fail();
        }
    }
}
