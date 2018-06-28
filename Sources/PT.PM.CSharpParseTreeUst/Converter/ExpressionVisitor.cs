using System;
using System.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PT.PM.Common.Nodes.TypeMembers;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;

namespace PT.PM.CSharpParseTreeUst.RoslynUstVisitor
{
    public partial class CSharpRoslynParseTreeConverter
    {
        #region Anonymous

        public override Ust VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            var parameters = node.ParameterList == null ? new ParameterDeclaration[0] :
                node.ParameterList.Parameters.Select(p => (ParameterDeclaration)VisitAndReturnNullIfError(p))
                .ToArray();
            var body = (BlockStatement)VisitBlock(node.Block);

            var result = new AnonymousMethodExpression(parameters, body, node.GetTextSpan());
            return result;
        }

        /// <summary>
        /// var a = new A
        /// {
        ///     b = 2,
        ///     c = 3,
        /// }
        ///
        /// var a = new A();
        /// a.b = 2;
        /// a.c = 3;
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override Ust VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
        {
            var typeToken = new TypeToken("Anonymous", node.OpenBraceToken.GetTextSpan());
            Expression[] args = node.Initializers.Select(init =>
            {
                try
                {
                    var left = init.NameEquals == null ? null :
                        new MemberReferenceExpression(typeToken,
                        ConvertId(init.NameEquals.Name.Identifier), init.NameEquals.Name.GetTextSpan());
                    var right = (Expression)base.Visit(init.Expression);

                    var assignment = new AssignmentExpression(left, right, init.GetTextSpan());
                    return assignment;
                }
                catch (Exception ex) when (!(ex is ThreadAbortException))
                {
                    Logger.LogError(new ConversionException(root?.SourceCodeFile, message: ex.Message));
                    return null;
                }
            }).ToArray();
            var argsNode = new ArgsUst(args, node.GetTextSpan());

            var result = new ObjectCreateExpression(typeToken, argsNode, node.GetTextSpan());
            return result;
        }

        public override Ust VisitAnonymousObjectMemberDeclarator(AnonymousObjectMemberDeclaratorSyntax node)
        {
            return base.VisitAnonymousObjectMemberDeclarator(node);
        }

        #endregion

        public override Ust VisitArgument(ArgumentSyntax node)
        {
            var result = (Expression)VisitAndReturnNullIfError(node.Expression);
            if (!node.RefKindKeyword.IsKind(SyntaxKind.None))
            {
                InOutModifierLiteral modifierLiteral = new InOutModifierLiteral(
                    node.RefKindKeyword.IsKind(SyntaxKind.OutKeyword) ? InOutModifier.Out : InOutModifier.InOut,
                    node.RefKindKeyword.GetTextSpan());
                result = new ArgumentExpression(modifierLiteral, result, node.GetTextSpan());
            }
            return result;
        }

        public override Ust VisitDeclarationExpression(DeclarationExpressionSyntax node)
        {
            var left = (Expression)VisitAndReturnNullIfError(node.Designation);
            return new AssignmentExpression(left, null, node.GetTextSpan());
        }

        public override Ust VisitSingleVariableDesignation(SingleVariableDesignationSyntax node)
        {
            return ConvertId(node.Identifier);
        }

        public override Ust VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
        {
            var result = (Expression)base.Visit(node.Expression);
            return result;
        }

        #region Arrays

        public override Ust VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
        {
            var type = ConvertType(base.Visit(node.Type.ElementType));
            List<Expression> sizes = node.Type.RankSpecifiers
                .SelectMany(rank => rank.Sizes.Select(s => (Expression)VisitAndReturnNullIfError(s))).ToList();
            List<Expression> inits = node.Initializer != null
                ? node.Initializer.Expressions.Select(e => (Expression)VisitAndReturnNullIfError(e)).ToList()
                : null;

            var result = new ArrayCreationExpression(type, sizes, inits, node.GetTextSpan());

            return result;
        }

        public override Ust VisitStackAllocArrayCreationExpression(StackAllocArrayCreationExpressionSyntax node)
        {
            var arrayTypeSyntax = (ArrayTypeSyntax)node.Type; // TODO: Fix it
            var type = (TypeToken)base.Visit(node.Type);
            var sizes = arrayTypeSyntax.RankSpecifiers
                .SelectMany(rank => rank.Sizes.Select(s => (Expression)VisitAndReturnNullIfError(s))).ToArray();

            var result = new ArrayCreationExpression(type, sizes, new Expression[0], node.GetTextSpan());
            return result;
        }

        public override Ust VisitArrayType(ArrayTypeSyntax node)
        {
            var arrayType = node.ToString().Replace(",", "][");
            var result = new TypeToken(arrayType, node.GetTextSpan());

            return result;
        }

        public override Ust VisitArrayRankSpecifier(ArrayRankSpecifierSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node)
        {
            var type = new TypeToken(CommonUtils.Prefix + "object", node.NewKeyword.GetTextSpan());
            var sizes = node.Commas.Select(c => new IntLiteral(0, c.GetTextSpan())).ToList();
            sizes.Add(new IntLiteral(0, node.CloseBracketToken.GetTextSpan()));

            var result = new ArrayCreationExpression(type, sizes, new Expression[0], node.GetTextSpan());
            return result;
        }

        public override Ust VisitOmittedArraySizeExpression(OmittedArraySizeExpressionSyntax node)
        {
            var result = new IntLiteral(0, node.GetTextSpan());
            return result;
        }

        #endregion

        public override Ust VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            var left = (Expression)base.Visit(node.Left);
            var right = (Expression)base.Visit(node.Right);

            AssignmentExpression result;
            var opText = node.OperatorToken.ValueText;
            if (opText == "=")
            {
                result = new AssignmentExpression(left, right, node.GetTextSpan());
            }
            else
            {
                BinaryOperator op = BinaryOperatorLiteral.TextBinaryOperator[opText.Remove(opText.Length - 1)];
                // TODO: implement assignment + operator
                result = new AssignmentExpression(left, right, node.GetTextSpan());
                result.BinaryOperator = new BinaryOperatorLiteral(
                        op, node.OperatorToken.GetTextSpan());
            }

            return result;
        }

        public override Ust VisitBaseExpression(BaseExpressionSyntax node)
        {
            var result = new BaseReferenceToken(node.GetTextSpan());
            return result;
        }

        public override Ust VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (node.OperatorToken.ValueText == "is") // x is y -> (y)x != null
            {
                var type = ConvertType(base.Visit(node.Right));
                var expression = (Expression)base.Visit(node.Left);
                var left = new CastExpression(type, expression, node.GetTextSpan());

                var operatorSpan = node.OperatorToken.GetTextSpan();
                var literal = new BinaryOperatorLiteral(BinaryOperator.NotEqual, operatorSpan);
                var right = new NullLiteral(operatorSpan);

                var result = new BinaryOperatorExpression(left, literal, right, node.GetTextSpan());
                return result;
            }

            if (node.OperatorToken.ValueText == "as")
            {
                var type = ConvertType(base.Visit(node.Right));
                var expression = (Expression)base.Visit(node.Left);

                var result = new CastExpression(type, expression, node.GetTextSpan());
                return result;
            }

            if (node.OperatorToken.ValueText == "??")
            {
                var trueExpression = (Expression)base.Visit(node.Left);
                var operatorSpan = node.OperatorToken.GetTextSpan();
                var condition = new BinaryOperatorExpression(
                    trueExpression,
                    new BinaryOperatorLiteral(BinaryOperator.NotEqual, operatorSpan),
                    new NullLiteral(operatorSpan),
                    operatorSpan);
                var falseExpression = (Expression)base.Visit(node.Right);

                var result = new ConditionalExpression(condition, trueExpression, falseExpression, node.GetTextSpan());
                return result;
            }
            else
            {
                var result = CreateBinaryOperatorExpression(node);
                return result;
            }
        }

        public override Ust VisitEqualsValueClause(EqualsValueClauseSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitCastExpression(CastExpressionSyntax node)
        {
            var type = ConvertType(base.Visit(node.Type));
            var expression = (Expression)base.Visit(node.Expression);

            var result = new CastExpression(type, expression, node.GetTextSpan());
            return result;
        }

        public override Ust VisitCheckedExpression(CheckedExpressionSyntax node)
        {
            return base.Visit(node.Expression);
        }

        public override Ust VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            var condition = (Expression)base.Visit(node.Condition);
            var trueExpression = (Expression)base.Visit(node.WhenTrue);
            var falseExpression = (Expression)base.Visit(node.WhenFalse);

            var result = new ConditionalExpression(
                condition,
                trueExpression,
                falseExpression,
                node.GetTextSpan()
            );
            return result;
        }

        public override Ust VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
        {
            Ust ustNode = base.Visit(node.Expression);
            Expression expression = ustNode as Expression;
            if (expression == null)
            {
                expression = new MultichildExpression(((ArgsUst)ustNode).Collection, ustNode.TextSpan);
            }
            Expression whenNotNullExpression;
            if (node.WhenNotNull is ElementBindingExpressionSyntax)
            {
                var args = (ArgsUst)VisitElementBindingExpression((ElementBindingExpressionSyntax)node.WhenNotNull);
                whenNotNullExpression = new IndexerExpression(expression, args, args.TextSpan);
            }
            else
            {
                whenNotNullExpression = (Expression)base.Visit(node.WhenNotNull);
            }
            var nullExpr = new NullLiteral(default(TextSpan));
            var binayOpLiteral = new BinaryOperatorLiteral(BinaryOperator.Equal, default(TextSpan));
            var condition = new BinaryOperatorExpression(expression, binayOpLiteral, nullExpr, default(TextSpan));

            var result = new ConditionalExpression(condition, nullExpr, whenNotNullExpression, node.GetTextSpan());
            return result;
        }

        public override Ust VisitDefaultExpression(DefaultExpressionSyntax node)
        {
            var span = node.GetTextSpan();
            var typeName = node.Type.ToString();
            switch (typeName)
            {
                case "string":
                case "char":
                    return new StringLiteral("", span);
                case "int":
                case "uint":
                case "sbyte":
                case "byte":
                case "short":
                case "ushort":
                case "long":
                case "ulong":
                    return new IntLiteral(0, span);
                case "float":
                case "double":
                case "decimal":
                    return new FloatLiteral(0.0, span);
                case "bool":
                default:
                    return new NullLiteral(span);
            }
        }

        public override Ust VisitAliasQualifiedName(AliasQualifiedNameSyntax node)
        {
            var alias = (IdToken)VisitIdentifierName(node.Alias);
            var name = (Token)base.Visit(node.Name);
        
            var result = new TypeToken(new [] { alias.Id, name.TextValue }, node.GetTextSpan());
            return result;
        }

        public override Ust VisitIdentifierName(IdentifierNameSyntax node)
        {
            IdToken result = ConvertId(node.Identifier);
            return result;
        }

        public override Ust VisitQualifiedName(QualifiedNameSyntax node)
        {
            var typeParts =
                node.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Select(id => id.Identifier.ValueText)
                .ToArray();
            var result = new TypeToken(typeParts, node.GetTextSpan());
            return result;
        }

        public override Ust VisitNullableType(NullableTypeSyntax node)
        {
            var result = new TypeToken(node.ElementType.ToString(), node.GetTextSpan());
            return result;
        }

        public override Ust VisitPointerType(PointerTypeSyntax node)
        {
            var result = ConvertType(base.Visit(node.ElementType));
            return result;
        }

        public override Ust VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            var target = (Expression)base.Visit(node.Expression);
            ArgsUst args = node.ArgumentList == null ? null : (ArgsUst)VisitBracketedArgumentList(node.ArgumentList);

            var result = new IndexerExpression(target, args, node.GetTextSpan());
            return result;
        }

        public override Ust VisitElementBindingExpression(ElementBindingExpressionSyntax node)
        {
            ArgsUst args = node.ArgumentList == null ? null : (ArgsUst)VisitBracketedArgumentList(node.ArgumentList);
            return args;
        }

        public override Ust VisitImplicitElementAccess(ImplicitElementAccessSyntax node)
        {
            var args = (ArgsUst)VisitBracketedArgumentList(node.ArgumentList);
            var target = new IdToken(CommonUtils.Prefix + "index_initializer", default(TextSpan));
            var result = new IndexerExpression(target, args, node.GetTextSpan());
            return result;
        }

        public override Ust VisitIndexerMemberCref(IndexerMemberCrefSyntax node)
        {
            
            throw new NotImplementedException();
        }

        public override Ust VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var target = (Expression)base.Visit(node.Expression);
            ArgsUst args = node.ArgumentList == null ? null : (ArgsUst)VisitArgumentList(node.ArgumentList);

            var result = new InvocationExpression(target, args, node.GetTextSpan());

            return result;
        }

        public override Ust VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            ParameterDeclaration[] parameters = node.Parameter == null ? new ParameterDeclaration[0] :
                  new[] { (ParameterDeclaration)VisitParameter(node.Parameter) };

            var result = GetAnonymousMethod(node, parameters);
            return result;
        }

        public override Ust VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            ParameterDeclaration[] parameters = node.ParameterList == null ? new ParameterDeclaration[0] :
                 node.ParameterList.Parameters.Select(p => (ParameterDeclaration)VisitAndReturnNullIfError(p))
                 .ToArray();

            var result = GetAnonymousMethod(node, parameters);
            return result;
        }

        private Ust GetAnonymousMethod(LambdaExpressionSyntax node, ParameterDeclaration[] parameters)
        {
            var idNameSyntax = node.Body as IdentifierNameSyntax;
            Statement bodyStatement;
            if (idNameSyntax != null)
                bodyStatement = new ReturnStatement((Expression)VisitIdentifierName(idNameSyntax), node.Body.GetTextSpan());
            else
            {
                var visited = base.Visit(node.Body);
                if (visited is Statement)
                    bodyStatement = (Statement)visited;
                else
                    bodyStatement = new ExpressionStatement((Expression)visited, node.Body.GetTextSpan());
            }
            var body = new BlockStatement(new[] { bodyStatement }, node.Body.GetTextSpan());

            var result = new AnonymousMethodExpression(parameters, body, node.GetTextSpan());
            return result;
        }

        public override Ust VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var name = ConvertId(node.Name.Identifier);
            Expression result;
            if (node.Expression is ElementBindingExpressionSyntax)
            {
                var args = (ArgsUst)VisitElementBindingExpression((ElementBindingExpressionSyntax)node.Expression);
                result = new IndexerExpression(name, args, args.TextSpan);
            }
            else
            {
                var target = (Expression)base.Visit(node.Expression);
                result = new MemberReferenceExpression(target, name, node.GetTextSpan());
            }

            return result;
        }

        public override Ust VisitMemberBindingExpression(MemberBindingExpressionSyntax node)
        {
            var result = ConvertId(node.Name.Identifier);
            return result;
        }

        // Named argument
        // Named expression

        public override Ust VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var type = ConvertType(base.Visit(node.Type));
            ArgsUst args = node.ArgumentList == null ? null : (ArgsUst)VisitArgumentList(node.ArgumentList);

            var initializers = node.Initializer == null ? null : 
                node.Initializer.Expressions.Select(e => (Expression)VisitAndReturnNullIfError(e))
                .ToList();

            var result = new ObjectCreateExpression(
                type,
                args,
                node.GetTextSpan())
            {
                Initializers = initializers
            };
            return result;
        }

        public override Ust VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            var children = node.Expressions.Select(e => (Expression)VisitAndReturnNullIfError(e))
                .ToArray();
            
            var result = new MultichildExpression(children, node.GetTextSpan());
            return result;
        }

        public override Ust VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            var result = (Expression)base.Visit(node.Expression);
            return result;
        }

        public override Ust VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            var token = node.Token;
            var span = node.GetTextSpan();

            if (token.Value == null)
            {
                return new NullLiteral(span);
            }

            var str = token.Value as string;
            if (str != null)
            {
                return new StringLiteral(str, span);
            }

            if (token.Value is char)
            {
                return new StringLiteral(token.ValueText, span);
            }

            var typeName = token.Value.GetType().Name;
            switch (typeName)
            {
                case "Boolean":
                    return new BooleanLiteral((bool)token.Value, node.GetTextSpan());
                case "Int32":
                case "UInt32":
                case "Int16":
                case "UInt16":
                case "Byte":
                case "SByte":
                case "Int64":
                    return new IntLiteral(System.Convert.ToInt64(token.Value), node.GetTextSpan());
                case "UInt64":
                    return new IntLiteral((long)System.Convert.ToUInt64(token.Value), node.GetTextSpan());
                case "Double":
                case "Single":
                case "Decimal":
                    return new FloatLiteral(System.Convert.ToDouble(token.Value), node.GetTextSpan());
                default:
                    throw new NotImplementedException();
            }
        }

        #region Query

        public override Ust VisitQueryExpression(QueryExpressionSyntax node)
        {
            IEnumerable<Expression> expressions = node.DescendantNodes()
                .OfType<ExpressionSyntax>()
                .Select(exp => VisitAndReturnNullIfError(exp) as Expression)
                .Where(expr => expr != null);

            var result = new MultichildExpression(expressions, node.GetTextSpan());
            return result;
        }

        public override Ust VisitQueryBody(QueryBodySyntax node)
        {
            return VisitQueryBody(node);
        }

        public override Ust VisitQueryContinuation(QueryContinuationSyntax node)
        {
            return base.VisitQueryContinuation(node);
        }

        #endregion

        public override Ust VisitSizeOfExpression(SizeOfExpressionSyntax node)
        {
            var result = new IntLiteral(0, node.GetTextSpan());
            return result;
        }

        public override Ust VisitThisExpression(ThisExpressionSyntax node)
        {
            var result = new ThisReferenceToken(node.GetTextSpan());
            return result;
        }

        public override Ust VisitTypeOfExpression(TypeOfExpressionSyntax node)
        {
            IdToken id = ConvertId(node.Keyword);
            TypeToken type = ConvertType(base.Visit(node.Type));
            ArgsUst args = new ArgsUst(new[] { type }, type.TextSpan);

            var result = new InvocationExpression(id, args, node.GetTextSpan());
            return result;
        }

        public override Ust VisitMakeRefExpression(MakeRefExpressionSyntax node)
        {
            IdToken id = ConvertId(node.Keyword);
            Expression expr = (Expression)base.Visit(node.Expression);
            ArgsUst args = new ArgsUst(new Expression[] { expr }, expr.TextSpan);

            var result = new InvocationExpression(id, args, node.GetTextSpan());
            return result;
        }

        public override Ust VisitRefTypeExpression(RefTypeExpressionSyntax node)
        {
            IdToken id = ConvertId(node.Keyword);
            Expression expr = (Expression)base.Visit(node.Expression);
            ArgsUst args = new ArgsUst(new Expression[] { expr }, expr.TextSpan);

            var result = new InvocationExpression(id, args, node.GetTextSpan());
            return result;
        }

        public override Ust VisitRefValueExpression(RefValueExpressionSyntax node)
        {
            IdToken id = ConvertId(node.Keyword);
            TypeToken type = (TypeToken)base.Visit(node.Type);
            Expression expr = (Expression)base.Visit(node.Expression);
            ArgsUst args = new ArgsUst(new Expression[] { expr, type }, expr.TextSpan);

            var result = new InvocationExpression(id, args, node.GetTextSpan());
            return result;
        }

        public override Ust VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            var result = CreateUnaryOperatorExpression(true, node);
            return result;
        }

        public override Ust VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            var result = CreateUnaryOperatorExpression(false, node);
            return result;
        }

        public override Ust VisitAwaitExpression(AwaitExpressionSyntax node)
        {
            var result = (Expression)base.Visit(node.Expression);
            return result;
        }

        #region String Interpolation

        public override Ust VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
        {
            Expression[] expressions = node.Contents.Select(content => (Expression)VisitAndReturnNullIfError(content)).ToArray();
            var result = new MultichildExpression(expressions, node.GetTextSpan());
            return result;
        }

        public override Ust VisitInterpolation(InterpolationSyntax node)
        {
            var result = base.Visit(node.Expression);
            return result;
        }

        public override Ust VisitInterpolatedStringText(InterpolatedStringTextSyntax node)
        {
            var result = new StringLiteral(node.ToString(), node.GetTextSpan());
            return result;
        }

        public override Ust VisitInterpolationAlignmentClause(InterpolationAlignmentClauseSyntax node)
        {
            return base.VisitInterpolationAlignmentClause(node);
        }

        public override Ust VisitInterpolationFormatClause(InterpolationFormatClauseSyntax node)
        {
            return base.VisitInterpolationFormatClause(node);
        }

        #endregion

        protected Ust CreateBinaryOperatorExpression(BinaryExpressionSyntax node)
        {
            var left = (Expression)base.Visit(node.Left);
            var op = new BinaryOperatorLiteral(node.OperatorToken.ValueText, node.OperatorToken.GetTextSpan());
            var right = (Expression)base.Visit(node.Right);

            var result = new BinaryOperatorExpression(left, op, right, node.GetTextSpan());
            return result;
        }

        protected UnaryOperatorExpression CreateUnaryOperatorExpression(bool prefix, ExpressionSyntax node)
        {
            ExpressionSyntax operandSyntax;
            SyntaxToken operatorToken;
            if (prefix)
            {
                var prefixOperator = (PrefixUnaryExpressionSyntax)node;
                operandSyntax = prefixOperator.Operand;
                operatorToken = prefixOperator.OperatorToken;
            }
            else
            {
                var postfixOperator = (PostfixUnaryExpressionSyntax)node;
                operandSyntax = postfixOperator.Operand;
                operatorToken = postfixOperator.OperatorToken;
            }
            var op = new UnaryOperatorLiteral(prefix, operatorToken.ValueText, operatorToken.GetTextSpan());
            var operand = (Expression)base.Visit(operandSyntax);

            var result = new UnaryOperatorExpression(
                op,
                operand,
                node.GetTextSpan()
            );
            return result;
        }
    }
}
