using System.IO;
using System.Linq;
using PT.PM.Common.CodeRepository;
using PT.PM.TestUtils;
using NUnit.Framework;
using PT.PM.CSharpParseTreeUst;

namespace PT.PM.Tests
{
    [TestFixture]
    public class SourceCodeRepositoryTests
    {
        [Test]
        public void AggregateFiles_TestProject_CorrectCountAndRelativePaths()
        {
            var repository = new FilesAggregatorCodeRepository(Path.Combine(TestUtility.TestsDataPath, "Test Project"), CSharp.Language);
            var fileNames = repository.GetFileNames().Select(fileName => repository.ReadFile(fileName)).ToArray();

            Assert.AreEqual(7, fileNames.Length);
            Assert.IsNotNull(fileNames.SingleOrDefault(f => f.Name == "1.cs" && f.RelativePath == ""));
            Assert.IsNotNull(fileNames.SingleOrDefault(f => f.Name == "1.cs" && f.RelativePath == "Folder"));
        }

        [Test]
        public void Check_AspxFileWithCSharpLanguage_NotIgnored()
        {
            var repository = new FilesAggregatorCodeRepository("", CSharp.Language);

            Assert.IsFalse(repository.IsFileIgnored("page.aspx"));
        }
    }
}
