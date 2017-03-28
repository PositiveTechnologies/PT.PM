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
//using Microsoft.CodeAnalysis.FindSymbols;

namespace PT.PM.CSharpParseTreeUst.RoslynUstVisitor
{
    public partial class RoslynUstCommonConverterVisitor
    {
        #region Anonymous

        public override UstNode VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            var parameters = node.ParameterList == null ? new ParameterDeclaration[0] :
                node.ParameterList.Parameters.Select(p => (ParameterDeclaration)VisitAndReturnNullIfError(p))
                .ToArray();
            var body = (BlockStatement)VisitBlock(node.Block);

            var result = new AnonymousMethodExpression(parameters, body, node.GetTextSpan(), FileNode);
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
        public override UstNode VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
        {
            var typeToken = new TypeToken("Anonymous", node.OpenBraceToken.GetTextSpan(), FileNode);
            Expression[] args = node.Initializers.Select(init =>
            {
                try
                {
                    var left = init.NameEquals == null ? null :
                        new MemberReferenceExpression(typeToken,
                        ConvertId(init.NameEquals.Name.Identifier), init.NameEquals.Name.GetTextSpan(), FileNode);
                    var right = (Expression)base.Visit(init.Expression);

                    var assignment = new AssignmentExpression(left, right, init.GetTextSpan(), FileNode);
                    return assignment;
                }
                catch (Exception ex)
                {
                    Logger.LogError(new ConversionException(ex.Message));
                    return null;
                }
            }).ToArray();
            var argsNode = new ArgsNode(args, node.GetTextSpan(), FileNode);

            var result = new ObjectCreateExpression(typeToken, argsNode, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitAnonymousObjectMemberDeclarator(AnonymousObjectMemberDeclaratorSyntax node)
        {
            return base.VisitAnonymousObjectMemberDeclarator(node);
        }

        #endregion

        public override UstNode VisitArgument(ArgumentSyntax node)
        {
            var result = (Expression)base.Visit(node.Expression);
            return result;
        }

        public override UstNode VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
        {
            var result = (Expression)base.Visit(node.Expression);
            return result;
        }

        #region Arrays

        public override UstNode VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
        {
            var type = ConvertType(base.Visit(node.Type.ElementType));
            List<Expression> sizes = node.Type.RankSpecifiers
                .SelectMany(rank => rank.Sizes.Select(s => (Expression)VisitAndReturnNullIfError(s))).ToList();
            List<Expression> inits = node.Initializer != null
                ? node.Initializer.Expressions.Select(e => (Expression)VisitAndReturnNullIfError(e)).ToList()
                : null;

            var result = new ArrayCreationExpression(type, sizes, inits, node.GetTextSpan(), FileNode);

            return result;
        }

        public override UstNode VisitStackAllocArrayCreationExpression(StackAllocArrayCreationExpressionSyntax node)
        {
            var arrayTypeSyntax = (ArrayTypeSyntax)node.Type; // TODO: Fix it
            var type = (TypeToken)base.Visit(node.Type);
            var sizes = arrayTypeSyntax.RankSpecifiers
                .SelectMany(rank => rank.Sizes.Select(s => (Expression)VisitAndReturnNullIfError(s))).ToArray();

            var result = new ArrayCreationExpression(type, sizes, new Expression[0], node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitArrayType(ArrayTypeSyntax node)
        {
            var arrayType = node.ToString().Replace(",", "][");
            var result = new TypeToken(arrayType, node.GetTextSpan(), FileNode);

            return result;
        }

        public override UstNode VisitArrayRankSpecifier(ArrayRankSpecifierSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node)
        {
            var type = new TypeToken(Helper.Prefix + "object", node.NewKeyword.GetTextSpan(), FileNode);
            var sizes = node.Commas.Select(c => new IntLiteral(0, c.GetTextSpan(), FileNode)).ToList();
            sizes.Add(new IntLiteral(0, node.CloseBracketToken.GetTextSpan(), FileNode));

            var result = new ArrayCreationExpression(type, sizes, new Expression[0], node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitOmittedArraySizeExpression(OmittedArraySizeExpressionSyntax node)
        {
            var result = new IntLiteral(0, node.GetTextSpan(), FileNode);
            return result;
        }

        #endregion

        public override UstNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            var left = (Expression)base.Visit(node.Left);
            var right = (Expression)base.Visit(node.Right);

            AssignmentExpression result;
            var opText = node.OperatorToken.ValueText;
            if (opText == "=")
            {
                result = new AssignmentExpression(left, right, node.GetTextSpan(), FileNode);
            }
            else
            {
                var op = BinaryOperatorLiteral.TextBinaryOperator[opText.Remove(opText.Length - 1)];
                result = ConverterHelper.ConvertToAssignmentExpression(left, op,
                    node.OperatorToken.GetTextSpan(), right,
                    node.GetTextSpan(), FileNode);
            }

            return result;
        }

        public override UstNode VisitBaseExpression(BaseExpressionSyntax node)
        {
            var result = new BaseReferenceExpression(node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (node.OperatorToken.ValueText == "is") // x is y -> (y)x != null
            {
                var type = ConvertType(base.Visit(node.Right));
                var expression = (Expression)base.Visit(node.Left);
                var left = new CastExpression(type, expression, node.GetTextSpan(), FileNode);

                var operatorSpan = node.OperatorToken.GetTextSpan();
                var literal = new BinaryOperatorLiteral(BinaryOperator.NotEqual, operatorSpan, FileNode);
                var right = new NullLiteral(operatorSpan, FileNode);

                var result = new BinaryOperatorExpression(left, literal, right, node.GetTextSpan(), FileNode);
                return result;
            }

            if (node.OperatorToken.ValueText == "as")
            {
                var type = ConvertType(base.Visit(node.Right));
                var expression = (Expression)base.Visit(node.Left);

                var result = new CastExpression(type, expression, node.GetTextSpan(), FileNode);
                return result;
            }

            if (node.OperatorToken.ValueText == "??")
            {
                var trueExpression = (Expression)base.Visit(node.Left);
                var operatorSpan = node.OperatorToken.GetTextSpan();
                var condition = new BinaryOperatorExpression(
                    trueExpression,
                    new BinaryOperatorLiteral(BinaryOperator.NotEqual, operatorSpan, FileNode),
                    new NullLiteral(operatorSpan, FileNode),
                    operatorSpan,
                    FileNode);
                var falseExpression = (Expression)base.Visit(node.Right);

                var result = new ConditionalExpression(condition, trueExpression, falseExpression, node.GetTextSpan(), FileNode);
                return result;
            }
            else
            {
                var result = CreateBinaryOperatorExpression(node);
                return result;
            }
        }

        public override UstNode VisitEqualsValueClause(EqualsValueClauseSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitCastExpression(CastExpressionSyntax node)
        {
            var type = ConvertType(base.Visit(node.Type));
            var expression = (Expression)base.Visit(node.Expression);

            var result = new CastExpression(type, expression, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitCheckedExpression(CheckedExpressionSyntax node)
        {
            return base.Visit(node.Expression);
        }

        public override UstNode VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            var condition = (Expression)base.Visit(node.Condition);
            var trueExpression = (Expression)base.Visit(node.WhenTrue);
            var falseExpression = (Expression)base.Visit(node.WhenFalse);

            var result = new ConditionalExpression(
                condition,
                trueExpression,
                falseExpression,
                node.GetTextSpan(),
                FileNode
            );
            return result;
        }

        public override UstNode VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
        {
            UstNode ustNode = base.Visit(node.Expression);
            Expression expression = ustNode as Expression;
            if (expression == null)
            {
                expression = new MultichildExpression(((ArgsNode)ustNode).Collection, ustNode.TextSpan, FileNode);
            }
            Expression whenNotNullExpression;
            if (node.WhenNotNull is ElementBindingExpressionSyntax)
            {
                var args = (ArgsNode)VisitElementBindingExpression((ElementBindingExpressionSyntax)node.WhenNotNull);
                whenNotNullExpression = new IndexerExpression(expression, args, args.TextSpan, FileNode);
            }
            else
            {
                whenNotNullExpression = (Expression)base.Visit(node.WhenNotNull);
            }
            var nullExpr = new NullLiteral(default(TextSpan), FileNode);
            var binayOpLiteral = new BinaryOperatorLiteral(BinaryOperator.Equal, default(TextSpan), FileNode);
            var condition = new BinaryOperatorExpression(expression, binayOpLiteral, nullExpr, default(TextSpan), FileNode);

            var result = new ConditionalExpression(condition, nullExpr, whenNotNullExpression, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitDefaultExpression(DefaultExpressionSyntax node)
        {
            var span = node.GetTextSpan();
            var typeName = node.Type.ToString();
            switch (typeName)
            {
                case "string":
                case "char":
                    return new StringLiteral("", span, FileNode);
                case "int":
                case "uint":
                case "sbyte":
                case "byte":
                case "short":
                case "ushort":
                case "long":
                case "ulong":
                    return new IntLiteral(0, span, FileNode);
                case "float":
                case "double":
                case "decimal":
                    return new FloatLiteral(0.0, span, FileNode);
                case "bool":
                default:
                    return new NullLiteral(span, FileNode);
            }
        }

        public override UstNode VisitAliasQualifiedName(AliasQualifiedNameSyntax node)
        {
            var alias = (IdToken)VisitIdentifierName(node.Alias);
            var name = (Token)base.Visit(node.Name);
        
            var result = new TypeToken(new [] { alias.Id, name.TextValue }, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            IdToken result = ConvertId(node.Identifier);
            return result;
        }

        public override UstNode VisitQualifiedName(QualifiedNameSyntax node)
        {
            var typeParts =
                node.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Select(id => id.Identifier.ValueText)
                .ToArray();
            var result = new TypeToken(typeParts, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitNullableType(NullableTypeSyntax node)
        {
            var result = new TypeToken(node.ElementType.ToString(), node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitPointerType(PointerTypeSyntax node)
        {
            var result = ConvertType(base.Visit(node.ElementType));
            return result;
        }

        public override UstNode VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            var target = (Expression)base.Visit(node.Expression);
            ArgsNode args = node.ArgumentList == null ? null : (ArgsNode)VisitBracketedArgumentList(node.ArgumentList);

            var result = new IndexerExpression(target, args, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitElementBindingExpression(ElementBindingExpressionSyntax node)
        {
            ArgsNode args = node.ArgumentList == null ? null : (ArgsNode)VisitBracketedArgumentList(node.ArgumentList);
            return args;
        }

        public override UstNode VisitImplicitElementAccess(ImplicitElementAccessSyntax node)
        {
            var args = (ArgsNode)VisitBracketedArgumentList(node.ArgumentList);
            var target = new IdToken(Helper.Prefix + "index_initializer", default(TextSpan), FileNode);
            var result = new IndexerExpression(target, args, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitIndexerMemberCref(IndexerMemberCrefSyntax node)
        {
            
            throw new NotImplementedException();
        }

        public override UstNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var target = (Expression)base.Visit(node.Expression);
            ArgsNode args = node.ArgumentList == null ? null : (ArgsNode)VisitArgumentList(node.ArgumentList);

            var result = new InvocationExpression(target, args, node.GetTextSpan(), FileNode);

            return result;
        }

        public override UstNode VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            ParameterDeclaration[] parameters = node.Parameter == null ? new ParameterDeclaration[0] :
                  new[] { (ParameterDeclaration)VisitParameter(node.Parameter) };

            var result = GetAnonymousMethod(node, parameters);
            return result;
        }

        public override UstNode VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            ParameterDeclaration[] parameters = node.ParameterList == null ? new ParameterDeclaration[0] :
                 node.ParameterList.Parameters.Select(p => (ParameterDeclaration)VisitAndReturnNullIfError(p))
                 .ToArray();

            var result = GetAnonymousMethod(node, parameters);
            return result;
        }

        private UstNode GetAnonymousMethod(LambdaExpressionSyntax node, ParameterDeclaration[] parameters)
        {
            var idNameSyntax = node.Body as IdentifierNameSyntax;
            Statement bodyStatement;
            if (idNameSyntax != null)
                bodyStatement = new ReturnStatement((Expression)VisitIdentifierName(idNameSyntax), node.Body.GetTextSpan(), FileNode);
            else
            {
                var visited = base.Visit(node.Body);
                if (visited is Statement)
                    bodyStatement = (Statement)visited;
                else
                    bodyStatement = new ExpressionStatement((Expression)visited, node.Body.GetTextSpan(), FileNode);
            }
            var body = new BlockStatement(new[] { bodyStatement }, node.Body.GetTextSpan(), FileNode);

            var result = new AnonymousMethodExpression(parameters, body, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var name = ConvertId(node.Name.Identifier);
            Expression result;
            if (node.Expression is ElementBindingExpressionSyntax)
            {
                var args = (ArgsNode)VisitElementBindingExpression((ElementBindingExpressionSyntax)node.Expression);
                result = new IndexerExpression(name, args, args.TextSpan, FileNode);
            }
            else
            {
                var target = (Expression)base.Visit(node.Expression);
                result = new MemberReferenceExpression(target, name, node.GetTextSpan(), FileNode);
            }

            return result;
        }

        public override UstNode VisitMemberBindingExpression(MemberBindingExpressionSyntax node)
        {
            var result = ConvertId(node.Name.Identifier);
            return result;
        }

        // Named argument
        // Named expression

        public override UstNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var type = ConvertType(base.Visit(node.Type));
            ArgsNode args = node.ArgumentList == null ? null : (ArgsNode)VisitArgumentList(node.ArgumentList);

            var initializers = node.Initializer == null ? null : 
                node.Initializer.Expressions.Select(e => (Expression)VisitAndReturnNullIfError(e))
                .ToList();

            var result = new ObjectCreateExpression(
                type,
                args,
                node.GetTextSpan(),
                FileNode)
            {
                Initializers = initializers
            };
            return result;
        }

        public override UstNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            var children = node.Expressions.Select(e => (Expression)VisitAndReturnNullIfError(e))
                .ToArray();
            
            var result = new MultichildExpression(children, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            var result = (Expression)base.Visit(node.Expression);
            return result;
        }

        public override UstNode VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            var token = node.Token;
            var span = node.GetTextSpan();

            if (token.Value == null)
            {
                return new NullLiteral(span, FileNode);
            }

            var str = token.Value as string;
            if (str != null)
            {
                return new StringLiteral(str, span, FileNode);
            }

            if (token.Value is char)
            {
                return new StringLiteral(token.ValueText, span, FileNode);
            }

            var typeName = token.Value.GetType().Name;
            switch (typeName)
            {
                case "Boolean":
                    return new BooleanLiteral((bool)token.Value, node.GetTextSpan(), FileNode);
                case "Int32":
                case "UInt32":
                case "Int16":
                case "UInt16":
                case "Byte":
                case "SByte":
                case "Int64":
                    return new IntLiteral(Convert.ToInt64(token.Value), node.GetTextSpan(), FileNode);
                case "UInt64":
                    return new IntLiteral((long)Convert.ToUInt64(token.Value), node.GetTextSpan(), FileNode);
                case "Double":
                case "Single":
                case "Decimal":
                    return new FloatLiteral(Convert.ToDouble(token.Value), node.GetTextSpan(), FileNode);
                default:
                    throw new NotImplementedException();
            }
        }

        #region Query

        public override UstNode VisitQueryExpression(QueryExpressionSyntax node)
        {
            var expressions = node.DescendantNodes().OfType<ExpressionSyntax>()
                .Select(exp => (Expression)VisitAndReturnNullIfError(exp)).ToList();

            var result = new MultichildExpression(expressions, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitQueryBody(QueryBodySyntax node)
        {
            return VisitQueryBody(node);
        }

        public override UstNode VisitQueryContinuation(QueryContinuationSyntax node)
        {
            return base.VisitQueryContinuation(node);
        }

        #endregion

        public override UstNode VisitSizeOfExpression(SizeOfExpressionSyntax node)
        {
            var result = new IntLiteral(0, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitThisExpression(ThisExpressionSyntax node)
        {
            var result = new ThisReferenceToken(node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitTypeOfExpression(TypeOfExpressionSyntax node)
        {
            IdToken id = ConvertId(node.Keyword);
            TypeToken type = ConvertType(base.Visit(node.Type));
            ArgsNode args = new ArgsNode(new[] { type }, type.TextSpan, FileNode);

            var result = new InvocationExpression(id, args, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitMakeRefExpression(MakeRefExpressionSyntax node)
        {
            IdToken id = ConvertId(node.Keyword);
            Expression expr = (Expression)base.Visit(node.Expression);
            ArgsNode args = new ArgsNode(new Expression[] { expr }, expr.TextSpan, FileNode);

            var result = new InvocationExpression(id, args, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitRefTypeExpression(RefTypeExpressionSyntax node)
        {
            IdToken id = ConvertId(node.Keyword);
            Expression expr = (Expression)base.Visit(node.Expression);
            ArgsNode args = new ArgsNode(new Expression[] { expr }, expr.TextSpan, FileNode);

            var result = new InvocationExpression(id, args, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitRefValueExpression(RefValueExpressionSyntax node)
        {
            IdToken id = ConvertId(node.Keyword);
            TypeToken type = (TypeToken)base.Visit(node.Type);
            Expression expr = (Expression)base.Visit(node.Expression);
            ArgsNode args = new ArgsNode(new Expression[] { expr, type }, expr.TextSpan, FileNode);

            var result = new InvocationExpression(id, args, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            var result = CreateUnaryOperatorExpression(true, node);
            return result;
        }

        public override UstNode VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            var result = CreateUnaryOperatorExpression(false, node);
            return result;
        }

        public override UstNode VisitAwaitExpression(AwaitExpressionSyntax node)
        {
            var result = (Expression)base.Visit(node.Expression);
            return result;
        }

        #region String Interpolation

        public override UstNode VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
        {
            Expression[] expressions = node.Contents.Select(content => (Expression)VisitAndReturnNullIfError(content)).ToArray();
            var result = new MultichildExpression(expressions, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitInterpolation(InterpolationSyntax node)
        {
            var result = base.Visit(node.Expression);
            return result;
        }

        public override UstNode VisitInterpolatedStringText(InterpolatedStringTextSyntax node)
        {
            var result = new StringLiteral(node.ToString(), node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitInterpolationAlignmentClause(InterpolationAlignmentClauseSyntax node)
        {
            return base.VisitInterpolationAlignmentClause(node);
        }

        public override UstNode VisitInterpolationFormatClause(InterpolationFormatClauseSyntax node)
        {
            return base.VisitInterpolationFormatClause(node);
        }

        #endregion

        protected UstNode CreateBinaryOperatorExpression(BinaryExpressionSyntax node)
        {
            var left = (Expression)base.Visit(node.Left);
            var op = new BinaryOperatorLiteral(node.OperatorToken.ValueText, node.OperatorToken.GetTextSpan(), FileNode);
            var right = (Expression)base.Visit(node.Right);

            var result = new BinaryOperatorExpression(left, op, right, node.GetTextSpan(), FileNode);
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
            var op = new UnaryOperatorLiteral(prefix, operatorToken.ValueText, operatorToken.GetTextSpan(), FileNode);
            var operand = (Expression)base.Visit(operandSyntax);

            var result = new UnaryOperatorExpression(
                op,
                operand,
                node.GetTextSpan(),
                FileNode
            );
            return result;
        }
    }
}
