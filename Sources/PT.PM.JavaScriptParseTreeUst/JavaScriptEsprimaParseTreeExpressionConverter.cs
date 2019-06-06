using Esprima;
using Esprima.Ast;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.TypeMembers;
using System;
using UstExprs = PT.PM.Common.Nodes.Expressions;
using UstLiterals = PT.PM.Common.Nodes.Tokens.Literals;
using UstTokens = PT.PM.Common.Nodes.Tokens;
using UstSpecific = PT.PM.Common.Nodes.Specific;
using Collections = System.Collections.Generic;
using PT.PM.Common;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes;

namespace PT.PM.JavaScriptParseTreeUst
{
    public partial class JavaScriptEsprimaParseTreeConverter
    {
        private UstExprs.Expression VisitExpression(Expression expression)
        {
            try
            {
                switch (expression)
                {
                    case AssignmentExpression assignmentExpression:
                        return VisitAssignmentExpression(assignmentExpression);
                    case AssignmentPattern assignmentPattern:
                        return VisitAssignmentPattern(assignmentPattern);
                    case ArrayExpression arrayExpression:
                        return VisitArrayExpression(arrayExpression);
                    case BinaryExpression binaryExpression:
                        return VisitBinaryExpression(binaryExpression);
                    case CallExpression callExpression:
                        return VisitCallExpression(callExpression);
                    case ConditionalExpression conditionalExpression:
                        return VisitConditionalExpression(conditionalExpression);
                    case FunctionExpression functionExpression:
                        return VisitFunctionExpression(functionExpression);
                    case Identifier identifier:
                        return VisitIdentifier(identifier);
                    case Literal literal:
                        return VisitLiteral(literal);
                    case MemberExpression memberExpression:
                        return VisitMemberExpression(memberExpression);
                    case NewExpression newExpression:
                        return VisitNewExpression(newExpression);
                    case ObjectExpression objectExpression:
                        return VisitObjectExpression(objectExpression);
                    case SequenceExpression sequenceExpression:
                        return VisitSequenceExpression(sequenceExpression);
                    case ThisExpression thisExpression:
                        return VisitThisExpression(thisExpression);
                    case Super super:
                        return VisitSuper(super);
                    case UnaryExpression unaryExpression:
                        return VisitUnaryExpression(unaryExpression);
                    case ArrowFunctionExpression arrowFunctionExpression:
                        return VisitArrowFunctionExpression(arrowFunctionExpression);
                    case TemplateLiteral templateLiteral:
                        return VisitTemplateLiteral(templateLiteral);
                    case TaggedTemplateExpression taggedTemplateExpression:
                        return VisitTaggedTemplateExpression(taggedTemplateExpression);
                    case ClassExpression classExpression:
                        return VisitClassExpression(classExpression);
                    case YieldExpression yieldExpression:
                        return VisitYieldExpression(yieldExpression);
                    case MetaProperty metaProperty:
                        return VisitMetaProperty(metaProperty);
                    default:
                        return VisitUnknownExpression(expression);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(new ConversionException(SourceFile, ex));
                return null;
            }
        }

        private UstExprs.AssignmentExpression VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        {
            var left = Visit(assignmentExpression.Left).AsExpression();
            var right = VisitExpression(assignmentExpression.Right);
            return new UstExprs.AssignmentExpression(left, right, GetTextSpan(assignmentExpression));
        }

        private UstExprs.AssignmentExpression VisitAssignmentPattern(AssignmentPattern assignmentPattern)
        {
            var left = Visit(assignmentPattern.Left).AsExpression();
            var right = Visit(assignmentPattern.Right).AsExpression();
            return new UstExprs.AssignmentExpression(left, right, GetTextSpan(assignmentPattern));
        }

        private UstExprs.ArrayCreationExpression VisitArrayExpression(ArrayExpression arrayExpression)
        {
            var inits = new Collections.List<UstExprs.Expression>(arrayExpression.Elements.Count);

            foreach (ArrayExpressionElement element in arrayExpression.Elements)
            {
                UstExprs.Expression expr = null;

                if (element is SpreadElement spreadElement)
                {
                    Logger.LogDebug("Spread elements are not supported for now"); // TODO
                }
                else if (element is Expression expression)
                {
                    expr = VisitExpression(expression);
                }
                else if (element != null)
                {
                    Logger.LogDebug($"{element.GetType().Name} are not supported for now"); // TODO
                }

                inits.Add(expr);
            }

            return new UstExprs.ArrayCreationExpression(null, null, inits, GetTextSpan(arrayExpression));
        }

        private UstExprs.BinaryOperatorExpression VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            UstTokens.BinaryOperator op = UstTokens.BinaryOperator.None;

            switch (binaryExpression.Operator)
            {
                case BinaryOperator.Plus:
                    op = UstTokens.BinaryOperator.Plus;
                    break;
                case BinaryOperator.Minus:
                    op = UstTokens.BinaryOperator.Minus;
                    break;
                case BinaryOperator.Times:
                    op = UstTokens.BinaryOperator.Multiply;
                    break;
                case BinaryOperator.Divide:
                    op = UstTokens.BinaryOperator.Divide;
                    break;
                case BinaryOperator.Modulo:
                    op = UstTokens.BinaryOperator.Mod;
                    break;
                case BinaryOperator.Equal:
                    op = UstTokens.BinaryOperator.Equal;
                    break;
                case BinaryOperator.NotEqual:
                    op = UstTokens.BinaryOperator.NotEqual;
                    break;
                case BinaryOperator.Greater:
                    op = UstTokens.BinaryOperator.Greater;
                    break;
                case BinaryOperator.GreaterOrEqual:
                    op = UstTokens.BinaryOperator.GreaterOrEqual;
                    break;
                case BinaryOperator.Less:
                    op = UstTokens.BinaryOperator.Less;
                    break;
                case BinaryOperator.LessOrEqual:
                    op = UstTokens.BinaryOperator.LessOrEqual;
                    break;
                case BinaryOperator.StrictlyEqual:
                    op = UstTokens.BinaryOperator.StrictEqual;
                    break;
                case BinaryOperator.StricltyNotEqual:
                    op = UstTokens.BinaryOperator.StrictNotEqual;
                    break;
                case BinaryOperator.BitwiseAnd:
                    op = UstTokens.BinaryOperator.BitwiseAnd;
                    break;
                case BinaryOperator.BitwiseOr:
                    op = UstTokens.BinaryOperator.BitwiseOr;
                    break;
                case BinaryOperator.BitwiseXOr:
                    op = UstTokens.BinaryOperator.BitwiseXor;
                    break;
                case BinaryOperator.LeftShift:
                    op = UstTokens.BinaryOperator.ShiftLeft;
                    break;
                case BinaryOperator.RightShift:
                    op = UstTokens.BinaryOperator.ShiftRight;
                    break;
                case BinaryOperator.UnsignedRightShift:
                    op = UstTokens.BinaryOperator.ShiftRight; // TODO: maybe add specific JavaScript operator?
                    break;
                case BinaryOperator.In:
                    op = UstTokens.BinaryOperator.In;
                    break;
                case BinaryOperator.InstanceOf:
                    op = UstTokens.BinaryOperator.InstanceOf;
                    break;
                case BinaryOperator.LogicalAnd:
                    op = UstTokens.BinaryOperator.LogicalAnd;
                    break;
                case BinaryOperator.LogicalOr:
                    op = UstTokens.BinaryOperator.LogicalOr;
                    break;
            }

            // TODO: literal text span in Esprima library
            var opLiteral = new UstLiterals.BinaryOperatorLiteral(op, default);
            var left = VisitExpression(binaryExpression.Left);
            var right = VisitExpression(binaryExpression.Right);

            return new UstExprs.BinaryOperatorExpression(left, opLiteral, right, GetTextSpan(binaryExpression));
        }

        private UstExprs.InvocationExpression VisitCallExpression(CallExpression callExpression)
        {
            var target = VisitExpression(callExpression.Callee);
            var argsUst = VisitArguments(callExpression.Arguments);
            return new UstExprs.InvocationExpression(target, argsUst, GetTextSpan(callExpression));
        }

        private UstExprs.ConditionalExpression VisitConditionalExpression(ConditionalExpression conditionalExpression)
        {
            var condition = VisitExpression(conditionalExpression.Test);
            var trueExpr = VisitExpression(conditionalExpression.Consequent);
            var falseExpr = VisitExpression(conditionalExpression.Alternate);
            return new UstExprs.ConditionalExpression(condition, trueExpr, falseExpr, GetTextSpan(conditionalExpression));
        }

        private UstExprs.AnonymousMethodExpression VisitFunctionExpression(IFunction function)
        {
            var parameters = VisitParameters(function.Params);
            var body = ConvertToBlockStatementIfRequired(function.Body);
            return new UstExprs.AnonymousMethodExpression(parameters, body, GetTextSpan((Node)function));
        }

        private UstTokens.IdToken VisitIdentifier(Identifier identifier)
        {
            return new UstTokens.IdToken(identifier.Name, GetTextSpan(identifier));
        }

        private UstTokens.Token VisitLiteral(Literal literal)
        {
            TextSpan textSpan = GetTextSpan(literal);

            switch (literal.TokenType)
            {
                case TokenType.BooleanLiteral:
                    return new UstLiterals.BooleanLiteral(literal.BooleanValue, textSpan);
                case TokenType.EOF:
                    // TODO
                    break;
                case TokenType.Identifier:
                    return new UstTokens.IdToken(literal.Raw, textSpan); // ?
                case TokenType.Keyword:
                    return new UstTokens.IdToken(literal.Raw, textSpan); // TODO: keywords support in UST
                case TokenType.NullLiteral:
                    return new UstLiterals.NullLiteral(textSpan);
                case TokenType.NumericLiteral:
                    return new UstLiterals.FloatLiteral(literal.NumericValue, textSpan); // TODO: separate at least int and double literals
                case TokenType.Punctuator:
                    // TODO
                    break;
                case TokenType.StringLiteral:
                    return convertHelper.ConvertString(textSpan);
                case TokenType.RegularExpression:
                    // TODO: maybe add new literal node RegularExpressionLiteral
                    return new UstLiterals.StringLiteral(literal.Regex.Pattern, textSpan);
                case TokenType.Template:
                    // TODO
                    break;
            }

            return null;
        }

        private UstExprs.MemberReferenceExpression VisitMemberExpression(MemberExpression memberExpression)
        {
            var target = VisitExpression(memberExpression.Object);
            var name = VisitExpression(memberExpression.Property);
            return new UstExprs.MemberReferenceExpression(target, name, GetTextSpan(memberExpression));
        }

        private UstExprs.ObjectCreateExpression VisitNewExpression(NewExpression newExpression)
        {
            var target = VisitExpression(newExpression.Callee);
            var argsUst = VisitArguments(newExpression.Arguments);
            return new UstExprs.ObjectCreateExpression(target, argsUst, GetTextSpan(newExpression));
        }

        private UstExprs.Expression VisitObjectExpression(ObjectExpression objectExpression)
        {
            var properties = VisitProperties(objectExpression.Properties);
            return new UstExprs.AnonymousObjectExpression(properties, GetTextSpan(objectExpression));
        }

        private UstSpecific.CommaExpression VisitSequenceExpression(SequenceExpression sequenceExpression)
        {
            var exprs = new Collections.List<UstExprs.Expression>(sequenceExpression.Expressions.Count);

            foreach (Expression expr in sequenceExpression.Expressions)
            {
                exprs.Add(VisitExpression(expr));
            }

            return new UstSpecific.CommaExpression(exprs, GetTextSpan(sequenceExpression));
        }

        private UstTokens.ThisReferenceToken VisitThisExpression(ThisExpression thisExpression)
        {
            return new UstTokens.ThisReferenceToken(GetTextSpan(thisExpression));
        }

        private UstTokens.BaseReferenceToken VisitSuper(Super super)
        {
            return new UstTokens.BaseReferenceToken(GetTextSpan(super));
        }

        private UstExprs.UnaryOperatorExpression VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            UstTokens.UnaryOperator op = UstTokens.UnaryOperator.None;

            switch (unaryExpression.Operator)
            {
                case UnaryOperator.Plus:
                    op = UstTokens.UnaryOperator.Plus;
                    break;
                case UnaryOperator.Minus:
                    op = UstTokens.UnaryOperator.Minus;
                    break;
                case UnaryOperator.BitwiseNot:
                    op = UstTokens.UnaryOperator.BitwiseNot;
                    break;
                case UnaryOperator.Delete:
                    op = UstTokens.UnaryOperator.Delete;
                    break;
                case UnaryOperator.Void:
                    op = UstTokens.UnaryOperator.Void;
                    break;
                case UnaryOperator.TypeOf:
                    op = UstTokens.UnaryOperator.TypeOf;
                    break;
                case UnaryOperator.Increment:
                    op = unaryExpression.Prefix ? UstTokens.UnaryOperator.Increment : UstTokens.UnaryOperator.PostIncrement;
                    break;
                case UnaryOperator.Decrement:
                    op = unaryExpression.Prefix ? UstTokens.UnaryOperator.Decrement : UstTokens.UnaryOperator.PostDecrement;
                    break;
            }

            // TODO: literal text span in Esprima library
            var opLiteral = new UstLiterals.UnaryOperatorLiteral(op, default);
            var expression = VisitExpression(unaryExpression.Argument);

            return new UstExprs.UnaryOperatorExpression(opLiteral, expression, GetTextSpan(unaryExpression));
        }

        private UstExprs.AnonymousMethodExpression VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
        {
            var parameters = VisitParameters(arrowFunctionExpression.Params);
            var body = Visit(arrowFunctionExpression.Body);

            return new UstExprs.AnonymousMethodExpression(parameters, body, GetTextSpan(arrowFunctionExpression));
        }

        private UstExprs.Expression VisitTemplateLiteral(TemplateLiteral templateLiteral)
        {
            var elems = new Collections.List<INode>(templateLiteral.Expressions.Count + templateLiteral.Quasis.Count);

            elems.AddRange(templateLiteral.Expressions);
            elems.AddRange(templateLiteral.Quasis);

            elems.Sort(NodeLocationComparer.Instance);

            UstExprs.Expression result = null;

            for (int i = 0; i < elems.Count; i++)
            {
                UstExprs.Expression expr;

                if (elems[i] is Expression expression)
                {
                    expr = VisitExpression(expression);
                }
                else
                {
                    expr = VisitTemplateElement((TemplateElement)elems[i]);
                }

                var opLiteral = new UstLiterals.BinaryOperatorLiteral(UstTokens.BinaryOperator.Plus,
                    TextSpan.FromBounds(expr.TextSpan.End, expr.TextSpan.End));

                result = result == null
                    ? expr
                    : new UstExprs.BinaryOperatorExpression(result, opLiteral, expr, result.TextSpan.Union(expr.TextSpan));
            }

            return result;
        }

        private UstExprs.Expression VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
        {
            var target = VisitExpression(taggedTemplateExpression.Tag);
            var arg = VisitTemplateLiteral(taggedTemplateExpression.Quasi);
            return new UstExprs.InvocationExpression(target, new ArgsUst(arg), GetTextSpan(taggedTemplateExpression));
        }

        private UstLiterals.StringLiteral VisitTemplateElement(TemplateElement templateElement)
        {
            return new UstLiterals.StringLiteral(templateElement.Value.Cooked, GetTextSpan(templateElement), 0);
        }

        private UstExprs.Expression VisitClassExpression(ClassExpression classExpression)
        {
            var className = classExpression.Id != null
                ? VisitIdentifier(classExpression.Id)
                : null;

            var baseTypes = new Collections.List<UstTokens.TypeToken>();
            if (classExpression.SuperClass != null)
            {
                if (VisitExpression(classExpression.SuperClass) is UstTokens.TypeToken superClassTypeToken)
                {
                    baseTypes.Add(superClassTypeToken);
                }
            }

            var properties = VisitClassBody(classExpression.Body);

            var typeDeclaration = new TypeDeclaration(null, className, properties, GetTextSpan(classExpression))
            {
                BaseTypes = baseTypes
            };

            return typeDeclaration.AsExpression();
        }

        private UstExprs.YieldExpression VisitYieldExpression(YieldExpression yieldExpression)
        {
            var argument = yieldExpression.Argument != null
                ? VisitExpression(yieldExpression.Argument)
                : null;
            return new UstExprs.YieldExpression(argument, GetTextSpan(yieldExpression));
        }

        private UstExprs.MemberReferenceExpression VisitMetaProperty(MetaProperty metaProperty)
        {
            var meta = VisitIdentifier(metaProperty.Meta);
            var property = VisitIdentifier(metaProperty.Property);
            return new UstExprs.MemberReferenceExpression(meta, property, GetTextSpan(metaProperty));
        }

        private ArgsUst VisitArguments(NodeList<ArgumentListElement> arguments)
        {
            var args = new Collections.List<UstExprs.Expression>(arguments.Count);

            foreach (ArgumentListElement arg in arguments)
            {
                // TODO: all cases of type casting
                if (arg is SpreadElement spreadElement)
                {
                    // TODO: spread elements processing
                    Logger.LogDebug("Spread elements are not supported for now");
                }
                else if (arg is Expression expression)
                {
                    args.Add(VisitExpression(expression));
                }
                else
                {
                    // TODO:
                    Logger.LogDebug($"{arg.GetType().Name} are not supported for now");
                }
            }

            return new ArgsUst(args);
        }

        private Collections.List<ParameterDeclaration> VisitParameters(NodeList<INode> parameters)
        {
            var result = new Collections.List<ParameterDeclaration>(parameters.Count);

            foreach (INode param in parameters)
            {
                UstTokens.IdToken name;
                if (param is Identifier identifier)
                {
                    name = VisitIdentifier(identifier);
                }
                else
                {
                    // TODO: extend parameter declaration
                    name = new UstTokens.IdToken("", GetTextSpan(param));
                }
                result.Add(new ParameterDeclaration(null, null, name, name.TextSpan));
            }

            return result;
        }

        private UstExprs.Expression VisitUnknownExpression(Expression expression)
        {
            string message = expression == null
                ? $"{nameof(expression)} can not be null"
                : $"Unknow {nameof(Expression)} type {expression.Type}";
            Logger.LogError(new ConversionException(SourceFile, message: message));
            return null;
        }
    }
}
