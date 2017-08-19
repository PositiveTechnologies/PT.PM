using System;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using Microsoft.CodeAnalysis;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.CSharpParseTreeUst.RoslynUstVisitor
{
    public partial class RoslynUstCommonConverterVisitor
    {
        protected IdToken ConvertId(SyntaxToken node)
        {
            string name = node.ValueText;
            return new IdToken(name, node.GetTextSpan(), FileNode);
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
    }
}
