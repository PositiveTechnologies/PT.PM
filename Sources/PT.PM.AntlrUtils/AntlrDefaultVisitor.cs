using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.AntlrUtils
{
    public class AntlrDefaultVisitor : IParseTreeVisitor<UstNode>, ILoggable
    {
        private static readonly Regex RegexHexLiteral = new Regex(@"^0[xX][a-fA-F0-9]+([uUlL]{0,2})$", RegexOptions.Compiled);
        private static readonly Regex RegexOctalLiteral = new Regex(@"^0[0-7]+([uUlL]{0,2})$", RegexOptions.Compiled);
        private static readonly Regex RegexBinaryLiteral = new Regex(@"^0[bB][01]+([uUlL]{0,2})$", RegexOptions.Compiled);
        private static readonly Regex RegexDecimalLiteral = new Regex(@"^[0-9]+([uUlL]{0,2})$", RegexOptions.Compiled);

        public FileNode FileNode { get; set; }

        public ITokenStream TokenStream { get; set; }

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public AntlrDefaultVisitor(string fileName, string fileData)
        {
            FileNode = new FileNode(fileName, fileData);
        }

        public UstNode Visit(IParseTree tree)
        {
            try
            {
                return tree.Accept(this);
            }
            catch (Exception ex)
            {
                var parserRuleContext = tree as ParserRuleContext;
                if (parserRuleContext != null)
                {
                    AntlrHelper.LogConversionError(ex, parserRuleContext, FileNode.FileName.Text, FileNode.FileData, Logger);
                }
                return DefaultResult;
            }
        }

        public UstNode VisitChildren(IRuleNode node)
        {
            UstNode result;
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
                    var child = Visit(node.GetChild(i));
                    if (child != null)
                    {
                        var childExpression = child as Expression;
                        // Ignore null.
                        if (childExpression != null)
                        {
                            exprs.Add(childExpression);
                        }
                        else
                        {
                            exprs.Add(new WrapperExpression(child));
                        }
                    }
                }
                result = new MultichildExpression(exprs, FileNode);
            }
            return result;
        }

        public virtual UstNode VisitTerminal(ITerminalNode node)
        {
            Token result;
            string nodeText = node.GetText();
            TextSpan textSpan = node.GetTextSpan();
            if ((nodeText.StartsWith("'") && nodeText.EndsWith("'")) ||
                (nodeText.StartsWith("\"") && nodeText.EndsWith("\"")))
            {
                result = new StringLiteral(nodeText.Substring(1, nodeText.Length - 2), textSpan, FileNode);
            }
            else if (nodeText.Contains("."))
            {
                double value;
                double.TryParse(nodeText, out value);
                return new FloatLiteral(value, textSpan, FileNode);
            }
            else if (RegexHexLiteral.IsMatch(nodeText))
            {
                long value;
                nodeText.TryConvertToInt64(16, out value);
                result = new IntLiteral(value, textSpan, FileNode);
            }
            else if (RegexOctalLiteral.IsMatch(nodeText))
            {
                long value;
                nodeText.TryConvertToInt64(8, out value);
                result = new IntLiteral(value, textSpan, FileNode);
            }
            else if (RegexBinaryLiteral.IsMatch(nodeText))
            {
                long value;
                nodeText.Substring(2).TryConvertToInt64(2, out value);
                result = new IntLiteral(value, textSpan, FileNode);
            }
            else if (RegexDecimalLiteral.IsMatch(nodeText))
            {
                long value;
                nodeText.TryConvertToInt64(10, out value);
                result = new IntLiteral(value, textSpan, FileNode);
            }
            else
            {
                result = new IdToken(nodeText, textSpan, FileNode);
            }
            return result;
        }

        public UstNode VisitErrorNode(IErrorNode node)
        {
            return DefaultResult;
        }

        protected UstNode VisitShouldNotBeVisited(IParseTree tree)
        {
            throw new ShouldNotBeVisitedException();
        }

        protected UstNode DefaultResult
        {
            get
            {
                return null;
            }
        }

        protected Expression CreateBinaryOperatorExpression(
            ParserRuleContext left, ITerminalNode operatorTerminal, ParserRuleContext right)
        {
            BinaryOperator binaryOperator = CreateBinaryOperator(operatorTerminal.GetText());

            var expression0 = (Expression)Visit(left);
            var expression1 = (Expression)Visit(right);
            var result = new BinaryOperatorExpression(
                expression0,
                new BinaryOperatorLiteral(binaryOperator, operatorTerminal.GetTextSpan(), FileNode),
                expression1,
                left.GetTextSpan().Union(right.GetTextSpan()), FileNode);

            return result;
        }

        protected virtual BinaryOperator CreateBinaryOperator(string binaryOperatorText)
        {
            return BinaryOperatorLiteral.TextBinaryOperator[binaryOperatorText];
        }
    }
}
