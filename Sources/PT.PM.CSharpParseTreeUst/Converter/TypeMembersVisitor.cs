using System;
using System.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.TypeMembers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PT.PM.CSharpParseTreeUst.RoslynUstVisitor
{
    public partial class CSharpRoslynParseTreeConverter
    {
        #region Never invoked overrides (Implementation error if invoked)

        public override UstNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitEventDeclaration(EventDeclarationSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            throw new InvalidOperationException();
        }

        #endregion

        #region Constructor

        public override UstNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            var typeName = ConvertId(node.Identifier);
            // TODO: fix with args node
            var args = node.ParameterList.Parameters.Select(p => (ParameterDeclaration)VisitAndReturnNullIfError(p))
                .ToArray();
            var modifiers = node.Modifiers.Select(ConvertModifier).ToList();
            var body = (BlockStatement)VisitBlock(node.Body);

            var result = new ConstructorDeclaration(typeName, args, body, node.GetTextSpan())
            {
                Modifiers = modifiers
            };
            return result;
        }

        public override UstNode VisitConstructorConstraint(ConstructorConstraintSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitConstructorInitializer(ConstructorInitializerSyntax node)
        {
            throw new InvalidOperationException();
        }

        #endregion

        public override UstNode VisitDestructorDeclaration(DestructorDeclarationSyntax node)
        {
            var name = new IdToken(node.Identifier.ValueText + "_Destroy", node.Identifier.GetTextSpan());
            var body = (BlockStatement)VisitBlock(node.Body);

            var result = new MethodDeclaration(name, null, body, node.GetTextSpan());
            return result;
        }

        public override UstNode VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
        {
            var init = (Expression)base.Visit(node.EqualsValue != null ? node.EqualsValue.Value : null);
            AssignmentExpression[] vars = new [] { new AssignmentExpression(
                ConvertId(node.Identifier),
                init,
                node.GetTextSpan())
            };

            var result = new FieldDeclaration(vars, node.GetTextSpan());
            return result;
        }

        public override UstNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var varDelaraions = node.Declaration.Variables.Select(
                var => (AssignmentExpression)VisitAndReturnNullIfError(var)).ToArray();
            var modifiers = node.Modifiers.Select(ConvertModifier).ToList();

            var result = new FieldDeclaration(varDelaraions, node.GetTextSpan())
            {
                Modifiers = modifiers
            };
            return result;
        }

        public override UstNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var id = new IdToken(node.Identifier.ValueText, node.Identifier.GetTextSpan());

            TypeToken returnType = null;
            if(node.ReturnType != null)
            {
                if (node.ReturnType is PredefinedTypeSyntax predifinedReturnType)
                {
                    var typeId = predifinedReturnType.Keyword.Text;
                    if (typeId != null)
                    {
                        returnType = new TypeToken(typeId, node.ReturnType.GetTextSpan(), FileNode);
                    }
                }
                else
                {
                    returnType = new TypeToken(node.ReturnType.ToString(), node.ReturnType.GetTextSpan(), FileNode);
                }
            }

            var parameters = node.ParameterList.Parameters.Select(p => (ParameterDeclaration)VisitAndReturnNullIfError(p)).ToArray();
            var statement = node.Body == null ? null : (BlockStatement)VisitBlock(node.Body); // abstract method if null
            var modifiers = node.Modifiers.Select(ConvertModifier).ToList();

            var result = new MethodDeclaration(
                id,
                parameters,
                statement,
                node.GetTextSpan())
            {
                Modifiers = modifiers,
                ReturnType = returnType,
            };
            return result;
        }

        public override UstNode VisitParameter(ParameterSyntax node)
        {
            TypeToken type = ConvertType(base.Visit(node.Type));
            var id = ConvertId(node.Identifier);
            var result = new ParameterDeclaration(type, id, node.GetTextSpan());
            return result;
        }

        public override UstNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            var initializer = node.Initializer != null ? (Expression)base.Visit(node.Initializer.Value) : null;

            var result = new AssignmentExpression(
                new IdToken(node.Identifier.ValueText, node.Identifier.GetTextSpan()),
                initializer,
                node.GetTextSpan()
            );

            return result;
        }
    }
}
