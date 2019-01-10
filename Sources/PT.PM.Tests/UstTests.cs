using NUnit.Framework;
using PT.PM.Common.Nodes;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MessagePack;

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

        [Test]
        public void Check_MessagePackTypes()
        {
            Type ustType = typeof(Ust);
            var ustTypes = Assembly.GetAssembly(ustType)
                .GetTypes()
                .Where(t => t.IsSubclassOf(ustType));
            var abstractTypes = ustTypes.Where(type => type.IsAbstract);

            foreach (Type abstractType in abstractTypes)
            {
                var subclassTypes = ustTypes.Where(type => type.IsSubclassOf(abstractType) && !type.IsAbstract);
                var unionAttrs = abstractType.GetCustomAttributes<UnionAttribute>(false);

                var unionAttrTypes = new List<Type>();
                foreach (UnionAttribute unionAttr in unionAttrs)
                {
                    Assert.AreEqual(unionAttr.SubType.Name,  ((NodeType)unionAttr.Key).ToString(), $"NodeType name does not match to attribute type name");
                    unionAttrTypes.Add(unionAttr.SubType);
                }
                
                CollectionAssert.AreEquivalent(subclassTypes, unionAttrTypes,
                    $"Collections of union attribute types and subclass types are not matched for base class {abstractType.Name}");
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
