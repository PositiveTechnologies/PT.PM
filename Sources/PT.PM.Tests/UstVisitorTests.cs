using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using PT.PM.TestUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PT.PM.Tests
{
    [TestFixture]
    public class UstVisitorTests
    {
        [Test]
        public void Check_IUstVisitor_AllVisitMethodsExists()
        {
            CheckVisitorMethods(typeof(IUstVisitor<>), typeof(Ust));
        }

        [Test]
        public void Check_IUstPatternVisitor_AllVisitMethodsExists()
        {
            CheckVisitorMethods(typeof(IPatternVisitor<>), typeof(PatternUst));
        }

        [Test]
        public void Check_AllDescendants_HaveParentsAfterConvert()
        {
            string fileName = Path.Combine(TestUtility.GrammarsDirectory, "csharp", "not-ready-examples", "AllInOne.cs");
            WorkflowResult result = TestUtility.CheckFile(fileName, Stage.Ust);

            IEnumerable<Ust> descendantsExceptFirst = result.Usts.First().WhereDescendants().Skip(1);
            foreach (var descendant in descendantsExceptFirst)
            {
                if (!(descendant is RootUst))
                {
                    Assert.IsNotNull(descendant.Parent);
                }
            }
        }

        private static void CheckVisitorMethods(Type visitorType, Type baseClass)
        {
            MethodInfo[] iUstVisitorMethods = visitorType.GetMethods();
            IEnumerable<Type> allUstNodeTypes = GetAssemblyUstNodeTypes(visitorType, baseClass);
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

        private static IEnumerable<Type> GetAssemblyUstNodeTypes(Type visitorType, Type baseClass)
        {
            return Assembly.GetAssembly(visitorType)
                .GetTypes()
                .Where(t => t.IsSubclassOf(baseClass) && !t.IsAbstract);
        }
    }
}
