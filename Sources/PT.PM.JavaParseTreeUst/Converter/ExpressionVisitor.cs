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
    public partial class JavaAntlrUstConverterVisitor
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
                        result = (ObjectCreateExpression)Visit(context.creator());
                        return result;
                    case JavaParser.LPAREN: // '(' type ')' expression
                        var type = (TypeToken)Visit(context.typeType());
                        target = (Expression)Visit(context.expression(0));
                        result = new CastExpression(type, target, textSpan, FileNode);
                        return result;
                    default: // unary operator ('+', '-', '++', '--', '~', '!')
                        UnaryOperator op = UnaryOperatorLiteral.PrefixTextUnaryOperator[child0Terminal.GetText()];
                        var opLiteral = new UnaryOperatorLiteral(op, child0Terminal.GetTextSpan(), FileNode);
                        target = (Expression)Visit(context.expression(0));
                        result = new UnaryOperatorExpression(opLiteral, target, textSpan, FileNode);
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
                        var id = context.Identifier();
                        if (id != null)
                        {
                            result = new MemberReferenceExpression(target, (IdToken)Visit(id), textSpan, FileNode);
                            return result;
                        }

                        var explicitGenericInvocation = context.explicitGenericInvocation();
                        if (explicitGenericInvocation != null)
                        {
                            // TODO: implement
                            throw new NotImplementedException();
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
                        args = new ArgsNode(new Expression[] { expr }, expr.TextSpan, FileNode);

                        result = new IndexerExpression(target, args, textSpan, FileNode);
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

                        result = new InvocationExpression(target, args, textSpan, FileNode);
                        return result;

                    case JavaParser.INSTANCEOF: // x instanceof y -> (y)x != null
                        var expression = (Expression)Visit(context.expression(0));
                        var type = (TypeToken)Visit(context.typeType());
                        result = new BinaryOperatorExpression
                        {
                            Left = new CastExpression(type, expression, context.GetTextSpan(), FileNode),
                            Operator = new BinaryOperatorLiteral(BinaryOperator.NotEqual, default(TextSpan), FileNode),
                            Right = new NullLiteral(default(TextSpan), FileNode),
                            TextSpan = context.GetTextSpan(),
                            FileNode = FileNode
                        };
                        return result;

                    case JavaParser.QUESTION: // '?'
                        var condition = (Expression)Visit(context.expression(0));
                        var trueExpr = (Expression)Visit(context.expression(1));
                        var falseExpr = (Expression)Visit(context.expression(2));

                        result = new ConditionalExpression(condition, trueExpr, falseExpr, textSpan, FileNode);
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
                                result = new AssignmentExpression(left, right, textSpan, FileNode);
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
                                    context.GetTextSpan(), FileNode);
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
                                var opLiteral = new BinaryOperatorLiteral(op, child1Terminal.GetTextSpan(), FileNode);

                                result = new BinaryOperatorExpression(left, opLiteral, right, textSpan, FileNode);
                            }
                        }
                        else
                        {
                            // post increment or decrement.
                            UnaryOperator op = UnaryOperatorLiteral.PostfixTextUnaryOperator[text];
                            var opLiteral = new UnaryOperatorLiteral(op, child1Terminal.GetTextSpan(), FileNode);

                            result = new UnaryOperatorExpression(opLiteral, left, textSpan, FileNode);
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
                        result = new ThisReferenceToken(textSpan, FileNode);
                        return result;

                    case JavaParser.SUPER:
                        result = new BaseReferenceExpression(textSpan, FileNode);
                        return result;

                    case JavaParser.VOID:
                        var id = new IdToken("TypeOf", ((ITerminalNode)context.GetChild(2)).GetTextSpan(), FileNode);
                        var child0TerminalSpan = child0Terminal.GetTextSpan();
                        result = new InvocationExpression(id, 
                            new ArgsNode(new Expression[] { new NullLiteral(child0TerminalSpan, FileNode) }, child0TerminalSpan, FileNode),
                            textSpan,
                            FileNode);
                        return result;
                }
            }
            
            JavaParser.TypeTypeContext type = context.typeType();
            if (type != null)
            {
                var typeToken = (TypeToken)Visit(type);
                var id = new IdToken("TypeOf", ((ITerminalNode)context.GetChild(2)).GetTextSpan(), FileNode);
                result = new InvocationExpression(id,
                    new ArgsNode(new Expression[] { typeToken }, typeToken.TextSpan, FileNode),
                    textSpan,
                    FileNode);
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

            var result = new TypeToken(resultType, context.GetTextSpan(), FileNode);
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
            var result = new ArgsNode(exprs, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitCreator(JavaParser.CreatorContext context)
        {
            var nonWildcardTypeArguments = context.nonWildcardTypeArguments();
            var typeName = new StringBuilder();
            if (nonWildcardTypeArguments != null)
            {
                var type0 = (TypeToken)Visit(nonWildcardTypeArguments);
                typeName.Append(type0.TypeText);
            }
            var type = (TypeToken)Visit(context.createdName());
            typeName.Append(type.TypeText); 
            JavaParser.ClassCreatorRestContext classCreatorRest = context.classCreatorRest();
            ArgsNode args;
            if (classCreatorRest != null)
                args = (ArgsNode)Visit(classCreatorRest.arguments());
            else
                args = new ArgsNode();
            // TODO: add classBody

            var result = new ObjectCreateExpression(new TypeToken(typeName.ToString(),
                type.TextSpan, FileNode), args,
                context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitCreatedName(JavaParser.CreatedNameContext context)
        {
            var primitiveType = context.primitiveType();
            TypeToken result;
            if (primitiveType != null)
            {
                result = (TypeToken)Visit(primitiveType);
                return result;
            }

            var resultTypeString = new StringBuilder();
            for (int i = 0; i < context.ChildCount; i++)
            {
                var idChild = context.GetChild(i) as ITerminalNode;
                if (idChild != null)
                {
                    resultTypeString.Append(((IdToken)Visit(idChild)).Id);
                    continue;
                }

                var typeChild = context.GetChild(i) as JavaParser.TypeArgumentsOrDiamondContext;
                if (typeChild != null)
                {
                    resultTypeString.Append(((TypeToken)Visit(typeChild)).TypeText);
                    continue;
                }

                resultTypeString.Append(((ITerminalNode)context.GetChild(i)).GetText());
            }

            result = new TypeToken(resultTypeString.ToString(), context.GetTextSpan(), FileNode);
            return result;
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
                context.GetTextSpan(), FileNode);
            return result;
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

            var result = new TypeToken(string.Join(",", types), context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitQualifiedName(JavaParser.QualifiedNameContext context)
        {
            string complexName = string.Join("", context.children.Select(c => c.ToString()).ToArray());
            TextSpan textSpan = context.GetTextSpan();

            var result = new StringLiteral(complexName, textSpan, FileNode);
            return result;
        }

        public UstNode VisitQualifiedNameList(JavaParser.QualifiedNameListContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitPrimitiveType(JavaParser.PrimitiveTypeContext context)
        {
            var name = context.GetChild<ITerminalNode>(0).GetText();
            var result = new TypeToken(name, context.GetTextSpan(), FileNode);
            return result;
        }

        public UstNode VisitLiteral(JavaParser.LiteralContext context)
        {
            var textSpan = context.GetTextSpan();

            ITerminalNode stringLiteral = context.StringLiteral();
            if (stringLiteral != null)
            {
                var text = stringLiteral.GetText();
                return new StringLiteral(text.Substring(1, text.Length - 2), textSpan, FileNode);
            }

            ITerminalNode intLiteral = context.IntegerLiteral();
            if (intLiteral != null)
            {
                return new IntLiteral(long.Parse(intLiteral.GetText().Replace("l", "").Replace("L", "")), textSpan, FileNode);
            }

            ITerminalNode boolLiteral = context.BooleanLiteral();
            if (boolLiteral != null)
            {
                return new BooleanLiteral(bool.Parse(boolLiteral.GetText()), textSpan, FileNode);
            }

            ITerminalNode charLiteral = context.CharacterLiteral();
            if (charLiteral != null)
            {
                return new StringLiteral(charLiteral.GetText(), textSpan, FileNode);
            }
            
            ITerminalNode floatLiteral = context.FloatingPointLiteral();
            if (floatLiteral != null)
            {
                var text = floatLiteral.GetText().ToLowerInvariant().Replace("d", "").Replace("f", "");
                return new FloatLiteral(double.Parse(text), textSpan, FileNode);
            }

            if (context.Start.Type == JavaParser.NullLiteral)
            {
                return new NullLiteral(textSpan, FileNode);
            }

            return VisitChildren(context);
        }
    }
}
