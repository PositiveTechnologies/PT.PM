using PT.PM.UstPreprocessing;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.TestUtils;
using PT.PM.Patterns.Nodes;
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
    public class UstVisitorTests
    {
        [Test]
        public void Check_IUstVisitor_AllVisitMethodsExists()
        {
            MethodInfo[] iUstVisitorMethods = typeof(IUstVisitor<>).GetMethods();
            IEnumerable<Type> allUstNodeTypes = ListenerVisitorUtils.GetAssemblyUstNodeTypes(typeof(UstNode), typeof(PatternVarDef));
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

        [Test]
        public void Check_AllDescendants_HaveNotParentsAfterConvert()
        {
            WorkflowResult result = TestHelper.CheckFile("AllInOne.cs", Language.CSharp, Stage.Convert);

            IEnumerable<UstNode> descendantsExceptFirst = result.Usts.First().Root.GetAllDescendants();
            foreach (var descendant in descendantsExceptFirst)
            {
                Assert.IsNull(descendant.Parent);
            }
        }

        [Test]
        public void Check_AllDescendants_HaveParentsAfterPreprocess()
        {
            WorkflowResult result = TestHelper.CheckFile("AllInOne.cs", Language.CSharp, Stage.Preprocess);

            IEnumerable<UstNode> descendantsExceptFirst = result.Usts.First().Root.GetAllDescendants().Skip(1);
            foreach (var descendant in descendantsExceptFirst)
            {
                Assert.IsNotNull(descendant.Parent);
            }
        }
    }
}
