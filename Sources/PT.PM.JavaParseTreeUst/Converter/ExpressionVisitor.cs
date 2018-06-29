using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System;
using System.Linq;

namespace PT.PM.JavaParseTreeUst.Converter
{
    public partial class JavaAntlrParseTreeConverter
    {
        #region Expression base

        public Ust VisitExpression(JavaParser.ExpressionContext context)
        {
            var textSpan = context.GetTextSpan();
            var child0Terminal = context.GetChild(0) as ITerminalNode;
            Expression target;
            Expression result;
            if (child0Terminal != null)
            {
                switch (child0Terminal.Symbol.Type)
                {
                    case JavaParser.NEW:
                        result = (Expression)Visit(context.creator());
                        return result;
                    case JavaParser.LPAREN: // '(' type ')' expression
                        var type = (TypeToken)Visit(context.typeType());
                        target = (Expression)Visit(context.expression(0));
                        result = new CastExpression(type, target, textSpan);
                        return result;
                    default: // unary operator ('+', '-', '++', '--', '~', '!')
                        UnaryOperator op = UnaryOperatorLiteral.PrefixTextUnaryOperator[child0Terminal.GetText()];
                        var opLiteral = new UnaryOperatorLiteral(op, child0Terminal.GetTextSpan());
                        target = (Expression)Visit(context.expression(0));
                        result = new UnaryOperatorExpression(opLiteral, target, textSpan);
                        return result;
                }
            }

            ArgsUst args;
            var child1Terminal = context.GetChild(1) as ITerminalNode;
            if (child1Terminal != null)
            {
                switch (child1Terminal.Symbol.Type)
                {
                    case JavaParser.DOT: // '.'
                        target = (Expression)Visit(context.expression(0));

                        if (context.methodCall() != null)
                        {
                            var invocation = (InvocationExpression)Visit(context.methodCall());
                            return new InvocationExpression(
                                new MemberReferenceExpression(target, invocation.Target, target.TextSpan.Union(invocation.TextSpan)),
                                invocation.Arguments,
                                textSpan);
                        }

                        // TODO: implement base processing

                        Expression rightPart = null;
                        if (context.IDENTIFIER() != null)
                        {
                            rightPart = (IdToken)Visit(context.IDENTIFIER());
                        }
                        else if (context.THIS() != null)
                        {
                            rightPart = new ThisReferenceToken(context.THIS().GetTextSpan());
                        }

                        if (rightPart != null)
                        {
                            return new MemberReferenceExpression(target, rightPart, textSpan);
                        }

                        if (context.innerCreator() != null)
                        {
                            return Visit(context.innerCreator());
                        }

                        if (context.explicitGenericInvocation() != null)
                        {
                            return VisitChildren(context.explicitGenericInvocation()).ToExpressionIfRequired();
                        }

                        break;

                    case JavaParser.LBRACK: // '['
                        target = (Expression)Visit(context.expression(0));
                        Expression expr = (Expression)Visit(context.expression(1));
                        args = new ArgsUst(new Expression[] { expr }, expr.TextSpan);

                        result = new IndexerExpression(target, args, textSpan);
                        return result;

                    case JavaParser.INSTANCEOF: // x instanceof y -> (y)x != null
                        var expression = (Expression)Visit(context.expression(0));
                        var type = (TypeToken)Visit(context.typeType());
                        result = new BinaryOperatorExpression
                        {
                            Left = new CastExpression(type, expression, context.GetTextSpan()),
                            Operator = new BinaryOperatorLiteral(BinaryOperator.NotEqual, default(TextSpan)),
                            Right = new NullLiteral(default(TextSpan)),
                            TextSpan = context.GetTextSpan(),
                            Root = root
                        };
                        return result;

                    case JavaParser.QUESTION: // '?'
                        var condition = (Expression)Visit(context.expression(0));
                        var trueExpr = (Expression)Visit(context.expression(1));
                        var falseExpr = (Expression)Visit(context.expression(2));

                        result = new ConditionalExpression(condition, trueExpr, falseExpr, textSpan);
                        return result;

                    case JavaParser.COLONCOLON:
                        return VisitChildren(context);

                    default: // binary operator
                        string text = child1Terminal.GetText();
                        var left = (Expression)Visit(context.expression(0));
                        JavaParser.ExpressionContext expr1 = context.expression(1);
                        if (expr1 != null)
                        {
                            var right = (Expression)Visit(expr1);

                            if (text == "=")
                            {
                                result = new AssignmentExpression(left, right, textSpan);
                            }
                            else if (BinaryOperatorLiteral.TextBinaryAssignmentOperator.Contains(text))
                            {
                                BinaryOperator? op;
                                if (text == ">>>=")
                                {
                                    op = BinaryOperator.ShiftRight; // TODO: fix shift operator.
                                }
                                else
                                {
                                    op = null;
                                }

                                result = new AssignmentExpression(left, right, context.GetTextSpan())
                                {
                                    BinaryOperator = op.HasValue
                                    ? new BinaryOperatorLiteral((BinaryOperator)op, child1Terminal.GetTextSpan())
                                    : ConvertToBinaryOperatorLiteral(text, child1Terminal.GetTextSpan())
                                };
                            }
                            else
                            {
                                BinaryOperator op;
                                if (text == ">>>")
                                {
                                    op = BinaryOperator.ShiftRight;  // TODO: fix shift operator.
                                }
                                else
                                {
                                    op = BinaryOperatorLiteral.TextBinaryOperator[text];
                                }
                                var opLiteral = new BinaryOperatorLiteral(op, child1Terminal.GetTextSpan());

                                result = new BinaryOperatorExpression(left, opLiteral, right, textSpan);
                            }
                        }
                        else
                        {
                            // post increment or decrement.
                            UnaryOperator op = UnaryOperatorLiteral.PostfixTextUnaryOperator[text];
                            var opLiteral = new UnaryOperatorLiteral(op, child1Terminal.GetTextSpan());

                            result = new UnaryOperatorExpression(opLiteral, left, textSpan);
                            return result;
                        }
                        return result;
                }
            }

            return VisitChildren(context);
        }

        public Ust VisitPrimary(JavaParser.PrimaryContext context)
        {
            TextSpan textSpan = context.GetTextSpan();
            var child0Terminal = context.GetChild(0) as ITerminalNode;
            Expression result;
            if (child0Terminal != null)
            {
                switch (child0Terminal.Symbol.Type)
                {
                    case JavaParser.LPAREN:
                        result = (Expression)Visit(context.expression());
                        return result;

                    case JavaParser.THIS:
                        result = new ThisReferenceToken(textSpan);
                        return result;

                    case JavaParser.SUPER:
                        result = new BaseReferenceToken(textSpan);
                        return result;

                    case JavaParser.VOID:
                        var id = new IdToken("TypeOf", ((ITerminalNode)context.GetChild(2)).GetTextSpan());
                        var child0TerminalSpan = child0Terminal.GetTextSpan();
                        result = new InvocationExpression(id,
                            new ArgsUst(new Expression[] { new NullLiteral(child0TerminalSpan) }, child0TerminalSpan),
                            textSpan);
                        return result;
                }
            }

            JavaParser.TypeTypeOrVoidContext type = context.typeTypeOrVoid();
            if (type != null)
            {
                var typeToken = (TypeToken)Visit(type);
                var id = new IdToken("TypeOf", ((ITerminalNode)context.GetChild(2)).GetTextSpan());
                result = new InvocationExpression(id,
                    new ArgsUst(new Expression[] { typeToken }, typeToken.TextSpan),
                    textSpan);
                return result;
            }

            JavaParser.NonWildcardTypeArgumentsContext args = context.nonWildcardTypeArguments();
            if (args != null)
            {
                var typeToken = (TypeToken)Visit(args);

                throw new NotImplementedException();
            }

            return Visit(context.GetChild(0));
        }

        public Ust VisitClassType(JavaParser.ClassTypeContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitParExpression(JavaParser.ParExpressionContext context)
        {
            var result = (Expression)Visit(context.expression());
            return result;
        }

        public Ust VisitInnerCreator(JavaParser.InnerCreatorContext context)
        {
            ArgsUst args = (ArgsUst)Visit(context.classCreatorRest().arguments());
            return new ObjectCreateExpression(
                    new TypeToken(context.IDENTIFIER().GetText(), context.IDENTIFIER().GetTextSpan()), args,
                    context.GetTextSpan());
        }

        public Ust VisitNonWildcardTypeArguments(JavaParser.NonWildcardTypeArgumentsContext context)
        {
            var type = (TypeToken)Visit(context.typeList());
            string resultType = context.GetChild<ITerminalNode>(0) + type.TypeText + context.GetChild<ITerminalNode>(1);

            var result = new TypeToken(resultType, context.GetTextSpan());
            return result;
        }

        public Ust VisitSuperSuffix(JavaParser.SuperSuffixContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitExplicitGenericInvocation(JavaParser.ExplicitGenericInvocationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitArguments(JavaParser.ArgumentsContext context)
        {
            JavaParser.ExpressionListContext expressionList = context.expressionList();
            if (expressionList != null)
            {
                var result = (ArgsUst)Visit(expressionList);
                return result;
            }
            else
            {
                return new ArgsUst();
            }
        }

        public Ust VisitExpressionList(JavaParser.ExpressionListContext context)
        {
            Expression[] exprs = context.expression().Select(expr => (Expression)Visit(expr)).ToArray();
            var result = new ArgsUst(exprs, context.GetTextSpan());
            return result;
        }

        public Ust VisitCreator(JavaParser.CreatorContext context)
        {
            Ust result;
            JavaParser.CreatedNameContext createdName = context.createdName();
            JavaParser.ArrayCreatorRestContext arrayCreatorRest = context.arrayCreatorRest();
            if (arrayCreatorRest != null)
            {
                var initializer = (MultichildExpression)Visit(arrayCreatorRest.arrayInitializer());
                if (initializer != null)
                {
                    // new int[] { 1, 2 };
                    int dimensions = (arrayCreatorRest.ChildCount - 1) / 2; // number of square bracket pairs
                    var sizes = Enumerable.Range(0, dimensions).Select(
                        _ => new IntLiteral(0, createdName.GetTextSpan())).ToList<Expression>();
                    var initializers = initializer.Expressions.Where(el => !(el is IdToken));
                    result = new ArrayCreationExpression(
                        new TypeToken(createdName.GetText(), createdName.GetTextSpan()), sizes,
                        initializers, context.GetTextSpan());
                }
                else
                {
                    // new int[3][4][];
                    int dimensions = (arrayCreatorRest.ChildCount - arrayCreatorRest.expression().Length) / 2;
                    var sizes = Enumerable.Range(0, dimensions).Select(
                        i => i < arrayCreatorRest.expression().Length ?
                                (Expression)Visit(arrayCreatorRest.expression(i)) :
                                new IntLiteral(0, createdName.GetTextSpan())).ToList();
                    result = new ArrayCreationExpression(
                        new TypeToken(createdName.GetText(), createdName.GetTextSpan()), sizes,
                        null, context.GetTextSpan());
                }
            }
            else
            {
                JavaParser.ClassCreatorRestContext classCreatorRest = context.classCreatorRest();
                ArgsUst args = (ArgsUst)Visit(classCreatorRest?.arguments()) ?? new ArgsUst();

                // TODO: add classBody

                result = new ObjectCreateExpression(
                    new TypeToken(createdName.GetText(), createdName.GetTextSpan()), args,
                    context.GetTextSpan());
            }
            return result;
        }

        public Ust VisitCreatedName(JavaParser.CreatedNameContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTypeArgumentsOrDiamond(JavaParser.TypeArgumentsOrDiamondContext context)
        {
            JavaParser.TypeArgumentsContext typeArguments = context.typeArguments();
            TypeToken result;
            if (typeArguments != null)
            {
                result = (TypeToken)Visit(typeArguments);
                return result;
            }

            result = new TypeToken(context.GetChild<ITerminalNode>(0).GetText() + context.GetChild<ITerminalNode>(1).GetText(),
                context.GetTextSpan());
            return result;
        }

        public Ust VisitTypeTypeOrVoid(JavaParser.TypeTypeOrVoidContext context)
        {
            if (context.typeType() != null)
            {
                return Visit(context.typeType());
            }

            return new TypeToken("void", context.GetTextSpan());
        }

        public Ust VisitTypeType(JavaParser.TypeTypeContext context)
        {
            if (context.classOrInterfaceType() != null)
            {
                return Visit(context.classOrInterfaceType());
            }

            return Visit(context.primitiveType());
        }

        public Ust VisitExplicitGenericInvocationSuffix(JavaParser.ExplicitGenericInvocationSuffixContext context)
        {
            return VisitChildren(context);
        }

        #endregion

        public Ust VisitTypeList(JavaParser.TypeListContext context)
        {
            var types = context.typeType().Select(t => ((TypeToken)Visit(t))?.TypeText)
                .Where(t => t != null).ToArray();

            var result = new TypeToken(string.Join(",", types), context.GetTextSpan());
            return result;
        }

        public Ust VisitQualifiedName(JavaParser.QualifiedNameContext context)
        {
            string complexName = string.Join("", context.children.Select(c => c.ToString()).ToArray());
            TextSpan textSpan = context.GetTextSpan();

            var result = new StringLiteral(complexName, textSpan);
            return result;
        }

        public Ust VisitQualifiedNameList(JavaParser.QualifiedNameListContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitPrimitiveType(JavaParser.PrimitiveTypeContext context)
        {
            var name = context.GetChild<ITerminalNode>(0).GetText();
            var result = new TypeToken(name, context.GetTextSpan());
            return result;
        }

        public Ust VisitLiteral(JavaParser.LiteralContext context)
        {
            var textSpan = context.GetTextSpan();

            ITerminalNode stringLiteral = context.STRING_LITERAL();
            if (stringLiteral != null)
            {
                string text = stringLiteral.GetText();
                return new StringLiteral(text.Substring(1, text.Length - 2), textSpan);
            }

            if (context.integerLiteral() != null)
            {
                return Visit(context.integerLiteral());
            }

            if (context.floatLiteral() != null)
            {
                return Visit(context.floatLiteral());
            }

            ITerminalNode boolLiteral = context.BOOL_LITERAL();
            if (boolLiteral != null)
            {
                return new BooleanLiteral(bool.Parse(boolLiteral.GetText()), textSpan);
            }

            ITerminalNode charLiteral = context.CHAR_LITERAL();
            if (charLiteral != null)
            {
                string text = charLiteral.GetText();
                return new StringLiteral(text.Substring(1, text.Length - 2), textSpan);
            }

            if (context.Start.Type == JavaParser.NULL_LITERAL)
            {
                return new NullLiteral(textSpan);
            }

            return VisitChildren(context);
        }

        public Ust VisitIntegerLiteral(JavaParser.IntegerLiteralContext context)
        {
            TextSpan textSpan = context.GetTextSpan();
            string text = context.GetText().Replace("_", "");
            return TryParseInteger(text, textSpan) ?? new IntLiteral(0, textSpan);
        }

        public Ust VisitFloatLiteral(JavaParser.FloatLiteralContext context)
        {
            string literalText = context.GetText().ToLowerInvariant().Replace("d", "").Replace("f", "").Replace("_", "");
            TextSpan textSpan = context.GetTextSpan();

            ITerminalNode floatLiteral = context.FLOAT_LITERAL();
            if (floatLiteral != null)
            {
                return new FloatLiteral(double.Parse(literalText), textSpan);
            }

            literalText = literalText.Replace("0x", "");
            string[] parts = literalText.Split('p');

            string significandString = parts[0];
            significandString.Replace(".", "").TryConvertToInt64(16, out long significand);
            double result = significand;
            int dotIndex = significandString.LastIndexOf('.');
            if (dotIndex != -1)
            {
                result = result / Math.Pow(16, significandString.Length - dotIndex - 1);
            }

            parts[1].TryConvertToInt64(10, out long exp);

            result = result * Math.Pow(2, exp);

            return new FloatLiteral(result, textSpan);
        }

        public Ust VisitLambdaExpression(JavaParser.LambdaExpressionContext context)
        {
            return VisitChildren(context).ToExpressionIfRequired();
        }

        public Ust VisitLambdaParameters(JavaParser.LambdaParametersContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLambdaBody(JavaParser.LambdaBodyContext context)
        {
            return Visit(context.GetChild(0));
        }
    }
}