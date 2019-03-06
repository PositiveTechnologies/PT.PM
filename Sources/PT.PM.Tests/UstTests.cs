using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PT.PM.Common.Nodes;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;

namespace PT.PM.Tests
{
    [TestFixture]
    public class UstTests
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
                    $"Visitor for Type {type} does not exist");
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
