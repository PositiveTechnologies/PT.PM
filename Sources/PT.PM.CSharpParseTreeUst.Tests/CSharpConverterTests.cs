using System.Linq;
using PT.PM.Common;
using PT.PM.TestUtils;
using NUnit.Framework;
using PT.PM.Common.Nodes.GeneralScope;
using System.IO;
using System;
using PT.PM.Common.Nodes;

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
            TestUtility.CheckFile(fileName, Stage.Ust, out RootUst ust);
            bool result = ust.AnyDescendantOrSelf(descendant =>
            {
                return descendant is TypeDeclaration typeDeclaration &&
                       typeDeclaration.BaseTypes.Any(type => type.TypeText == nameof(IDisposable));
            });
            Assert.IsTrue(result, "Ust doesn't contain type declaration node with IDisposable base type");
        }

        [Test]
        public void Check_Array_Initializations()
        {
            string fileName = Path.Combine(TestUtility.TestsDataPath, "ArrayExamples.cs");
            TestUtility.CheckFile(fileName, Stage.Ust, out RootUst ust);
        }
    }
}
