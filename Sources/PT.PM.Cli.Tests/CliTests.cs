using PT.PM.Common;
using PT.PM.Common.Tests;
using PT.PM.Patterns;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM.Cli.Tests
{
    [TestFixture]
    public class CliTests
    {
        private readonly static string exeName = Path.Combine(TestHelper.TestsPath, "PT.PM.Cli.exe");

        [Test]
        public void CheckCli_Patterns_CorrectErrorMessages()
        {
            string patternsStr = PreparePatternsString();
            var result = ProcessHelpers.SetupHiddenProcessAndStart(exeName, $"--stage {Stage.Patterns} --patterns {patternsStr} --log-errors");

            Assert.AreEqual("Error: token recognition error at: '>' at 1:18", result.Output[2]);
            Assert.AreEqual("Error: no viable alternative at input '(?' at 1:1", result.Output[3]);
        }

        [Test]
        public void CheckCli_LogPath_FilesInProperDirectory()
        {
            string patternsStr = PreparePatternsString();

            string logPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(exeName));
            try
            {
                if (Directory.Exists(logPath))
                {
                    Directory.Delete(logPath, true);
                }
                var result = ProcessHelpers.SetupHiddenProcessAndStart(exeName, $"--stage {Stage.Patterns} --patterns {patternsStr} --logs-dir \"{logPath}\"");
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
        public void CheckCli_SeveralLanguages_OnlyPassedLanguagesProcessed()
        {
            ProcessExecutionResult result = ProcessHelpers.SetupHiddenProcessAndStart(exeName,
                $"-f \"{TestHelper.TestsDataPath}\" " +
                $"-l {Language.PlSql} {Language.TSql} " +
                $"--stage {Stage.Parse} --log-debugs");

            // Do not process php (csharp, java etc.) files.
            Assert.IsFalse(result.Output.Any(line => line.Contains("php")));
            Assert.IsTrue(result.Output.Any(line => line.Contains("has been detected")));

            result = ProcessHelpers.SetupHiddenProcessAndStart(exeName,
                $"-f \"{TestHelper.TestsDataPath}\" " +
                $"-l {Language.PlSql} " +
                $"--stage {Stage.Parse} --log-debugs");

            // Do not detect language for only one language.
            Assert.IsFalse(result.Output.Any(line => line.Contains("has been detected")));
        }

        [Test]
        public void CheckCli_FakeLanguage_CorrectlyProcessed()
        {
            // Patterns: [{"Name":"","Key":"1","Languages":"Fake","DataFormat":"Dsl","Value":"<[(?i)password(?-i)]> = <[\"\\w*\" || null]>","CweId":"","Description":""}]
            ProcessExecutionResult result = ProcessHelpers.SetupHiddenProcessAndStart(exeName,
               $"--stage {Stage.Patterns} " +
               $"--patterns kAAAAB+LCAAAAAAABAAljb0KwjAURl8l3KkVHVyltoOhUBRHlyTDxYYSTJOSH4JY391b3M534PCJD9xx1nAC2MNVvwmORDd0U8ZJR9o9vjQpjgl7H2ZM5Hi0pB5o85Y2oupMvWCMxYex6g6mVi07s0ZIkLLsJLB1ZS5bq1rKLkUP4/+R6/gMZknGu0181Q+1349CkAAAAA== " +
               $"--log-debugs --log-errors");

            Assert.AreEqual("Error: Language \"Fake\" is not supported or wrong.", result.Output[2]);
            Assert.AreEqual("Pattern \"1\" ignored because of it doesn't have target languages.", result.Output[3]);
        }

        [Test]
        public void CheckCli_FilePatternsRepository_CorrectlyProcessed()
        {
            var patternsFileName = Path.Combine(Path.GetTempPath(), "patterns.json");
            File.WriteAllText(patternsFileName, "[{\"Key\":\"1\",\"Value\":\"<[(?i)password(?-i)]> = <[\\\"\\\\w*\\\" || null]>\"}]");
            try
            {
                var result = ProcessHelpers.SetupHiddenProcessAndStart(exeName,
                    $"-f \"{Path.Combine(TestHelper.TestsDataPath, "Patterns.cs")}\" " +
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
                    Value = "(?i)password(?-i)]> = <[\"\\w*\" || null]>"
                }
            };
            var patternsStr = StringCompressorEscaper.CompressEscape(JsonConvert.SerializeObject(patternDtos, Formatting.None, new StringEnumConverter()));
            return patternsStr;
        }
    }
}
