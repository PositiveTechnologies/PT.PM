using System.IO;
using System.Linq;
using PT.PM.Common;
using PT.PM.Common.Tests;
using NUnit.Framework;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Ust;

namespace PT.PM.CSharpAstConversion.Tests
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
    }
}
