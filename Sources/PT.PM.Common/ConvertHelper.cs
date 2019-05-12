using System;
using System.Globalization;
using System.Numerics;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common
{
    public class ConvertHelper : ILoggable
    {
        private readonly RootUst root;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Language Language { get; }

        public ConvertHelper(RootUst root)
        {
            this.root = root ?? throw new ArgumentNullException(nameof(root));
            Language = root.Language;
        }

        public Token ConvertToken(ReadOnlySpan<char> span, TextSpan textSpan)
        {
            int spanLength = span.Length;
            char firstChar = span[0];

            if (spanLength == 1 && firstChar == '*')
            {
                if (firstChar == '*')
                {
                    return new IdToken(span.ToString(), textSpan);
                }

                if (char.IsDigit(firstChar) && TryConvertNumeric(span, textSpan, 10, out Literal numeric))
                {
                    return numeric;
                }
            }

            if (spanLength > 1)
            {
                if (firstChar == '@')
                {
                    return new IdToken(span.Slice(1).ToString(), textSpan);
                }

                if (firstChar == '[')
                {
                    return new IdToken(span.Slice(1, span.Length - 2).ToString(), textSpan);
                }

                if (span[span.Length - 1] == '\'' || span[span.Length - 1] == '"')
                {
                    return ConvertString(textSpan);
                }

                if (char.IsDigit(firstChar))
                {
                    char secondChar = char.ToLowerInvariant(span[1]);

                    int fromBase = char.IsDigit(secondChar)
                        ? 10
                        : secondChar == 'x'
                            ? 16
                            : secondChar == 'o'
                                ? 8
                                : secondChar == 'b'
                                    ? 2
                                    : -1;

                    if (fromBase != -1 && TryConvertNumeric(span, textSpan, fromBase, out Literal numeric))
                    {
                        return numeric;
                    }
                }
            }

            bool id = true;
            for (int i = 0; i < spanLength; i++)
            {
                char c = span[i];
                if ((i == 0 ? !char.IsLetter(c) : !char.IsLetterOrDigit(c)) && c != '_')
                {
                    id = false;
                    break;
                }
            }

            if (id)
            {
                return new IdToken(span.ToString(), textSpan);
            }

            if (TryParseDoubleInvariant(span.ToString(), out double floatValue))
            {
                return new FloatLiteral(floatValue, textSpan);
            }

            // TODO: Maybe return undefined token
            Logger.LogDebug($"Literal cannot be extracted from Token {textSpan} with value {span.ToString()}");
            return null;
        }

        // TODO: implement TryParse methods with Span when netstandard 2.1 comes out
        public bool TryConvertNumeric(ReadOnlySpan<char> value, TextSpan textSpan, int fromBase, out Literal numeric)
        {
            string valueString;

            if (value.Length == 0)
            {
                numeric = null;
                return false;
            }

            char lastChar = char.ToLowerInvariant(value[value.Length - 1]);
            if (lastChar == 'u' || lastChar == 'l')
            {
                value = value.Slice(0, value.Length - 1);

                if (value.Length > 0)
                {
                    lastChar = char.ToLowerInvariant(value[value.Length - 1]);
                    if (lastChar == 'u' || lastChar == 'l')
                    {
                        value = value.Slice(0, value.Length - 1);
                    }
                }

                if (value.Length == 0)
                {
                    numeric = null;
                    return false;
                }
            }

            if (fromBase == 10)
            {
                valueString = value.ToString();
                if (int.TryParse(valueString, out int intValue))
                {
                    numeric = new IntLiteral(intValue, textSpan);
                    return true;
                }

                if (long.TryParse(valueString, out long longValue))
                {
                    numeric = new LongLiteral(longValue, textSpan);
                    return true;
                }

                if (BigInteger.TryParse(valueString, out BigInteger bigValue))
                {
                    numeric = new BigIntLiteral(bigValue, textSpan);
                    return true;
                }

                numeric = null;
                return false;
            }

            if (fromBase == 16 || fromBase == 8 || fromBase == 2)
            {
                char secondChar = fromBase == 16 ? 'x' : fromBase == 8 ? 'o' : 'b';

                value = value.Length > 2 && value[0] == '0' && char.ToLowerInvariant(value[1]) == secondChar
                    ? value.Slice(2)
                    : value;
                valueString = value.ToString();

                try
                {
                    numeric = new IntLiteral(Convert.ToInt32(valueString, fromBase), textSpan);
                    return true;
                }
                catch
                {
                    try
                    {
                        numeric = new LongLiteral(Convert.ToInt64(valueString, fromBase), textSpan);
                        return true;
                    }
                    catch
                    {
                        if (fromBase == 16)
                        {
                            if (BigInteger.TryParse(valueString,
                                NumberStyles.HexNumber, CultureInfo.InvariantCulture, out BigInteger bigValue))
                            {
                                numeric = new BigIntLiteral(bigValue, textSpan);
                                return true;
                            }

                            numeric = null;
                            return false;
                        }

                        var result = new BigInteger();

                        foreach (char c in value)
                        {
                            int nextDigit = c - '0';
                            if (nextDigit < 0 || nextDigit >= fromBase)
                            {
                                numeric = null;
                                return false;
                            }

                            result = result * fromBase + nextDigit;
                        }

                        numeric = new BigIntLiteral(result);
                        return true;
                    }
                }
            }

            throw new NotSupportedException($"{fromBase} base сonversion is not supported");
        }

        public StringLiteral ConvertString(TextSpan textSpan)
        {
            int startInd = textSpan.Start;
            int endInd = textSpan.Start + textSpan.Length - 1;
            string data = root.CurrentSourceFile.Data;
            int escapeCharsLength = 0;

            if (startInd < data.Length)
            {
                char firstChar = char.ToLowerInvariant(data[startInd]);

                if (Language.IsSql() && firstChar == 'n' ||
                    Language == Language.Python && firstChar == 'b')
                {
                    startInd++;
                    escapeCharsLength = 1;
                }
            }

            // check for first quote
            if (startInd < data.Length)
            {
                char firstChar = char.ToLowerInvariant(data[startInd]);
                if (firstChar == '\'' || firstChar == '"')
                {
                    startInd++;
                    escapeCharsLength = 1;
                }
            }

            // check for last quote
            if (endInd < data.Length)
            {
                char lastChar = data[endInd];
                if (lastChar == '\'' || lastChar == '"')
                {
                    endInd--;
                    escapeCharsLength = 1;
                }
            }

            return new StringLiteral(TextSpan.FromBounds(startInd, endInd + 1, textSpan.File), root,
                escapeCharsLength);
        }

        public static bool TryParseDoubleInvariant(string s, out double value)
        {
            return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }
    }
}