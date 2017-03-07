using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Patterns.Nodes;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.UstPreprocessing.Tests
{
    [TestFixture]
    public class UstListenerTests
    {
        private const string Enter = "Enter";
        private const string Exit = "Exit";

        private static string[] BeginExpectedSequence = new string[]
        {
            Enter + nameof(BlockStatement),
                Enter + nameof(ExpressionStatement),
                    Enter + nameof(BinaryOperatorExpression),
                        Enter + nameof(IdToken),
                        Exit + nameof(IdToken),
                        Enter + nameof(IntLiteral),
                        Exit + nameof(IntLiteral),
                    Exit + nameof(BinaryOperatorExpression),
                Exit + nameof(ExpressionStatement),
        };

        private static string[] EndExpectedSequence = new string[]
        {
                                    Enter + nameof(IdToken),
                                Exit + nameof(IdToken),
                            Exit + nameof(InvocationExpression),
                        Exit + nameof(ExpressionStatement),
                    Exit + nameof(IfElseStatement),
                Exit + nameof(ForStatement),
            Exit + nameof(BlockStatement)
        };

        [Test]
        public void Check_IUstListener_AllEnterExitMethodsExists()
        {
            MethodInfo[] listenerMethods = typeof(IUstListener).GetMethods();
            IEnumerable<Type> allAstNodeTypes = ListenerVisitorUtils.GetAssemblyAstNodeTypes(typeof(UstNode), typeof(PatternVarDef));
            foreach (Type type in allAstNodeTypes)
            {
                Assert.IsTrue(listenerMethods
                    .FirstOrDefault(methodInfo =>
                    {
                        var parameters = methodInfo.GetParameters();
                        return methodInfo.Name == Enter && parameters.Length > 0 && parameters[0].ParameterType == type;
                    }) != null,
                    $"Enter method for Type {type} is not exists");

                Assert.IsTrue(listenerMethods
                    .FirstOrDefault(methodInfo =>
                    {
                        var parameters = methodInfo.GetParameters();
                        return methodInfo.Name == Exit && parameters.Length > 0 && parameters[0].ParameterType == type;
                    }) != null,
                    $"Visitor for Type {type} is not exists");
            }
        }

        [Test]
        public void Check_DynamicListener_ExpectedEnterExitMethodsVisited()
        {
            var invokesSequence = new List<string>();
            var sampleTree = SampleTree.Init();
            var mock = new Mock<UstListener>();

            mock.Setup(listener => listener.Enter(It.IsAny<BinaryOperatorExpression>())).Callback((BinaryOperatorExpression s) => invokesSequence.Add(Enter + nameof(BinaryOperatorExpression)));
            mock.Setup(listener => listener.Enter(It.IsAny<InvocationExpression>())).Callback((InvocationExpression s) => invokesSequence.Add(Enter + nameof(InvocationExpression)));
            mock.Setup(listener => listener.Enter(It.IsAny<MemberReferenceExpression>())).Callback((MemberReferenceExpression s) => invokesSequence.Add(Enter + nameof(MemberReferenceExpression)));
            mock.Setup(listener => listener.Enter(It.IsAny<UnaryOperatorExpression>())).Callback((UnaryOperatorExpression s) => invokesSequence.Add(Enter + nameof(UnaryOperatorExpression)));
            mock.Setup(listener => listener.Enter(It.IsAny<BooleanLiteral>())).Callback((BooleanLiteral s) => invokesSequence.Add(Enter + nameof(BooleanLiteral)));
            mock.Setup(listener => listener.Enter(It.IsAny<FloatLiteral>())).Callback((FloatLiteral s) => invokesSequence.Add(Enter + nameof(FloatLiteral)));
            mock.Setup(listener => listener.Enter(It.IsAny<IntLiteral>())).Callback((IntLiteral s) => invokesSequence.Add(Enter + nameof(IntLiteral)));
            mock.Setup(listener => listener.Enter(It.IsAny<NullLiteral>())).Callback((NullLiteral s) => invokesSequence.Add(Enter + nameof(NullLiteral)));
            mock.Setup(listener => listener.Enter(It.IsAny<StringLiteral>())).Callback((StringLiteral s) => invokesSequence.Add(Enter + nameof(StringLiteral)));
            mock.Setup(listener => listener.Enter(It.IsAny<IdToken>())).Callback((IdToken s) => invokesSequence.Add(Enter + nameof(IdToken)));
            mock.Setup(listener => listener.Enter(It.IsAny<BlockStatement>())).Callback((BlockStatement s) => invokesSequence.Add(Enter + nameof(BlockStatement)));
            mock.Setup(listener => listener.Enter(It.IsAny<ExpressionStatement>())).Callback((ExpressionStatement s) => invokesSequence.Add(Enter + nameof(ExpressionStatement)));
            mock.Setup(listener => listener.Enter(It.IsAny<ForStatement>())).Callback((ForStatement s) => invokesSequence.Add(Enter + nameof(ForStatement)));
            mock.Setup(listener => listener.Enter(It.IsAny<IfElseStatement>())).Callback((IfElseStatement s) => invokesSequence.Add(Enter + nameof(IfElseStatement)));

            mock.Setup(listener => listener.Exit(It.IsAny<BinaryOperatorExpression>())).Callback((BinaryOperatorExpression s) => invokesSequence.Add(Exit + nameof(BinaryOperatorExpression)));
            mock.Setup(listener => listener.Exit(It.IsAny<InvocationExpression>())).Callback((InvocationExpression s) => invokesSequence.Add(Exit + nameof(InvocationExpression)));
            mock.Setup(listener => listener.Exit(It.IsAny<MemberReferenceExpression>())).Callback((MemberReferenceExpression s) => invokesSequence.Add(Exit + nameof(MemberReferenceExpression)));
            mock.Setup(listener => listener.Exit(It.IsAny<UnaryOperatorExpression>())).Callback((UnaryOperatorExpression s) => invokesSequence.Add(Exit + nameof(UnaryOperatorExpression)));
            mock.Setup(listener => listener.Exit(It.IsAny<BooleanLiteral>())).Callback((BooleanLiteral s) => invokesSequence.Add(Exit + nameof(BooleanLiteral)));
            mock.Setup(listener => listener.Exit(It.IsAny<FloatLiteral>())).Callback((FloatLiteral s) => invokesSequence.Add(Exit + nameof(FloatLiteral)));
            mock.Setup(listener => listener.Exit(It.IsAny<IntLiteral>())).Callback((IntLiteral s) => invokesSequence.Add(Exit + nameof(IntLiteral)));
            mock.Setup(listener => listener.Exit(It.IsAny<NullLiteral>())).Callback((NullLiteral s) => invokesSequence.Add(Exit + nameof(NullLiteral)));
            mock.Setup(listener => listener.Exit(It.IsAny<StringLiteral>())).Callback((StringLiteral s) => invokesSequence.Add(Exit + nameof(StringLiteral)));
            mock.Setup(listener => listener.Exit(It.IsAny<IdToken>())).Callback((IdToken s) => invokesSequence.Add(Exit + nameof(IdToken)));
            mock.Setup(listener => listener.Exit(It.IsAny<BlockStatement>())).Callback((BlockStatement s) => invokesSequence.Add(Exit + nameof(BlockStatement)));
            mock.Setup(listener => listener.Exit(It.IsAny<ExpressionStatement>())).Callback((ExpressionStatement s) => invokesSequence.Add(Exit + nameof(ExpressionStatement)));
            mock.Setup(listener => listener.Exit(It.IsAny<ForStatement>())).Callback((ForStatement s) => invokesSequence.Add(Exit + nameof(ForStatement)));
            mock.Setup(listener => listener.Exit(It.IsAny<IfElseStatement>())).Callback((IfElseStatement s) => invokesSequence.Add(Exit + nameof(IfElseStatement)));

            mock.Object.Walk(sampleTree);

            var actualSequence = invokesSequence.Take(BeginExpectedSequence.Length);
            CollectionAssert.AreEqual(BeginExpectedSequence, actualSequence);

            actualSequence = invokesSequence.Skip(invokesSequence.Count() - EndExpectedSequence.Length);
            CollectionAssert.AreEqual(EndExpectedSequence, actualSequence);
        }
    }
}
