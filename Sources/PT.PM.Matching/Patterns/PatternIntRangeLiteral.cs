using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens.Literals;
using System;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public class PatternIntRangeLiteral : PatternUst, ITerminalPattern
    {
        public long MinValue { get; set; }

        public long MaxValue { get; set; }

        public PatternIntRangeLiteral()
            : this(long.MinValue, long.MaxValue)
        {
        }

        public PatternIntRangeLiteral(long value, TextSpan textSpan = default)
            : this(value, value, textSpan)
        {
        }

        public PatternIntRangeLiteral(long minValue, long maxValue, TextSpan textSpan = default)
            : base(textSpan)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public override bool Any => MinValue == long.MinValue && MaxValue == long.MaxValue;

        public void ParseAndPopulate(string range)
        {
            if (string.IsNullOrEmpty(range))
            {
                MinValue = long.MinValue;
                MaxValue = long.MaxValue;
                return;
            }

            if (range.StartsWith("<(") && range.EndsWith(")>"))
            {
                range = range.Substring(2, range.Length - 4);
            }

            int index = range.IndexOf("..", StringComparison.Ordinal);

            if (index != -1)
            {
                string value = range.Remove(index);
                long minValue;

                if (value == "")
                {
                    minValue = long.MinValue;
                }
                else if (!long.TryParse(value, out minValue))
                {
                    throw new FormatException($"Invalid or too big value {value} while {nameof(PatternIntRangeLiteral)} parsing.");
                }
                MinValue = minValue;

                value = range.Substring(index + 2);
                long maxValue;

                if (value == "")
                {
                    maxValue = long.MaxValue;
                }
                else if (!long.TryParse(value, out maxValue))
                {
                    throw new FormatException($"Invalid or too big value {value} while {nameof(PatternIntRangeLiteral)} parsing.");
                }
                MaxValue = maxValue;
            }
            else
            {
                long value;
                if (!long.TryParse(range, out value))
                {
                    throw new FormatException($"Invalid or too big value {range} while {nameof(PatternIntRangeLiteral)} parsing.");
                }

                MinValue = value;
                MaxValue = value;
            }
        }

        public override string ToString()
        {
            string result;

            if (MinValue == MaxValue)
            {
                result = MinValue.ToString();
            }
            else
            {
                if (MinValue == long.MinValue)
                {
                    result = MaxValue == long.MaxValue ? "" : $"..{MaxValue}";
                }
                else
                {
                    result = MaxValue == long.MaxValue ? $"{MinValue}.." : $"{MinValue}..{MaxValue}";
                }
            }

            return $"<({result})>";
        }

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            if (ust is IntLiteral intLiteral)
            {
                return MinValue <= intLiteral.Value && MaxValue > intLiteral.Value
                    ? context.AddMatch(intLiteral)
                    : context.Fail();
            }
            if (ust is LongLiteral longLiteral)
            {
                return MinValue <= longLiteral.Value && MaxValue > longLiteral.Value
                    ? context.AddMatch(longLiteral)
                    : context.Fail();
            }
            if (ust is BigIntLiteral bigIntLiteral)
            {
                return MinValue <= bigIntLiteral.Value && MaxValue > bigIntLiteral.Value
                    ? context.AddMatch(bigIntLiteral)
                    : context.Fail();
            }

            if (context.UstConstantFolder != null &&
                context.UstConstantFolder.TryGetOrFold(ust, out FoldResult foldingResult))
            {
                context.MatchedWithFolded = true;
                if (foldingResult.Value is long longValue)
                {
                    return MinValue <= longValue && MaxValue > longValue
                        ? context.AddMatches(foldingResult.TextSpans)
                        : context.Fail();
                }
            }

            return context.Fail();
        }
    }
}
