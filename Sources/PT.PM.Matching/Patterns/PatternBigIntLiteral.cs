using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Numerics;

namespace PT.PM.Matching.Patterns
{
    public class PatternBigIntLiteral : PatternUst, ITerminalPattern
    {
        public BigInteger Value { get; set; }

        public PatternBigIntLiteral()
        {
        }

        public PatternBigIntLiteral(BigInteger value, TextSpan textSpan = default)
            : base(textSpan)
        {
            Value = value;
        }

        public override string ToString() => $"{Value}";

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            switch (ust)
            {
                case BigIntLiteral bigIntLiteral:
                    return Value == bigIntLiteral.Value
                        ? context.AddMatch(ust)
                        : context.Fail();
                case IntLiteral intLiteral:
                    return Value == intLiteral.Value
                        ? context.AddMatch(ust)
                        : context.Fail();
                case LongLiteral longLiteral:
                    return Value == longLiteral.Value
                        ? context.AddMatch(ust)
                        : context.Fail();
            }

            if (context.UstConstantFolder != null &&
                context.UstConstantFolder.TryGetOrFold(ust, out FoldResult foldingResult))
            {
                context.MatchedWithFolded = true;
                if (foldingResult.Value is int intValue)
                {
                    return Value == intValue ? context.AddMatches(foldingResult.TextSpans) : context.Fail();
                }
                if (foldingResult.Value is long longValue)
                {
                    return Value == longValue ? context.AddMatches(foldingResult.TextSpans) : context.Fail();
                }
                if (foldingResult.Value is BigInteger bigIntValue)
                {
                    return Value == bigIntValue ? context.AddMatches(foldingResult.TextSpans) : context.Fail();
                }

            }

            return context.Fail();
        }
    }
}