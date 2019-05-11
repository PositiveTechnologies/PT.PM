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
using System.Text.RegularExpressions;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Linq;
using System.Threading;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrConverter : IParseTreeToUstConverter, IParseTreeVisitor<Ust>
    {
        private static readonly Regex RegexHexLiteral = new Regex(@"^0[xX]([a-fA-F0-9]+)([uUlL]{0,2})$", RegexOptions.Compiled);
        private static readonly Regex RegexOctalLiteral = new Regex(@"^0([0-7]+)([uUlL]{0,2})$", RegexOptions.Compiled);
        private static readonly Regex RegexBinaryLiteral = new Regex(@"^0[bB]([01]+)([uUlL]{0,2})$", RegexOptions.Compiled);
        private static readonly Regex RegexDecimalLiteral = new Regex(@"^([0-9]+)([uUlL]{0,2})$", RegexOptions.Compiled);

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

        public Ust VisitList(IList<IParseTree> nodes)
        {
            var exprs = new List<Expression>();
            for (int i = 0; i < nodes.Count; i++)
            {
                var child = Visit(nodes[i]);
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
            var result = new MultichildExpression(exprs);
            return result;
        }

        public virtual Ust VisitTerminal(ITerminalNode node)
        {
            string nodeText = node.GetText();
            TextSpan textSpan = node.GetTextSpan();
            if (nodeText.StartsWith("'") && nodeText.EndsWith("'") ||
                nodeText.StartsWith("\"") && nodeText.EndsWith("\""))
            {
                return TextUtils.GetStringLiteralWithoutQuotes(textSpan, root);
            }

            if (nodeText.Contains("."))
            {
                double.TryParse(nodeText, out double value);
                return new FloatLiteral(value, textSpan);
            }

            var integerToken = TryParseInteger(nodeText, textSpan);
            if (integerToken != null)
            {
                return integerToken;
            }

            return new IdToken(nodeText, textSpan);
        }

        public Ust VisitErrorNode(IErrorNode node)
        {
            return DefaultResult;
        }

        protected Token TryParseInteger(string text, TextSpan textSpan)
        {
            Match match = RegexHexLiteral.Match(text);
            if (match.Success)
            {
                return TextUtils.TryCreateNumericLiteral(match.Groups[1].Value, textSpan, 16);
            }
            match = RegexOctalLiteral.Match(text);
            if (match.Success)
            {
                return TextUtils.TryCreateNumericLiteral(match.Groups[1].Value, textSpan, 8);
            }
            match = RegexBinaryLiteral.Match(text);
            if (match.Success)
            {
                return TextUtils.TryCreateNumericLiteral(match.Groups[1].Value, textSpan, 2);
            }
            match = RegexDecimalLiteral.Match(text);
            if (match.Success)
            {
                return TextUtils.TryCreateNumericLiteral(match.Groups[1].Value, textSpan);
            }
            return null;
        }

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
            string text = token.Text;
            TextSpan textSpan = token.GetTextSpan();
            Token result = null;

            try
            {
                if (text == "*")
                {
                    result = new IdToken(text, textSpan);
                }
                else if (text.StartsWith("@"))
                {
                    result = new IdToken(text.Substring(1), textSpan);
                }
                else if ((text.StartsWith("\"") || text.StartsWith("["))
                    && text.Length > 1)
                {
                    result = new IdToken(text.Substring(1, text.Length - 2), textSpan);
                }
                else if (text.EndsWith("'"))
                {
                    int startIndex = textSpan.Start + 1;
                    if (text.StartsWith("N"))
                    {
                        startIndex++;
                    }
                    result = new StringLiteral(TextSpan.FromBounds(startIndex, textSpan.End - 1, textSpan.File), root);
                }
                else if (text.All(char.IsDigit))
                {
                    result = TextUtils.TryCreateNumericLiteral(text, textSpan);
                }
                else if (text.StartsWith("0X", StringComparison.OrdinalIgnoreCase))
                {
                    result = TextUtils.TryCreateNumericLiteral(text, textSpan, 16);
                }
                else if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double floatValue))
                {
                    result = new FloatLiteral(floatValue, textSpan);
                }
            }
            catch
            {
                Logger.LogDebug($"Literal cannot be extracted from {nameof(token)} with symbol {text}");
            }

            if (result == null && text.Any(c => char.IsLetterOrDigit(c) || c == '_'))
            {
                result = new IdToken(text, textSpan);
            }

            return result;
        }
    }
}
