using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.CSharpAstConversion.RoslynAstVisitor;
using AspxParser;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.CSharpAstConversion
{
    public class AspxToCsConverter : DepthFirstAspxWithoutCloseTagVisitor<UstNode>
    {
        private Stack<bool> runAtServer = new Stack<bool>();
        private readonly FileNode fileNode;
        private int NamespaceDepth;

        public AspxToCsConverter(string fileName, string fileData)
        {
            fileNode = new FileNode(fileName, fileData);
            NamespaceDepth = 0;
            runAtServer.Push(false);
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

            fileNode.Root = members.CreateRootNamespace(Language.Aspx, fileNode);
            return fileNode;
        }

        public override UstNode Visit(AspxNode.HtmlTag node)
        {
            runAtServer.Push(node.Attributes.IsRunAtServer);
            NamespaceDepth++;

            var members = new List<UstNode>();
            foreach (var child in node.Children)
            {
                UstNode accepted = child.Accept(this);
                if (accepted != null)
                {
                    members.Add(accepted);
                }
            }
            var result = new NamespaceDeclaration(new StringLiteral($"aspx{NamespaceDepth}"), members, Language.Aspx, node.Location.GetTextSpan(), fileNode);

            NamespaceDepth--;
            runAtServer.Pop();

            return result;
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

        private UstNode ParseAndConvert(string code, AspxParser.Location location)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(kind: SourceCodeKind.Script));
            var converter = new RoslynAstCommonConverterVisitor(tree, fileNode);
            FileNode resultFileNode = converter.Walk();
            UstNode result = NodeHelpers.CreateLanguageNamespace(resultFileNode.Root, Language.Aspx, fileNode);
            result.ApplyActionToDescendants(uAstNode => uAstNode.TextSpan = uAstNode.TextSpan.AddOffset(location.Start));
            return result;
        }

        private UstNode ParseExpressionAndConvert(string expression, int offset)
        {
            ExpressionSyntax node = SyntaxFactory.ParseExpression(expression);
            var converter = new RoslynAstCommonConverterVisitor(node, fileNode);
            UstNode result = converter.Walk(node);
            if (result is FileNode)
            {
                result = ((FileNode)result).Root;
            }
            result.ApplyActionToDescendants(uAstNode => uAstNode.TextSpan = uAstNode.TextSpan.AddOffset(offset));
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
