using System.IO;
using System.Linq;
using PT.PM.Common.CodeRepository;
using PT.PM.TestUtils;
using NUnit.Framework;

namespace PT.PM.Tests
{
    [TestFixture]
    public class SourceCodeRepositoryTests
    {
        [Test]
        public void AggregateFiles_TestProject_CorrectCountAndRelativePaths()
        {
            var repository = new FilesAggregatorCodeRepository(Path.Combine(TestHelper.TestsDataPath, "Test Project"), ".cs");
            var fileNames = repository.GetFileNames().Select(fileName => repository.ReadFile(fileName)).ToArray();

            Assert.AreEqual(7, fileNames.Length);
            Assert.IsNotNull(fileNames.SingleOrDefault(f => f.Name == "1.cs" && f.RelativePath == ""));
            Assert.IsNotNull(fileNames.SingleOrDefault(f => f.Name == "1.cs" && f.RelativePath == "Folder"));
        }
    }
}
