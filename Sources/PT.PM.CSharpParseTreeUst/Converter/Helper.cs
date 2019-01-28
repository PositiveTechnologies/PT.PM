using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private Ust HandleTupleStatement(VariableDeclarationSyntax node)
        {
            List<AssignmentExpression> variables =
                node.Variables.Select(v => (AssignmentExpression)VisitAndReturnNullIfError(v)).ToList();

            var typeNode = (TupleTypeSyntax)node.Type;
            var textSpan = node.GetTextSpan();

            for (int i = 0; i < typeNode.Elements.Count; i++)
            {
                TupleElementSyntax typeElement = typeNode.Elements[i];
                if (typeElement.Identifier == null)
                {
                    continue;
                }

                variables.ForEach(variable =>
                {
                    var tupleTypeElementMemRef = new MemberReferenceExpression
                    {
                        Target = new IdToken(((IdToken)variable.Left).Id, variable.Left.TextSpan),
                        Name = ConvertId(typeElement.Identifier)
                    };

                    var right = (TupleCreateExpression)variable?.Right;
                    if (right == null)
                    {
                        return;
                    }
                    var oldName = ((AssignmentExpression)right.Initializers[i]).Left
                        as MemberReferenceExpression;
                    var newName = CopyMemberReference(oldName);
                    right.Initializers.Add(new AssignmentExpression
                    {
                        Left = tupleTypeElementMemRef,
                        Right = newName,
                        TextSpan = textSpan
                    });
                });
            }
            var declaration = new VariableDeclarationExpression(null, variables, textSpan);
            return new ExpressionStatement(declaration);
        }

        private Ust HandleTupleStatement(VariableDeclaratorSyntax node)
        {
            var tuple = (TupleCreateExpression)base.Visit(node.Initializer.Value);
            var idText = node.Identifier.ValueText;
            var idTextSpan = node.Identifier.GetTextSpan();

            tuple.Initializers.ForEach(init =>
            {
                var assignment = (AssignmentExpression)init;
                assignment.Left = new MemberReferenceExpression(
                    new IdToken(idText, idTextSpan), assignment.Left, assignment.Left.TextSpan);

                if(assignment.Right is MemberReferenceExpression memref  
                && memref.Target == null) // Memref with empty target created in tuple expression visitor
                {
                    memref.Target = new IdToken(idText, idTextSpan);
                }
            });

            return new AssignmentExpression(new IdToken(idText, idTextSpan), tuple, node.GetTextSpan());
        }

        private MemberReferenceExpression CopyMemberReference(MemberReferenceExpression memRef)
        {
            Expression target = null;
            if (memRef.Target is MemberReferenceExpression memRefTarget)
            {
                target = CopyMemberReference(memRefTarget);
            }
            else
            {
                target = new IdToken(((IdToken)memRef.Target).Id, memRef.Target.TextSpan);
            }

            return new MemberReferenceExpression
            {
                Target = target,
                Name = new IdToken(((IdToken)memRef.Name).Id, memRef.Name.TextSpan),
                TextSpan = memRef.TextSpan
            };
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
