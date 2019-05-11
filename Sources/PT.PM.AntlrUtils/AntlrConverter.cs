using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Globalization;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrConverter : IParseTreeToUstConverter, IParseTreeVisitor<Ust>
    {
        protected RootUst root;

        public abstract Language Language { get; }

        public HashSet<Language> AnalyzedLanguages { get; set; }

        public IList<IToken> Tokens { get; set; }

        public string[] RuleNames { get; set; }

        public ILogger Logger { get; set; } = DummyLogger.Instance;


        public RootUst ParentRoot { get; set; }

        public AntlrConverter()
        {
            AnalyzedLanguages = Language.GetSelfAndSublanguages();
        }

        public RootUst Convert(ParseTree langParseTree)
        {
            var antlrParseTree = (AntlrParseTree)langParseTree;
            ParserRuleContext tree = antlrParseTree.SyntaxTree;
            ICharStream inputStream = tree.start.InputStream ?? tree.stop?.InputStream;

            if (inputStream == null)
            {
                return null;
            }

            RootUst result;
            try
            {
                Tokens = Language.GetSublanguages().Length > 0 ? antlrParseTree.Tokens : new List<IToken>();
                root = new RootUst(langParseTree.SourceFile, Language);
                Ust visited = Visit(tree);
                if (visited is RootUst rootUst)
                {
                    result = rootUst;
                }
                else
                {
                    result = root;
                    result.Node = visited;
                }
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ConversionException(langParseTree.SourceFile, ex));
                return null;
            }

            result.Comments = antlrParseTree.Comments.Select(c => new Comment(c.GetTextSpan())
            {
                Root = result
            })
            .ToArray();

            result.FillAscendants();

            return result;
        }

        public Ust Visit(IParseTree tree)
        {
            try
            {
                return tree?.Accept(this);
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                if (tree is ParserRuleContext parserRuleContext)
                {
                    Logger.LogConversionError(ex, parserRuleContext, root.SourceFile);
                }
                return DefaultResult;
            }
        }

        public Ust VisitChildren(IRuleNode node)
        {
            Ust result;
            if (node.ChildCount == 0)
            {
                result = null;
            }
            else if (node.ChildCount == 1)
            {
                result = Visit(node.GetChild(0));
            }
            else
            {
                var exprs = new List<Expression>();
                for (int i = 0; i < node.ChildCount; i++)
                {
                    Ust child = Visit(node.GetChild(i));
                    if (child != null)
                    {
                        // Ignore null.
                        if (child is Expression childExpression)
                        {
                            exprs.Add(childExpression);
                        }
                        else
                        {
                            exprs.Add(new WrapperExpression(child));
                        }
                    }
                }
                result = new MultichildExpression(exprs);
            }
            return result;
        }

        public virtual Ust VisitTerminal(ITerminalNode node) => ExtractLiteral(node.Symbol);
        public Ust VisitErrorNode(IErrorNode node) => DefaultResult;

        protected Ust VisitShouldNotBeVisited(IParseTree tree)
        {
            string ruleName = "";
            if (tree is ParserRuleContext parserRuleContext)
            {
                ruleName = RuleNames?.ElementAtOrDefault(parserRuleContext.RuleIndex)
                           ?? parserRuleContext.RuleIndex.ToString();
            }

            throw new ShouldNotBeVisitedException(ruleName);
        }

        protected Ust DefaultResult => null;

        protected AssignmentExpression CreateAssignExpr(Expression left, Expression right, ParserRuleContext context, ParserRuleContext assignOperator)
        {
            return UstUtils.CreateAssignExpr(left, right, context.GetTextSpan(), assignOperator?.GetText(), assignOperator?.GetTextSpan() ?? TextSpan.Zero);
        }

        protected Expression CreateBinaryOperatorExpression(
            ParserRuleContext left, IToken operatorTerminal, ParserRuleContext right)
        {
            return CreateBinaryOperatorExpression(left, operatorTerminal.Text, operatorTerminal.GetTextSpan(), right);
        }

        protected Expression CreateBinaryOperatorExpression(
            ParserRuleContext left, ITerminalNode operatorTerminal, ParserRuleContext right)
        {
            return CreateBinaryOperatorExpression(left, operatorTerminal.GetText(), operatorTerminal.GetTextSpan(),  right);
        }

        protected Expression CreateBinaryOperatorExpression(ParserRuleContext left, string operatorText, TextSpan operatorTextSpan, ParserRuleContext right)
        {
            BinaryOperator binaryOperator = BinaryOperatorLiteral.TextBinaryOperator[operatorText.ToLowerInvariant()];

            var expression0 = (Expression)Visit(left);
            var expression1 = (Expression)Visit(right);
            var result = new BinaryOperatorExpression(
                expression0,
                new BinaryOperatorLiteral(binaryOperator, operatorTextSpan),
                expression1,
                left.GetTextSpan().Union(right.GetTextSpan()));

            return result;
        }

        protected Expression CreateUnaryOperatorExpression(ParserRuleContext operand, IToken operatorTerminal, bool prefix = true)
        {
            UnaryOperator op = prefix
                ? UnaryOperatorLiteral.PrefixTextUnaryOperator[operatorTerminal.Text]
                : UnaryOperatorLiteral.PostfixTextUnaryOperator[operatorTerminal.Text];
            var result = new UnaryOperatorExpression
            {
                Operator = new UnaryOperatorLiteral(op, operatorTerminal.GetTextSpan()),
                Expression = Visit(operand).ToExpressionIfRequired()
            };
            result.TextSpan = result.Expression.TextSpan.Union(result.Operator.TextSpan);
            return result;
        }

        protected Token ExtractLiteral(IToken token)
        {
            ReadOnlySpan<char> span = ExtractSpan(token, out TextSpan textSpan);

            int spanLength = span.Length;
            char firstChar = span[0];

            if (spanLength == 1 && firstChar == '*')
            {
                if (firstChar == '*')
                {
                    return new IdToken(span.ToString(), textSpan);
                }

                if (char.IsDigit(firstChar) && TryParseNumeric(span, textSpan, 10, out Literal numeric))
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
                    int startIndex = 0;

                    char firstCharLowered = char.ToLowerInvariant(firstChar);
                    if (Language.IsSql() && firstCharLowered == 'n' ||
                        Language == Language.Python && firstChar == 'b')
                    {
                        startIndex++;
                    }

                    if (span[startIndex] == '\'' || span[startIndex] == '"')
                    {
                        startIndex++;
                        return new StringLiteral(TextSpan.FromBounds(textSpan.Start + startIndex, textSpan.End - 1, textSpan.File),
                            root);
                    }
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

                    if (fromBase != -1 && TryParseNumeric(span, textSpan, fromBase, out Literal numeric))
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

        protected static ReadOnlySpan<char> ExtractSpan(IToken token, out TextSpan textSpan)
        {
            ReadOnlySpan<char> span;

            if (token is LightToken lightToken)
            {
                textSpan = lightToken.TextSpan;
                span = lightToken.Span;
            }
            else
            {
                textSpan = token.GetTextSpan();
                span = token.Text.AsSpan();
            }

            return span;
        }

        // TODO: implement TryParse methods with Span when netstandard 2.1 comes out
        protected static bool TryParseNumeric(ReadOnlySpan<char> value, TextSpan textSpan, int fromBase, out Literal numeric)
        {
            string valueString;

            char lastChar = char.ToLowerInvariant(value[value.Length - 1]);
            if (lastChar == 'u' || lastChar == 'l')
            {
                value = value.Slice(value.Length - 1);
            }

            lastChar = char.ToLowerInvariant(value[value.Length - 1]);
            if (lastChar == 'u' || lastChar == 'l')
            {
                value = value.Slice(value.Length - 1);
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
                    numeric = new IntLiteral(System.Convert.ToInt32(valueString, fromBase), textSpan);
                    return true;
                }
                catch
                {
                    try
                    {
                        numeric = new LongLiteral(System.Convert.ToInt64(valueString, fromBase), textSpan);
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

        protected bool TryParseDoubleInvariant(string s, out double value)
        {
            return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }
    }
}
