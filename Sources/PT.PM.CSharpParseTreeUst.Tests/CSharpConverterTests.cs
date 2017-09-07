using System.Linq;
using PT.PM.Common;
using PT.PM.TestUtils;
using NUnit.Framework;
using PT.PM.Common.Nodes.GeneralScope;

namespace PT.PM.CSharpParseTreeUst.Tests
{
    [TestFixture]
    public class CSharpConverterTests
    {
        [TestCase("AllInOne.cs")]
        [TestCase("ConvertError.cs")]
        public void Convert_CSharp_WithoutErrors(string fileName)
        {
            TestHelper.CheckFile(fileName, Language.CSharp, Stage.Convert);
        }

        [Test]
        public void Convert_WebGoatNet_WithoutException()
        {
            string projectKey = "WebGoat.NET-1c6cab";
            TestHelper.CheckProject(
                TestProjects.CSharpProjects.Single(p => p.Key == projectKey), Language.CSharp, Stage.Convert);
        }

        [TestCase("AllInOne.cs")]
        public void Convert_CSharp_BaseTypesExist(string fileName)
        {
            var workflowResults = TestHelper.CheckFile(fileName, Language.CSharp, Stage.Convert);
            var ust = workflowResults.Usts.First();
            bool result = ust.Root.DoesAnyDescendantMatchPredicate(el =>
            {
                bool isTypeDeclaration = el.NodeType == Common.Nodes.NodeType.TypeDeclaration;
                return isTypeDeclaration && ((TypeDeclaration)el).BaseTypes.Any(t => t.TypeText == "IDisposable");
            });
            Assert.IsTrue(result, "Ust doesn't contain type declaration node with IDisposable base type");
        }
    }
}
