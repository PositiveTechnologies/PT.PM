using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternIntLiteral : PatternUst<IntLiteral>
    {
        public long Value { get; set; }

        public PatternIntLiteral()
        {
        }

        public PatternIntLiteral(long value, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();

        public override MatchingContext Match(IntLiteral intLiteral, MatchingContext context)
        {
            MatchingContext newContext;

            if (intLiteral.Value == Value)
            {
                newContext = context.AddMatch(intLiteral);
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext;
        }
    }
}
