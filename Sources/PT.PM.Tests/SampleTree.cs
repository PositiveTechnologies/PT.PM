using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;

namespace PT.PM.UstPreprocessing.Tests
{
    public static class SampleTree
    {
        public static Ust Init()
        {
            var result = new BlockStatement
            {
                Statements = new List<Statement>()
                {
                    new ExpressionStatement
                    {
                        Expression = new BinaryOperatorExpression
                        {
                            Left = new IdToken("variable"),
                            Operator = new BinaryOperatorLiteral { BinaryOperator = BinaryOperator.Equal },
                            Right = new IntLiteral(42)
                        }
                    },
                    new ExpressionStatement
                    {
                        Expression = new InvocationExpression
                        {
                            Target = new MemberReferenceExpression
                            {
                                Target = new MemberReferenceExpression
                                {
                                    Target = new IdToken("b"),
                                    Name = new IdToken("a")
                                },
                                Name = new IdToken("c"),
                            },
                            Arguments = new ArgsUst(new Expression[]
                            {
                                new BooleanLiteral(true),
                                new StringLiteral("asdf"),
                                new NullLiteral(),
                                new UnaryOperatorExpression
                                {
                                    Operator = new UnaryOperatorLiteral { UnaryOperator = UnaryOperator.Minus },
                                    Expression = new FloatLiteral(1234.5678)
                                }
                            })
                        }
                    },
                    new ForStatement
                    {
                        Initializers = new List<Statement>()
                        {
                            new ExpressionStatement
                            {
                                Expression = new AssignmentExpression
                                {
                                    Left = new IdToken("index"),
                                    Right = new IntLiteral(0)
                                }
                            }
                        },
                        Condition = new BinaryOperatorExpression
                        {
                            Left = new IdToken("index"),
                            Operator =  new BinaryOperatorLiteral(BinaryOperator.Less),
                            Right = new IntLiteral(100)
                        },
                        Iterators = new List<Expression>()
                        {
                            new UnaryOperatorExpression
                            {
                                Operator = new UnaryOperatorLiteral(UnaryOperator.Increment),
                                Expression = new IdToken("index")
                            }
                        },
                        Statement = new IfElseStatement
                        {
                            Condition = new BinaryOperatorExpression
                            {
                                Left = new IdToken("index"),
                                Operator = new BinaryOperatorLiteral(BinaryOperator.Less),
                                Right = new IntLiteral(50)
                            },
                            TrueStatement = new ExpressionStatement
                            {
                                Expression = new InvocationExpression
                                {
                                    Target = new IdToken("print"),
                                    Arguments = new ArgsUst()
                                }
                            },
                            FalseStatement = null
                        }
                    }
                }
            };
            return result;
        }
    }
}
