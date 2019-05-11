using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PT.PM.Common;
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
            Enum.TryParse(token.ValueText, true, out Modifier modifier);
            return new ModifierLiteral(modifier, token.GetTextSpan());
        }

        protected TypeToken ConvertType(Ust node)
        {
            if (node is TypeToken typeToken)
                return typeToken;

            if (node is IdToken idToken)
                return new TypeToken(idToken.Id, idToken.TextSpan);

            return null;
        }

        /// <summary>
        /// When a name is absent, deconstruct a given tuple, otherwise create virtual member references
        /// </summary>
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

                var initValues = node.Variables.Select(v =>
                {
                    var tuple = v.Initializer?.Value as TupleExpressionSyntax;
                    if (tuple == null)
                    {
                        return null;
                    }
                    return (Expression)base.Visit(tuple.Arguments.ElementAtOrDefault(i));
                }).ToList();

                variables = CreateVirtualMemberRefs(variables, typeElement, i, initValues, typeNode.Elements[i].GetTextSpan());
            }
            var declaration = new VariableDeclarationExpression(null, variables, textSpan);
            return new ExpressionStatement(declaration);
        }

        private List<AssignmentExpression> CreateVirtualMemberRefs(List<AssignmentExpression> variables,
            TupleElementSyntax typeElement, int initializerNumber,
            List<Expression> initValues, TextSpan textSpan)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                var variable = variables[i];
                var tupleTypeElementMemRef = new MemberReferenceExpression
                {
                    Target = new IdToken(((IdToken)variable.Left).Id, variable.Left.TextSpan),
                    Name = ConvertId(typeElement.Identifier)
                };

                var right = variable.Right as TupleCreateExpression;
                right?.Initializers.Add(new AssignmentExpression
                {
                    Left = tupleTypeElementMemRef,
                    Right = initValues[i],
                    TextSpan = textSpan
                });
            }
            return variables;
        }

        private Ust HandleTupleStatement(VariableDeclaratorSyntax node)
        {
            var tuple = base.Visit(node.Initializer.Value) as TupleCreateExpression;

            if (tuple == null)
            {
                return null;
            }

            var idText = node.Identifier.ValueText;
            var idTextSpan = node.Identifier.GetTextSpan();

            tuple.Initializers.ForEach(init =>
            {
                var assignment = (AssignmentExpression)init;
                assignment.Left = new MemberReferenceExpression(
                    new IdToken(idText, idTextSpan), assignment.Left, assignment.Left.TextSpan);
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
                Logger.LogError(new ConversionException(root?.SourceFile, message: ex.Message)
                {
                    TextSpan = node.GetTextSpan()
                });
                return null;
            }
        }
    }
}
