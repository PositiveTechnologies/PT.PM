using System.Linq;
using PT.PM.Common;
using PT.PM.TestUtils;
using NUnit.Framework;
using PT.PM.Common.Nodes.GeneralScope;
using System.IO;
using System;

namespace PT.PM.CSharpParseTreeUst.Tests
{
    [TestFixture]
    public class CSharpConverterTests
    {
        [Test]
        public void Convert_CSharpAllInOne_WithoutErrors()
        {
            TestUtility.CheckFile(Path.Combine(TestUtility.GrammarsDirectory, "csharp", "not-ready-examples", "AllInOne.cs"), Stage.Ust);
        }

        [Test]
        public void Convert_CSharp_BaseTypesExist()
        {
            string fileName = Path.Combine(TestUtility.GrammarsDirectory, "csharp", "not-ready-examples", "AllInOne.cs");
            var workflowResults = TestUtility.CheckFile(fileName, Stage.Ust);
            var ust = workflowResults.Usts.First();
            bool result = ust.AnyDescendant(descendant =>
            {
                return descendant is TypeDeclaration typeDeclaration &&
                       typeDeclaration.BaseTypes.Any(type => type.TypeText == nameof(IDisposable));
            });
            Assert.IsTrue(result, "Ust doesn't contain type declaration node with IDisposable base type");
        }
    }
}
