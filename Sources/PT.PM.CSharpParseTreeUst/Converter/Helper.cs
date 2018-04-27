using System;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using Microsoft.CodeAnalysis;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Threading;

namespace PT.PM.CSharpParseTreeUst.RoslynUstVisitor
{
    public partial class CSharpRoslynParseTreeConverter
    {
        protected IdToken ConvertId(SyntaxToken node)
        {
            return new IdToken(node.ValueText, node.GetTextSpan());
        }

        protected ModifierLiteral ConvertModifier(SyntaxToken token)
        {
            Modifier modifier;
            Enum.TryParse(token.ValueText, true, out modifier);
            return new ModifierLiteral(modifier, token.GetTextSpan());
        }

        protected TypeToken ConvertType(Ust node)
        {
            var typeToken = node as TypeToken;
            if (typeToken != null)
                return typeToken;

            var idToken = node as IdToken;
            if (idToken != null)
                return new TypeToken(idToken.Id, idToken.TextSpan);

            return null;
        }

        protected Ust VisitAndReturnNullIfError(SyntaxNode node)
        {
            try
            {
                return base.Visit(node);
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ConversionException(root?.SourceCodeFile, message: ex.Message)
                {
                    TextSpan = node.GetTextSpan()
                });
                return null;
            }
        }
    }
}
