using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public class PatternCommentRegex : PatternUst, IRegexPattern, ITerminalPattern
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

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            var commentLiteral = ust as CommentLiteral;
            if (commentLiteral == null)
            {
                return context.Fail();
            }
            
            IEnumerable<TextSpan> matches = Regex
                .MatchRegex(commentLiteral.Comment)
                .Select(location => location.AddOffset(commentLiteral.TextSpan.Start));

            return matches.Count() > 0
                ? context.AddMatches(matches)
                : context.Fail();
        }
    }
}
