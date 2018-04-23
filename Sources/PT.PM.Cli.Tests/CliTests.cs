using NUnit.Framework;
using PT.PM.Cli.Common;
using PT.PM.Common;
using PT.PM.TestUtils;
using System.IO;

namespace PT.PM.Cli.Tests
{
    [TestFixture]
    public class CliTests
    {
        private const string patterns = "(?i)password(?-i)]> = <[\"\"\\w*\"\" || null]>";

        [Test]
        public void CheckCli_String_CorrectlySplitted()
        {
            CollectionAssert.AreEqual(new string[0], "".SplitArguments());
            CollectionAssert.AreEqual(new[] { "-a", "--param", "-c" }, "-a  --param -c    ".SplitArguments());
            CollectionAssert.AreEqual(new[] { "-s", "a\"b c", "-d", "", "", "-e", "" }, "-s \"a\"\"b c\" -d \"\" \"\" -e \"\"".SplitArguments());
            CollectionAssert.AreEqual(new[] { "sa", "b" }, "s\"a\" b".SplitArguments());
        }

        [Test]
        public void CheckCli_Patterns_CorrectErrorMessages()
        {
            var cliProcessor = new TestsCliProcessor();
            cliProcessor.Process($"--stage {Stage.Pattern} --patterns \"{patterns}\" --log-errors true");

            var errors = (cliProcessor.Logger as LoggerMessageCounter).Errors;
            Assert.AreEqual($"Pattern ParsingException in \"Pattern\": token recognition error at: '>' at {new LineColumnTextSpan(1, 19, 1, 20)}.", errors[0]);
            Assert.AreEqual($"Pattern ParsingException in \"Pattern\": no viable alternative at input '(?' at {new LineColumnTextSpan(1, 2, 1, 3)}.", errors[1]);
        }

        [Test]
        public void CheckCli_LogPath_FilesInProperDirectory()
        {
            string logPath = Path.Combine(Path.GetTempPath(), "PT.PM");
            try
            {
                if (Directory.Exists(logPath))
                {
                    Directory.Delete(logPath, true);
                }
                var result = ProcessUtils.SetupHiddenProcessAndStart("dotnet", $"{TestUtility.PtPmExePath} --stage {Stage.Pattern} --patterns \"{patterns}\" --logs-dir \"{logPath}\"");
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
    }
}
