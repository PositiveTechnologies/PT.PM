using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Ust;
using PT.PM.Patterns;
using PT.PM.TestUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PT.PM.UstPreprocessing.Tests
{
    [TestFixture]
    public class UstVisitorTests
    {
        [Test]
        public void Check_IUstVisitor_AllVisitMethodsExists()
        {
            CheckVisitorMethods(typeof(IUstVisitor<>));
        }

        [Test]
        public void Check_IUstPatternVisitor_AllVisitMethodsExists()
        {
            CheckVisitorMethods(typeof(IUstPatternVisitor<>));
        }

        [Test]
        public void Check_AllDescendants_HaveParentsAfterConvert()
        {
            WorkflowResult result = TestHelper.CheckFile("AllInOne.cs", Language.CSharp, Stage.Convert);

            IEnumerable<UstNode> descendantsExceptFirst = result.Usts.First().GetAllDescendants().Skip(1);
            foreach (var descendant in descendantsExceptFirst)
            {
                if (!(descendant is RootNode))
                {
                    Assert.IsNotNull(descendant.Parent);
                }
            }
        }

        private static IEnumerable<Type> GetAssemblyUstNodeTypes(params Type[] types)
        {
            return types.SelectMany(type => Assembly.GetAssembly(type).GetTypes())
                .Where(t => t.IsSubclassOf(typeof(UstNode)) && !t.IsAbstract);
        }

        private static void CheckVisitorMethods(Type visitorType)
        {
            MethodInfo[] iUstVisitorMethods = visitorType.GetMethods();
            IEnumerable<Type> allUstNodeTypes = GetAssemblyUstNodeTypes(visitorType);
            foreach (Type type in allUstNodeTypes)
            {
                Assert.IsTrue(iUstVisitorMethods
                    .FirstOrDefault(methodInfo =>
                    {
                        var parameters = methodInfo.GetParameters();
                        return parameters.Length > 0 && parameters[0].ParameterType == type;
                    }) != null,
                    $"Visitor for Type {type} is not exists");
            }
        }
    }
}
