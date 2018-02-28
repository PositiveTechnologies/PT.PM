using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.CSharpParseTreeUst;
using PT.PM.TestUtils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM.Tests
{
    [TestFixture]
    public class SourceCodeRepositoryTests
    {
        [Test]
        public void FileCodeRepository_TestPath_CorrectPathsAndNames()
        {
            string fullName = Path.Combine(TestUtility.TestsDataPath, "Test Project", "1.cs");
            var fileCodeRepository = new FileCodeRepository(fullName);
            IEnumerable<string> fileNames = fileCodeRepository.GetFileNames();

            CodeFile sourceCode = fileCodeRepository.ReadFile(fileNames.Single());

            Assert.AreEqual("1.cs", sourceCode.Name);
            Assert.AreEqual("", sourceCode.RelativePath);
            Assert.AreEqual(Path.GetDirectoryName(fullName), sourceCode.RootPath);
            Assert.AreEqual(fullName, sourceCode.FullName);
        }

        [Test]
        public void AggregateFiles_TestProject_CorrectCountAndRelativePaths()
        {
            string rootPath = Path.Combine(TestUtility.TestsDataPath, "Test Project");
            var repository = new DirectoryCodeRepository(rootPath, CSharp.Language);
            var fileNames = repository.GetFileNames().Select(fileName => repository.ReadFile(fileName)).ToArray();

            Assert.AreEqual(7, fileNames.Length);

            Assert.IsNotNull(fileNames.SingleOrDefault(f =>
                f.Name == "1.cs" && f.RelativePath == "" && f.FullName == Path.Combine(rootPath, "1.cs")));
            Assert.IsNotNull(fileNames.SingleOrDefault(f =>
                f.Name == "1.cs" && f.RelativePath == "Folder" && f.FullName == Path.Combine(rootPath, "Folder", "1.cs")));
        }

        [Test]
        public void Check_AspxFileWithCSharpLanguage_NotIgnored()
        {
            var repository = new DirectoryCodeRepository("", CSharp.Language);

            Assert.IsFalse(repository.IsFileIgnored("page.aspx"));
        }
    }
}
