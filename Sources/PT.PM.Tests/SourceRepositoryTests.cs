using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.SourceRepository;
using PT.PM.Common.Files;
using PT.PM.Common.Utils;
using PT.PM.TestUtils;

namespace PT.PM.Tests
{
    [TestFixture]
    public class SourceRepositoryTests
    {
        [Test]
        public void FileSourceRepository_TestPath_CorrectPathsAndNames()
        {
            string fullName = Path.Combine(TestUtility.TestsDataPath, "Test Project", "1.cs");
            var fileSourceRepository = new FileSourceRepository(fullName);
            IEnumerable<string> fileNames = fileSourceRepository.GetFileNames();

            var source = (TextFile)fileSourceRepository.ReadFile(fileNames.Single());

            Assert.AreEqual("1.cs", source.Name);
            Assert.AreEqual("", source.RelativePath);
            Assert.AreEqual(Path.GetDirectoryName(fullName), source.RootPath);
            Assert.AreEqual(fullName, source.FullName);
        }

        [Test]
        public void AggregateFiles_TestProject_CorrectCountAndRelativePaths()
        {
            string rootPath = Path.Combine(TestUtility.TestsDataPath, "Test Project");
            var repository = new DirectorySourceRepository(rootPath, languages: Language.CSharp);
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
            var repository = new DirectorySourceRepository("", Language.CSharp);

            CollectionAssert.IsNotEmpty(repository.GetLanguages("page.aspx", true));
        }

        [Test]
        public void Check_CorrectLongestCommonPaths()
        {
            string path = FileSourceRepository.GetLongestCommonPath(new string[0]);
            Assert.AreEqual(@"", path);

            path = FileSourceRepository.GetLongestCommonPath(new[] { "" });
            Assert.AreEqual(@"", path);

            path = FileSourceRepository.GetLongestCommonPath(new[] { @"C:\dir\dir1\file1".NormalizeDirSeparator() });
            Assert.AreEqual(@"C:\dir\dir1".NormalizeDirSeparator(), path);

            path = FileSourceRepository.GetLongestCommonPath(new[] { @"C:\dir\dir1\file1".NormalizeDirSeparator(), @"C:\dir\dir1\file2".NormalizeDirSeparator() });
            Assert.AreEqual(@"C:\dir\dir1".NormalizeDirSeparator(), path);

            path = FileSourceRepository.GetLongestCommonPath(new[] { @"C:\dir\dir1\abc".NormalizeDirSeparator(), @"C:\dir\dir1\bcd".NormalizeDirSeparator() });
            Assert.AreEqual(@"C:\dir\dir1".NormalizeDirSeparator(), path);

            path = FileSourceRepository.GetLongestCommonPath(new[] { @"C:\dir\dir1\file1".NormalizeDirSeparator(), @"D:\dir\dir1\file2".NormalizeDirSeparator() });
            Assert.AreEqual(@"", path);
        }
    }
}
