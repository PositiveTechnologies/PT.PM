using System;
using System.Linq;
using Antlr4.Runtime.Tree;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.AntlrUtils
{
    public class AntlrConvertHelper : ConvertHelper
    {
        private AntlrParser AntlrParser { get; }

        public AntlrConvertHelper(RootUst root, AntlrParser antlrParser)
            : base(root)
        {
            AntlrParser = antlrParser ?? throw new ArgumentNullException(nameof(antlrParser));
        }

        public Ust Convert(ITerminalNode node)
        {
            ReadOnlySpan<char> span = node.Symbol.ExtractSpan(out TextSpan textSpan);
            int tokenType = node.Symbol.Type;

            if (AntlrParser.IdentifierTokenTypes.Contains(tokenType) ||
                AntlrParser.IdentifierRuleType.Contains(node.Parent.GetType()))
            {
                return new IdToken(span.ToString(), textSpan);
            }

            if (AntlrParser.StringTokenTypes.Contains(tokenType))
            {
                return ConvertString(textSpan);
            }

            if (tokenType == AntlrParser.NullTokenType)
            {
                return new NullLiteral(textSpan);
            }

            if (tokenType == AntlrParser.TrueTokenType || tokenType == AntlrParser.FalseTokenType)
            {
                return new BooleanLiteral(bool.Parse(span.ToString()), textSpan); // TODO: split values in grammars
            }

            if (tokenType == AntlrParser.ThisTokenType)
            {
                return new ThisReferenceToken(textSpan);
            }

            int fromBase = tokenType == AntlrParser.DecNumberTokenType
                ? 10
                : tokenType == AntlrParser.HexNumberTokenType
                    ? 16
                    : tokenType == AntlrParser.OctNumberTokenType
                        ? 8
                        : tokenType == AntlrParser.BinNumberTokenType
                            ? 2
                            : -1;

            if (fromBase != -1)
            {
                TryConvertNumeric(span, textSpan, fromBase, out Literal result);
                return result;
            }

            if (tokenType == AntlrParser.FloatNumberTokenType)
            {
                TryParseDoubleInvariant(span.ToString(), out double value);
                return new FloatLiteral(value, textSpan);
            }

            return ConvertToken(span, textSpan, false);
        }
    }
}