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
        protected ConvertHelper convertHelper;

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
                convertHelper = new ConvertHelper(root) {Logger = Logger};
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

            result.Comments = antlrParseTree.Comments.Select(c => new Comment(c.GetTextSpan(), result)
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

        public virtual Ust VisitTerminal(ITerminalNode node)
        {
            ReadOnlySpan<char> span = node.Symbol.ExtractSpan(out TextSpan textSpan);
            return convertHelper.ConvertToken(span, textSpan);
        }

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

        protected Ust ConvertToken(IToken token)
        {
            ReadOnlySpan<char> span = token.ExtractSpan(out TextSpan textSpan);
            return convertHelper.ConvertToken(span, textSpan);
        }

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
                Expression = Visit(operand).AsExpression()
            };
            result.TextSpan = result.Expression.TextSpan.Union(result.Operator.TextSpan);
            return result;
        }
    }
}
