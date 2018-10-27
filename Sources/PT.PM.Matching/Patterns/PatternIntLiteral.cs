using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternIntLiteral : PatternUst<Ust>, ITerminalPattern
    {
        public long Value { get; set; }

        public PatternIntLiteral()
        {
        }

        public PatternIntLiteral(long value, TextSpan textSpan = default)
            : base(textSpan)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();

        public override MatchContext Match(Ust ust, MatchContext context)
        {
            if (ust is IntLiteral intLiteral)
            {
                return Value == intLiteral.Value ? context.AddMatch(intLiteral) : context.Fail();
            }

            if (context.UstConstantFolder != null &&
                context.UstConstantFolder.TryFold(ust, out FoldResult foldingResult))
            {
                context.MatchedWithFolded = true;
                if (foldingResult.Value is long longValue)
                {
                    return Value == longValue ? context.AddMatches(foldingResult.TextSpans) : context.Fail();
                }
            }

            return context.Fail();
        }
    }
}
