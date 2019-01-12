using PT.PM.Cli.Tests;
using System.IO;
using NUnit.Framework;
using PT.PM.Common.Utils;

namespace PT.PM.Net.Tests
{
    [TestFixture]
    public class LongPathTests
    {
        [Test]
        public void Process_LongDirPath_ThrowException()
        {
            string tempDir = Path.GetTempPath();
            tempDir = tempDir.EndsWith("\\") ? tempDir : tempDir + '\\';
            string shortDirPath = Path.Combine(tempDir, new string('a', PathUtils.MaxDirLength - tempDir.Length));
            Assert.AreEqual(PathUtils.MaxDirLength, shortDirPath.Length);
            Assert.AreEqual(shortDirPath, shortDirPath.NormalizeDirPath());

            try
            {
                Directory.CreateDirectory(shortDirPath);
            }
            finally
            {
                if (Directory.Exists(shortDirPath))
                {
                    Directory.Delete(shortDirPath);
                }
            }

            string longDirPath = Path.Combine(tempDir, new string('a', PathUtils.MaxDirLength - tempDir.Length + 1));
            Assert.AreEqual(PathUtils.MaxDirLength + 1, longDirPath.Length);
            Assert.Throws<PathTooLongException>(() => Directory.CreateDirectory(longDirPath));

            string normalizedLongDirPath = longDirPath.NormalizeDirPath();
            Assert.IsTrue(normalizedLongDirPath.StartsWith(@"\\?\"));

            try
            {
                Directory.CreateDirectory(normalizedLongDirPath);
            }
            finally
            {
                if (Directory.Exists(normalizedLongDirPath))
                {
                    Directory.Delete(normalizedLongDirPath);
                }
            }
        }

        [Test]
        public void Process_LongFilePath_ThrowException()
        {
            string tempDir = Path.GetTempPath();
            tempDir = tempDir.EndsWith("\\") ? tempDir : tempDir + '\\';
            string shortFilePath = Path.Combine(tempDir, new string('a', PathUtils.MaxPathLength - tempDir.Length));
            Assert.AreEqual(PathUtils.MaxPathLength, shortFilePath.Length);
            Assert.AreEqual(shortFilePath, shortFilePath.NormalizeFilePath());

            try
            {
                File.WriteAllText(shortFilePath, "");
            }
            finally
            {
                if (File.Exists(shortFilePath))
                {
                    File.Delete(shortFilePath);
                }
            }

            string longFilePath = Path.Combine(tempDir, new string('a', PathUtils.MaxPathLength - tempDir.Length + 1));
            Assert.AreEqual(PathUtils.MaxPathLength + 1, longFilePath.Length);

            Assert.Throws<DirectoryNotFoundException>(() => File.WriteAllText(longFilePath, ""));

            string normLongPath = longFilePath.NormalizeFilePath();
            Assert.IsTrue(normLongPath.StartsWith(@"\\?\"));

            try
            {
                File.WriteAllText(normLongPath, "");
            }
            finally
            {
                if (File.Exists(normLongPath))
                {
                    File.Delete(normLongPath);
                }
            }
        }

        [Test]
        public void Process_LongFilePath_MatchesExpected()
        {
            string codeSample = "<?php $password = 'hardcoded';";
            string patternSample = "<[.*]> = #";

            string tempDir = Path.GetTempPath();

            string longFilePath = Path.Combine(tempDir, new string('f', PathUtils.MaxPathLength - tempDir.Length - ".php".Length + 1) + ".php");
            Assert.AreEqual(PathUtils.MaxPathLength + 1, longFilePath.Length);

            string longPatternPath = Path.Combine(tempDir, new string('p', PathUtils.MaxPathLength - tempDir.Length - ".pattern".Length + 1) + ".pattern");
            Assert.AreEqual(PathUtils.MaxPathLength + 1, longPatternPath.Length);

            string longLogDirsPath = Path.Combine(tempDir, new string('l', PathUtils.MaxDirLength - tempDir.Length + 1));
            Assert.AreEqual(PathUtils.MaxDirLength + 1, longLogDirsPath.Length);

            string longDirPath = Path.Combine(tempDir, new string('d', PathUtils.MaxDirLength - tempDir.Length));
            Assert.AreEqual(PathUtils.MaxDirLength, longDirPath.Length);

            string normLongFilePath = longFilePath.NormalizeFilePath();
            string normLongPatternPath = longPatternPath.NormalizeFilePath();
            string normLongLogDirsPath = longLogDirsPath.NormalizeDirPath();
            string normLongDirPath = longDirPath.NormalizeDirPath(true);

            WorkflowResult result1, result2, result3;
            try
            {
                File.WriteAllText(normLongFilePath, codeSample);
                File.WriteAllText(normLongPatternPath, patternSample);

                Directory.CreateDirectory(normLongDirPath);
                File.WriteAllText(Path.Combine(normLongDirPath, "fffffffffffffff1.php").NormalizeFilePath(), codeSample);
                File.WriteAllText(Path.Combine(normLongDirPath, "fffffffffffffff2.php").NormalizeFilePath(), codeSample);

                result1 = new TestsCliProcessor().Process($"-f {longFilePath} --patterns {longPatternPath}");
                result2 = new TestsCliProcessor().Process($"-f {normLongFilePath} --patterns {normLongPatternPath}");

                result3 = new TestsCliProcessor().Process($"-f {normLongDirPath} --patterns {longPatternPath} -t 1");
            }
            finally
            {
                if (File.Exists(normLongFilePath))
                {
                    File.Delete(normLongFilePath);
                }
                if (File.Exists(normLongPatternPath))
                {
                    File.Delete(normLongPatternPath);
                }
                if (Directory.Exists(normLongDirPath))
                {
                    Directory.Delete(normLongDirPath, true);
                }
            }

            Assert.AreEqual(0, result1.ErrorCount);
            Assert.AreEqual(1, result1.TotalMatchesCount);

            Assert.AreEqual(0, result2.ErrorCount);
            Assert.AreEqual(1, result2.TotalMatchesCount);

            Assert.AreEqual(0, result3.ErrorCount);
            Assert.AreEqual(2, result3.TotalMatchesCount);
        }
    }
}
