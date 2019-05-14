using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public class PatternCommentRegex : PatternUst, IRegexPattern, ITerminalPattern
    {
        [JsonIgnore]
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

        [JsonIgnore]
        public override bool Any => Regex.ToString() == Default;

        public override string ToString() => $"</*{(Any ? "" : Regex.ToString())}*/>";

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            var commentLiteral = ust as Comment;
            if (commentLiteral == null)
            {
                return context.Fail();
            }

            List<TextSpan> matches = Regex.MatchRegex(commentLiteral.CurrentSourceFile, commentLiteral.TextSpan, 0);

            return matches.Count > 0
                ? context.AddMatches(matches)
                : context.Fail();
        }
    }
}
