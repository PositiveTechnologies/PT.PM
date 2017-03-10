using System;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using Microsoft.CodeAnalysis;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes.Expressions;
//using Microsoft.CodeAnalysis.FindSymbols;
using PT.PM.Common.Symbols;

namespace PT.PM.CSharpParseTreeUst.RoslynUstVisitor
{
    public partial class RoslynUstCommonConverterVisitor
    {
        protected IdToken ConvertId(SyntaxToken node)
        {
            string name = node.ValueText;
            TypeSymbol typeSymbol = null;

            /*if (SemanticModel != null)
            {
                var symbolInfo = SemanticModel.GetSymbolInfo(node.Parent);
                var symbol2 = SymbolFinder.FindSymbolAtPositionAsync(SemanticModel, node.Span.Start, new AdhocWorkspace()).Result;
                var b = symbol2;
                var asdf = b.ToString();

                var typeInfo = SemanticModel.GetTypeInfo(node.Parent);

                if (symbolInfo.Symbol != null)
                {
                    name = symbolInfo.Symbol.ToString();
                }
                var a = SemanticModel.GetTypeInfo(node.Parent);
                if (typeInfo.Type != null)
                {
                    typeSymbol = new TypeSymbol(typeInfo.Type.ToString());
                }
            }*/

            return new IdToken(name, node.GetTextSpan(), FileNode) { ReturnType = typeSymbol };
        }

        protected ModifierLiteral ConvertModifier(SyntaxToken token)
        {
            Modifier modifier;
            Enum.TryParse(token.ValueText, true, out modifier);
            return new ModifierLiteral(modifier, token.GetTextSpan(), FileNode);
        }

        protected TypeToken ConvertType(UstNode node)
        {
            var typeToken = node as TypeToken;
            if (typeToken != null)
                return typeToken;

            var idToken = node as IdToken;
            if (idToken != null)
                return new TypeToken(idToken.Id, idToken.TextSpan, FileNode);

            return null;
        }

        protected UstNode VisitAndReturnNullIfError(SyntaxNode node)
        {
            try
            {
                return base.Visit(node);
            }
            catch (Exception ex)
            {
                Logger.LogError(new ConversionException(ex.Message)
                {
                    FileName = FileNode?.FileName?.Text ?? "",
                    TextSpan = node.GetTextSpan()
                });
                return null;
            }
        }

        /*protected void Resolve(SyntaxToken node, Expression expression)
        {
            if (SemanticModel != null)
            {
                var symbolInfo = SemanticModel.GetSymbolInfo(node.Parent);
                if (symbolInfo.Symbol != null)
                {
                    name = symbolInfo.Symbol.ToString();
                }
                if (expression.ReturnType == null)
                {
                    var typeInfo = SemanticModel.GetTypeInfo(node.Parent);
                    if (typeInfo.Type != null)
                    {
                        expression.ReturnType = new TypeSymbol() { Name = typeInfo.Type.ToString() };
                    }
                }
            }
        }*/

        protected void ResolveType(SyntaxNode node, TypeToken type)
        {
            if (SemanticModel != null)
            {
                var symbolInfo = SemanticModel.GetSymbolInfo(node);
                if (symbolInfo.Symbol != null)
                {
                    type.TypeText = symbolInfo.Symbol.ContainingNamespace + "." + symbolInfo.Symbol.Name;
                    type.ReturnType = new TypeSymbol(type.TypeText);
                }
            }
        }
    }
}
