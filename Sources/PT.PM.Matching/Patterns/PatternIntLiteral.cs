using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternIntLiteral : PatternBase
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

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is IntLiteral intLiteral && intLiteral.Value == Value)
            {
                newContext = context.AddMatch(ust);
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext;
        }
    }
}
