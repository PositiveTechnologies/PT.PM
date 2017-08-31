using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.JavaParseTreeUst.Parser;
using PT.PM.AntlrUtils;
using Antlr4.Runtime.Tree;
using System;
using System.Linq;
using System.Text;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.JavaParseTreeUst.Converter
{
    public partial class JavaAntlrParseTreeConverter
    {
        #region Expression base

        public UstNode VisitExpression(JavaParser.ExpressionContext context)
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

            ArgsNode args;
            var child1Terminal = context.GetChild(1) as ITerminalNode;
            if (child1Terminal != null)
            {
                switch (child1Terminal.Symbol.Type)
                {
                    case JavaParser.DOT: // '.'
                        target = (Expression)Visit(context.expression(0));
                        var id = context.IDENTIFIER();
                        if (id != null)
                        {
                            result = new MemberReferenceExpression(target, (IdToken)Visit(id), textSpan);
                            return result;
                        }

                        var explicitGenericInvocation = context.explicitGenericInvocation();
                        if (explicitGenericInvocation != null)
                        {
                            return VisitChildren(context).ToExpressionIfRequired();
                        }

                        var child2Terminal = context.GetChild<ITerminalNode>(1);
                        // TODO: implement
                        switch (child2Terminal.Symbol.Type)
                        {
                            case JavaParser.THIS:
                                break;
                            case JavaParser.NEW:
                                break;
                            case JavaParser.SUPER:
                                break;
                        }
                        break;

                    case JavaParser.LBRACK: // '['
                        target = (Expression)Visit(context.expression(0));
                        Expression expr = (Expression)Visit(context.expression(1));
                        args = new ArgsNode(new Expression[] { expr }, expr.TextSpan);

                        result = new IndexerExpression(target, args, textSpan);
                        return result;

                    case JavaParser.LPAREN: // '('
                        target = (Expression)Visit(context.expression(0));
                        // TODO: fix with ArgsNode
                        JavaParser.ExpressionListContext expressionList = context.expressionList();

                        if (expressionList != null)
                        {
                            args = (ArgsNode)Visit(expressionList);
                        }
                        else
                        {
                            args = new ArgsNode();
                        }

                        result = new InvocationExpression(target, args, textSpan);
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
                                BinaryOperator op;
                                if (text == ">>>=")
                                {
                                    op = BinaryOperator.ShiftRight; // TODO: fix shift operator.
                                }
                                else
                                {
                                    op = BinaryOperatorLiteral.TextBinaryOperator[text.Remove(text.Length - 1)];
                                }

                                result = ConverterHelper.ConvertToAssignmentExpression(left, op, child1Terminal.GetTextSpan(), right,
                                    context.GetTextSpan());
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

            return Visit(context.GetChild(0));
        }

        public UstNode VisitPrimary(JavaParser.PrimaryContext context)
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
                        result = new BaseReferenceExpression(textSpan);
                        return result;

                    case JavaParser.VOID:
                        var id = new IdToken("TypeOf", ((ITerminalNode)context.GetChild(2)).GetTextSpan());
                        var child0TerminalSpan = child0Terminal.GetTextSpan();
                        result = new InvocationExpression(id,
                            new ArgsNode(new Expression[] { new NullLiteral(child0TerminalSpan) }, child0TerminalSpan),
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
                    new ArgsNode(new Expression[] { typeToken }, typeToken.TextSpan),
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

        public UstNode VisitMethodReference(JavaParser.MethodReferenceContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitClassType(JavaParser.ClassTypeContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitParExpression(JavaParser.ParExpressionContext context)
        {
            var result = (Expression)Visit(context.expression());
            return result;
        }

        public UstNode VisitInnerCreator(JavaParser.InnerCreatorContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitNonWildcardTypeArguments(JavaParser.NonWildcardTypeArgumentsContext context)
        {
            var type = (TypeToken)Visit(context.typeList());
            string resultType = context.GetChild<ITerminalNode>(0) + type.TypeText + context.GetChild<ITerminalNode>(1);

            var result = new TypeToken(resultType, context.GetTextSpan());
            return result;
        }

        public UstNode VisitSuperSuffix(JavaParser.SuperSuffixContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitExplicitGenericInvocation(JavaParser.ExplicitGenericInvocationContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitArguments(JavaParser.ArgumentsContext context)
        {
            JavaParser.ExpressionListContext expressionList = context.expressionList();
            if (expressionList != null)
            {
                var result = (ArgsNode)Visit(expressionList);
                return result;
            }
            else
            {
                return new ArgsNode();
            }
        }

        public UstNode VisitExpressionList(JavaParser.ExpressionListContext context)
        {
            Expression[] exprs = context.expression().Select(expr => (Expression)Visit(expr)).ToArray();
            var result = new ArgsNode(exprs, context.GetTextSpan());
            return result;
        }

        public UstNode VisitCreator(JavaParser.CreatorContext context)
        {
            UstNode result;
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
                    var initializers = initializer.Expressions.Where(el => el.NodeType != NodeType.IdToken);
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
                ArgsNode args = (ArgsNode)Visit(classCreatorRest?.arguments()) ?? new ArgsNode();

                // TODO: add classBody

                result = new ObjectCreateExpression(
                    new TypeToken(createdName.GetText(), createdName.GetTextSpan()), args,
                    context.GetTextSpan());
            }
            return result;
        }

        public UstNode VisitCreatedName(JavaParser.CreatedNameContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitTypeArgumentsOrDiamond(JavaParser.TypeArgumentsOrDiamondContext context)
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

        public UstNode VisitTypeTypeOrVoid(JavaParser.TypeTypeOrVoidContext context)
        {
            if (context.typeType() != null)
            {
                return Visit(context.typeType());
            }

            return new TypeToken("void", context.GetTextSpan());
        }

        public UstNode VisitTypeType(JavaParser.TypeTypeContext context)
        {
            var result = (TypeToken)Visit(context.GetChild(0));
            return result;
            //TODO: fix
            /*var lastType = result.Type.Last();
            var terminalNodes = context.children.OfType<ITerminalNode>();
            foreach (var node in terminalNodes)
                lastType += node.Symbol.Text;
            result.Type[result.Type.Count - 1] = lastType;
            return result;*/
        }

        public UstNode VisitExplicitGenericInvocationSuffix(JavaParser.ExplicitGenericInvocationSuffixContext context)
        {
            return VisitChildren(context);
        }

        #endregion

        public UstNode VisitTypeList(JavaParser.TypeListContext context)
        {
            var types = context.typeType().Select(t => ((TypeToken)Visit(t))?.TypeText)
                .Where(t => t != null).ToArray();

            var result = new TypeToken(string.Join(",", types), context.GetTextSpan());
            return result;
        }

        public UstNode VisitQualifiedName(JavaParser.QualifiedNameContext context)
        {
            string complexName = string.Join("", context.children.Select(c => c.ToString()).ToArray());
            TextSpan textSpan = context.GetTextSpan();

            var result = new StringLiteral(complexName, textSpan);
            return result;
        }

        public UstNode VisitQualifiedNameList(JavaParser.QualifiedNameListContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitPrimitiveType(JavaParser.PrimitiveTypeContext context)
        {
            var name = context.GetChild<ITerminalNode>(0).GetText();
            var result = new TypeToken(name, context.GetTextSpan());
            return result;
        }

        public UstNode VisitLiteral(JavaParser.LiteralContext context)
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

            ITerminalNode boolLiteral = context.BOOL_LITERAL();
            if (boolLiteral != null)
            {
                return new BooleanLiteral(bool.Parse(boolLiteral.GetText()), textSpan);
            }

            ITerminalNode charLiteral = context.CHAR_LITERAL();
            if (charLiteral != null)
            {
                return new StringLiteral(charLiteral.GetText(), textSpan);
            }

            ITerminalNode floatLiteral = context.FLOAT_LITERAL();
            if (floatLiteral != null)
            {
                var text = floatLiteral.GetText().ToLowerInvariant().Replace("d", "").Replace("f", "").Replace("_", "");
                return new FloatLiteral(double.Parse(text), textSpan);
            }

            if (context.Start.Type == JavaParser.NULL_LITERAL)
            {
                return new NullLiteral(textSpan);
            }

            return VisitChildren(context);
        }

        public UstNode VisitIntegerLiteral(JavaParser.IntegerLiteralContext context)
        {
            TextSpan textSpan = context.GetTextSpan();
            string text = context.GetText().Replace("_", "");
            return TryParseInteger(text, textSpan) ?? new IntLiteral(0, textSpan);
        }

        public UstNode VisitLambdaExpression(JavaParser.LambdaExpressionContext context)
        {
            return VisitChildren(context).ToExpressionIfRequired();
        }

        public UstNode VisitLambdaParameters(JavaParser.LambdaParametersContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitLambdaBody(JavaParser.LambdaBodyContext context)
        {
            return Visit(context.GetChild(0));
        }
    }
}
