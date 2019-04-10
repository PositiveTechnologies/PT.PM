using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PT.PM.Cli.Common;
using PT.PM.Common;
using PT.PM.Common.Files;
using PT.PM.TestUtils;

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
        public void CheckCli_ValidAndInvalidArgs_CorrectlyNormalized()
        {
            var normalizer = new CliParametersNormalizer<CliTestsParameters>();
            string[] outArgs;

            Assert.IsFalse(normalizer.Normalize(new[] { "-upp", "val1", "val2", "-u", "-s", "str" }, out outArgs));
            CollectionAssert.AreEqual(new List<string> { "-s", "str" }, outArgs);

            Assert.IsFalse(normalizer.Normalize(new[] { "val", "--s", "-str", "-int1", "1", "--option", "opt" }, out outArgs));
            CollectionAssert.AreEqual(new List<string> { "-s", "-str", "--int1", "1", "--option", "opt" }, outArgs);

            Assert.IsTrue(normalizer.Normalize(new[] { "-s", "str", "--int1", "1" }, out outArgs));
        }

        [Test]
        public void CheckCli_ArgsWithTypes_CorrectlyNormalized()
        {
            var paramsNormalizer = new CliParametersNormalizer<CliTestsParameters>();
            string[] outArgs;

            string[] inputArgs = "--int x --uint x --byte x --sbyte x --short x --ushort x --long x --ulong x --float x --double x --decimal x --bool x --enum x".SplitArguments();
            Assert.IsFalse(paramsNormalizer.Normalize(inputArgs, out outArgs));
            Assert.AreEqual(13, paramsNormalizer.Errors.Count);
            CollectionAssert.AreEqual(new List<string>(), outArgs);

            paramsNormalizer.CheckTypes = false;
            paramsNormalizer.Errors.Clear();
            Assert.IsTrue(paramsNormalizer.Normalize(inputArgs, out outArgs));
            Assert.AreEqual(0, paramsNormalizer.Errors.Count);
            Assert.AreEqual(inputArgs.Length - 1, outArgs.Length);

            paramsNormalizer.CheckTypes = true;
            paramsNormalizer.Errors.Clear();
            inputArgs = "--int -1 --uint 2 --byte 3 --sbyte -4 --short -5 --ushort 6 --long -7 --ulong 8 --float 9.0 --double 10.0 --decimal 11.0 --bool true --enum file".SplitArguments();
            Assert.IsTrue(paramsNormalizer.Normalize(inputArgs, out outArgs));
            Assert.AreEqual(0, paramsNormalizer.Errors.Count);
            Assert.AreEqual(inputArgs.Length - 1, outArgs.Length);
        }

        [Test]
        public void CheckCli_DuplicateParams_CorrectlyNormalized()
        {
            var paramsNormalizer = new CliParametersNormalizer<CliTestsParameters>();
            string[] outArgs;

            string[] inputArgs = "--int -1 --int 1".SplitArguments();

            paramsNormalizer.CheckDuplicates = false;
            paramsNormalizer.Errors.Clear();
            Assert.IsTrue(paramsNormalizer.Normalize(inputArgs, out outArgs));
            Assert.AreEqual(0, paramsNormalizer.Errors.Count);
            Assert.AreEqual(4, outArgs.Length);

            paramsNormalizer.CheckDuplicates = true;
            paramsNormalizer.Errors.Clear();
            Assert.IsFalse(paramsNormalizer.Normalize(inputArgs, out outArgs));
            Assert.AreEqual(1, paramsNormalizer.Errors.Count);
            Assert.AreEqual(2, outArgs.Length);
            Assert.AreEqual("--int", outArgs[0]);
            Assert.AreEqual("1", outArgs[1]);
        }

        [Test]
        public void CheckCli_BoolValues_CorrectlyNormalized()
        {
            var paramsNormalizer = new CliParametersNormalizer<CliTestsParameters>();
            string[] outArgs;

            Assert.IsTrue(paramsNormalizer.Normalize(new[] { "--bool1", "--bool2" }, out outArgs));
            CollectionAssert.AreEqual(new List<string> { "--bool1", "true", "--bool2", "true" }, outArgs);

            Assert.IsTrue(paramsNormalizer.Normalize(new[] { "--bool1", "true", "--bool2", "false" }, out outArgs));
            CollectionAssert.AreEqual(new List<string> { "--bool1", "true", "--bool2", "false" }, outArgs);

            Assert.IsTrue(paramsNormalizer.Normalize(new[] { "--bool" }, out outArgs));
            CollectionAssert.AreEqual(new List<string> { "--bool" }, outArgs);

            Assert.IsTrue(paramsNormalizer.Normalize(new[] { "--bool", "--bool1" }, out outArgs));
            CollectionAssert.AreEqual(new List<string> { "--bool", "--bool1", "true" }, outArgs);

            Assert.IsTrue(paramsNormalizer.Normalize(new[] { "--bool", "true" }, out outArgs));
            CollectionAssert.AreEqual(new List<string> { "--bool" }, outArgs);

            Assert.IsTrue(paramsNormalizer.Normalize(new[] { "--bool", "false" }, out outArgs));
            CollectionAssert.AreEqual(new List<string> { }, outArgs);
        }

        [Test]
        public void CheckCli_Patterns_CorrectErrorMessages()
        {
            var cliProcessor = new TestsCliProcessor();
            cliProcessor.Process($"--stage {Stage.Pattern} --patterns \"{patterns}\"");

            var errors = (cliProcessor.Logger as TestLogger).Errors;
            string patternKey = patterns.Replace("\"\"", "\"");
            Assert.AreEqual($"Pattern {patternKey} ParsingException: token recognition error at: '>' at {new LineColumnTextSpan(1, 19, 1, 20)}.", errors[0]);
            Assert.AreEqual($"Pattern {patternKey} ParsingException: no viable alternative at input '(?' at {new LineColumnTextSpan(1, 2, 1, 3)}.", errors[1]);
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
                var processor = new Processor("dotnet")
                {
                    Arguments = $"{TestUtility.PtPmExePath} --stage {Stage.Pattern} --patterns \"{patterns}\" --logs-dir \"{logPath}\""
                };
                processor.Start();
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
        public void CheckCli_LogLevels_CorrectLogMessages()
        {
            string logPath = Path.Combine(Path.GetTempPath(), "PT.PM");
            string fileName = Path.Combine(TestUtility.TestsDataPath, "PatternsWithParseErrors.php");
            try
            {
                if (Directory.Exists(logPath))
                {
                    Directory.Delete(logPath, true);
                }

                ProcessWithLogLevel(fileName, logPath, LogLevel.Off);

                DirectoryAssert.DoesNotExist(logPath);

                ProcessWithLogLevel(fileName, logPath, LogLevel.Info, true);

                DirectoryAssert.DoesNotExist(logPath);

                ProcessWithLogLevel(fileName, logPath, LogLevel.Error);

                string[] outputLogLines = File.ReadAllLines(Path.Combine(logPath, "output.log"));

                Assert.IsTrue(outputLogLines.Any(line => line.StartsWith("ERROR")), "ERROR lines should be existed");

                ProcessWithLogLevel(fileName, logPath, LogLevel.Info);
                outputLogLines = File.ReadAllLines(Path.Combine(logPath, "output.log"));

                StringAssert.StartsWith("Time elapsed", outputLogLines[outputLogLines.Length - 1]);

                ProcessWithLogLevel(fileName, logPath, LogLevel.Debug);
                outputLogLines = File.ReadAllLines(Path.Combine(logPath, "output.log"));
                Assert.IsTrue(outputLogLines.Any(line => line.StartsWith("DEBUG")), "DEBUG messages should be existed");
            }
            finally
            {
                if (Directory.Exists(logPath))
                {
                    Directory.Delete(logPath, true);
                }
            }
        }

        private static void ProcessWithLogLevel(string fileName, string logPath, LogLevel logLevel, bool disableLogToFile = false)
        {
            string arguments =
                $"{TestUtility.PtPmExePath} -f {fileName} --logs-dir \"{logPath}\" --log-level {logLevel}";
            if (disableLogToFile)
            {
                arguments += " --no-log-to-file";
            }
            var processor = new Processor("dotnet")
            {
                Arguments = arguments
            };
            processor.Start();
        }

        [Test]
        public void CheckCli_DefaultParams()
        {
            string configFile = Path.Combine(TestUtility.TestsOutputPath, "config.json");
            File.WriteAllText(configFile, @"{ ""Stage"": ""Pattern"" }");
            var result = new TestsCliProcessor().Process($"-c {configFile}");
            Assert.AreEqual(0, result.ThreadCount);
        }

        [Test]
        public void CheckCli_ParamsFromCli()
        {
            string configFile = Path.Combine(TestUtility.TestsOutputPath, "config.json");
            File.WriteAllText(configFile, @"{ ""Stage"": ""Pattern"" }");
            var result = new TestsCliProcessor().Process($"-c {configFile} -t 2");
            Assert.AreEqual(2, result.ThreadCount);
        }

        [Test]
        public void CheckCli_ParamsFromConfigJson()
        {
            string configFile = Path.Combine(TestUtility.TestsOutputPath, "config.json");
            File.WriteAllText(configFile, @"{ ""ThreadCount"": 4 }");
            var result = new TestsCliProcessor().Process($"-c {configFile} -t 2");
            Assert.AreEqual(4, result.ThreadCount);
        }
    }
}
