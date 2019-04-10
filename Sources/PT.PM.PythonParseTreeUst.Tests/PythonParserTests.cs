using System.IO;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.TestUtils;

namespace PT.PM.PythonParseTreeUst.Tests
{
    [TestFixture]
    public class PythonParserTests
    {
        [TestCase("python3")]
        public void Parse_Python_Files_WithoutErrors(string examplesFolder)
        {
            TestUtility.CheckProject(Path.Combine(TestUtility.GrammarsDirectory, examplesFolder, "examples"),
                Language.Python, Stage.ParseTree);
        }
        
    }
}