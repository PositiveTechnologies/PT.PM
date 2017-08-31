using AspxParser;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.CSharpParseTreeUst.RoslynUstVisitor;
using System.Collections.Generic;
using System;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Collections;

namespace PT.PM.CSharpParseTreeUst
{
    public class AspxConverter : DepthFirstAspxWithoutCloseTagVisitor<UstNode>, IParseTreeToUstConverter
    {
        private Stack<bool> runAtServer = new Stack<bool>();
        private SourceCodeFile sourceCodeFile;
        private int namespaceDepth;

        public Language Language => Language.Aspx;

        public HashSet<Language> AnalyzedLanguages { get; set; }

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public RootNode ParentRoot { get; set; }

        public AspxConverter()
        {
            AnalyzedLanguages = Language.GetSelfAndSublanguages();
            
            namespaceDepth = 0;
            runAtServer.Push(false);
        }

        public RootNode Convert(ParseTree langParseTree)
        {
            RootNode result = null;

            var aspxParseTree = (AspxParseTree)langParseTree;
            try
            {
                sourceCodeFile = langParseTree.SourceCodeFile;
                UstNode visited = aspxParseTree.Root.Accept(this);
                if (visited is RootNode rootUstNode)
                {
                    result = rootUstNode;
                }
                else
                {
                    result = new RootNode(sourceCodeFile, Language);
                    result.Node = visited;
                }
                result.FillAscendants();
            }
            catch (Exception ex)
            {
                Logger.LogError(new ConversionException(aspxParseTree.SourceCodeFile.FullPath, ex));
                result = new RootNode(langParseTree.SourceCodeFile, Language);
            }

            return result;
        }

        public override UstNode Visit(AspxNode.Root node)
        {
            var members = new List<UstNode>();
            foreach (var child in node.Children)
            {
                var accepted = child.Accept(this);
                if (accepted != null)
                {
                    members.Add(accepted);
                }
            }

            if (members.Count == 1)
            {
                return members[0];
            }

            return new Collection(members);
        }

        public override UstNode Visit(AspxNode.HtmlTag node)
        {
            runAtServer.Push(node.Attributes.IsRunAtServer);
            namespaceDepth++;

            var members = new List<UstNode>();
            foreach (var child in node.Children)
            {
                UstNode accepted = child.Accept(this);
                if (accepted != null)
                {
                    members.Add(accepted);
                }
            }

            namespaceDepth--;
            runAtServer.Pop();

            if (members.Count == 1)
            {
                return members[0];
            }

            return new Collection(members);
        }

        public override UstNode Visit(AspxNode.AspxTag node)
        {
            runAtServer.Push(node.Attributes.IsRunAtServer);
            var result = VisitChildren(node);
            runAtServer.Pop();
            return result;
        }

        public override UstNode Visit(AspxNode.AspxDirective node)
        {
            // TODO: implement AspxDirective processing.
            runAtServer.Push(node.Attributes.IsRunAtServer);
            var result = VisitChildren(node);
            runAtServer.Pop();
            return result;
        }

        public override UstNode Visit(AspxNode.DataBinding node)
        {
            return VisitChildren(node);
        }

        public override UstNode Visit(AspxNode.CodeRender node)
        {
            return ParseAndConvert(node.Expression, node.Location);
        }

        public override UstNode Visit(AspxNode.CodeRenderExpression node)
        {
            string wrappedExpression = ResponseWriteWrappedString(node.Expression);
            return ParseExpressionAndConvert(wrappedExpression, node.Location.Start);
        }

        public override UstNode Visit(AspxNode.CodeRenderEncode node)
        {
            string wrappedExpression = HtmlEncodeWrappedString(node.Expression);
            return ParseExpressionAndConvert(wrappedExpression, node.Location.Start);
        }

        public override UstNode Visit(AspxNode.Literal node)
        {
            UstNode result = null;
            if (!string.IsNullOrWhiteSpace(node.Text) && runAtServer.Peek())
            {
                result = ParseAndConvert(node.Text, node.Location);
            }
            return result;
        }

        private UstNode ParseAndConvert(string code, global::AspxParser.Location location)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(kind: SourceCodeKind.Script));
            var converter = new CSharpRoslynParseTreeConverter();
            RootNode result = converter.Convert(new CSharpRoslynParseTree(tree) { SourceCodeFile = sourceCodeFile });
            result.ApplyActionToDescendants(ustNode => ustNode.TextSpan = ustNode.TextSpan.AddOffset(location.Start));
            return result;
        }

        private UstNode ParseExpressionAndConvert(string expression, int offset)
        {
            ExpressionSyntax node = SyntaxFactory.ParseExpression(expression);
            var converter = new CSharpRoslynParseTreeConverter();
            UstNode result = converter.Visit(node);
            RootNode resultRoot =
                result as RootNode ?? new RootNode(sourceCodeFile, Language.CSharp) { Node = result };
            resultRoot.SourceCodeFile = sourceCodeFile;
            result.ApplyActionToDescendants(ustNode => ustNode.TextSpan = ustNode.TextSpan.AddOffset(offset));
            return result;
        }

        private static string ResponseWriteWrappedString(string expression)
        {
            return $"Response.Write({expression})";
        }

        private static string HtmlEncodeWrappedString(string expression)
        {
            return $"System.Net.WebUtility.HtmlEncode({expression})";
        }
    }
}
