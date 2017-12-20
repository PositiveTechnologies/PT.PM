using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Matching;
using PT.PM.TestUtils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM.Cli.Tests
{
    [TestFixture]
    public class CliTests
    {
        private readonly static string exeName = Path.Combine(TestUtility.TestsPath, "PT.PM.Cli.exe");

        [Test]
        public void CheckCli_Patterns_CorrectErrorMessages()
        {
            if (CommonUtils.IsRunningOnLinux)
            {
                Assert.Ignore("TODO: fix failed Cli unit-test on mono (Linux)");
            }

            string patternsStr = PreparePatternsString();
            var result = ProcessUtils.SetupHiddenProcessAndStart(exeName, $"--stage {Stage.Pattern} --patterns {patternsStr} --log-errors");

            Assert.AreEqual("Pattern ParsingException in \"Pattern\": token recognition error at: '>' at [1;19]-[1;20).", result.Output[2]);
            Assert.AreEqual("Pattern ParsingException in \"Pattern\": no viable alternative at input '(?' at [1;2]-[1;3).", result.Output[3]);
        }

        [Test]
        public void CheckCli_LogPath_FilesInProperDirectory()
        {
            if (CommonUtils.IsRunningOnLinux)
            {
                Assert.Ignore("TODO: fix failed Cli unit-test on mono (Linux)");
            }

            string patternsStr = PreparePatternsString();

            string logPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(exeName));
            try
            {
                if (Directory.Exists(logPath))
                {
                    Directory.Delete(logPath, true);
                }
                var result = ProcessUtils.SetupHiddenProcessAndStart(exeName, $"--stage {Stage.Pattern} --patterns {patternsStr} --logs-dir \"{logPath}\"");
                var logFiles = Directory.GetFiles(logPath, "*.*");
                Assert.Greater(logFiles.Length, 0);
            }
            finally
            {
                if (Directory.Exists(logPath))
                {
                    Directory.Delete(logPath, true);
                }
            }
        }

        [Test]
        [Ignore("TODO: fix on CI")]
        public void CheckCli_SeveralLanguages_OnlyPassedLanguagesProcessed()
        {
            if (CommonUtils.IsRunningOnLinux)
            {
                Assert.Ignore("TODO: fix failed Cli unit-test on mono (Linux)");
            }

            ProcessExecutionResult result = ProcessUtils.SetupHiddenProcessAndStart(exeName,
                $"-f \"{TestUtility.TestsDataPath}\" " +
                $"-l PlSql,TSql " +
                $"--stage {Stage.ParseTree} --log-debugs");

            // Do not process php (csharp, java etc.) files.
            Assert.IsTrue(result.Output.Any(line => line.Contains(".php has not been read")));
            Assert.IsTrue(result.Output.Any(line => line.Contains("has been detected")));

            result = ProcessUtils.SetupHiddenProcessAndStart(exeName,
                $"-f \"{TestUtility.TestsDataPath}\" " +
                $"-l PlSql " +
                $"--stage {Stage.ParseTree} --log-debugs");

            // Do not detect language for only one language.
            Assert.IsFalse(result.Output.Any(line => line.Contains("has been detected")));
        }

        [Test]
        public void CheckCli_FakeLanguage_CorrectlyProcessed()
        {
            if (CommonUtils.IsRunningOnLinux)
            {
                Assert.Ignore("TODO: fix failed Cli unit-test on mono (Linux)");
            }

            var patternTempFile = Path.GetTempFileName() + ".json";
            File.WriteAllText(patternTempFile, "[{\"Name\":\"\",\"Key\":\"1\",\"Languages\":[\"Fake\"],\"DataFormat\":\"Dsl\",\"Value\":\"<[(?i)password(?-i)]> = <[\\\"\\\\w*\\\" || null]>\", \"CweId\":\"\", \"Description\":\"\"}]");
            ProcessExecutionResult result = ProcessUtils.SetupHiddenProcessAndStart(exeName,
               $"--stage {Stage.Pattern} " +
               $"--patterns {patternTempFile} " +
               $"--log-debugs --log-errors");

            Assert.AreEqual("PatternNode \"1\" doesn't have proper target languages.", result.Output[2]);
        }

        [Test]
        [Ignore("TODO: fix on CI")]
        public void CheckCli_FilePatternsRepository_CorrectlyProcessed()
        {
            if (CommonUtils.IsRunningOnLinux)
            {
                Assert.Ignore("TODO: fix failed Cli unit-test on mono (Linux)");
            }

            var patternsFileName = Path.Combine(Path.GetTempPath(), "patterns.json");
            File.WriteAllText(patternsFileName, "[{\"Key\":\"1\",\"Value\":\"<[(?i)password(?-i)]> = <[\\\"\\\\w*\\\" || null]>\"}]");
            try
            {
                var result = ProcessUtils.SetupHiddenProcessAndStart(exeName,
                    $"-f \"{Path.Combine(TestUtility.TestsDataPath, "Patterns.cs")}\" " +
                    $"--patterns \"{patternsFileName}\"");

                Assert.IsTrue(result.Output.Any(str => str.Contains("Pattern matched")));
            }
            finally
            {
                File.Delete(patternsFileName);
            }
        }

        private static string PreparePatternsString()
        {
            List<PatternDto> patternDtos = new List<PatternDto>()
            {
                new PatternDto
                {
                    Key = "1",
                    DataFormat = "Dsl",
                    Value = "(?i)password(?-i)]> = <[\"\\w*\" || null]>"
                }
            };
            var patternsStr = StringCompressorEscaper.CompressEscape(JsonConvert.SerializeObject(patternDtos, Formatting.None, new StringEnumConverter()));
            return patternsStr;
        }
    }
}
