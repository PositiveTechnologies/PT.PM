using System.IO;
using System.Linq;
using NUnit.Framework;
using PT.PM.Cli.Common;
using PT.PM.Common;
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
        public void CheckCli_ValidAndInvalidArgs_CorrectlyParsed()
        {
            var parser = new CliParametersParser<CliTestsParameters>();

            var result = parser.Parse(new[] {"-upp", "val1", "val2", "-u", "-s", "str"});
            Assert.AreEqual(4, result.Errors.Count);
            Assert.AreEqual("str", result.Parameters.File);

            result = parser.Parse(new[] {"val", "--s", "-str", "-int1", "1", "--option", "opt"});
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("-str", result.Parameters.File);
            Assert.AreEqual(1, result.Parameters.Int1);
            Assert.AreEqual("opt", result.Parameters.Option);

            result = parser.Parse(new[] {"-s", "str", "--int1", "1"});
            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual("str", result.Parameters.File);
            Assert.AreEqual(1, result.Parameters.Int1);
        }

        [Test]
        public void CheckCli_ArgsWithTypes_CorrectlyParsed()
        {
            var parser = new CliParametersParser<CliTestsParameters>();

            string[] inputArgs = "--int x --uint x --byte x --sbyte x --short x --ushort x --long x --ulong x --float x --double x --decimal x --bool x --enum x".SplitArguments();
            var result = parser.Parse(inputArgs);
            var parameters = result.Parameters;
            var errors = result.Errors;

            Assert.AreEqual(13, errors.Count);
            Assert.AreEqual(default(int), parameters.Int);
            Assert.AreEqual(default(uint), parameters.UInt);
            Assert.AreEqual(default(byte), parameters.Byte);
            Assert.AreEqual(default(sbyte), parameters.SByte);
            Assert.AreEqual(default(short), parameters.Short);
            Assert.AreEqual(default(ushort), parameters.UShort);
            Assert.AreEqual(default(long), parameters.Long);
            Assert.AreEqual(default(ulong), parameters.ULong);
            Assert.AreEqual(default(float), parameters.Float);
            Assert.AreEqual(default(double), parameters.Double);
            Assert.AreEqual(default(decimal), parameters.Decimal);
            Assert.AreEqual(default(bool), parameters.Bool);
            Assert.AreEqual(default(Stage), parameters.Enum);

            inputArgs = "--int -1 --uint 2 --byte 3 --sbyte -4 --short -5 --ushort 6 --long -7 --ulong 8 --float 9.0 --double 10.0 --decimal 11.0 --bool true --enum file --array a,b,c".SplitArguments();
            result = parser.Parse(inputArgs);
            parameters = result.Parameters;
            errors = result.Errors;

            Assert.AreEqual(0, errors.Count);
            Assert.AreEqual(-1, parameters.Int);
            Assert.AreEqual(2, parameters.UInt);
            Assert.AreEqual(3, parameters.Byte);
            Assert.AreEqual(-4, parameters.SByte);
            Assert.AreEqual(-5, parameters.Short);
            Assert.AreEqual(6, parameters.UShort);
            Assert.AreEqual(-7, parameters.Long);
            Assert.AreEqual(8, parameters.ULong);
            Assert.AreEqual(9.0, parameters.Float);
            Assert.AreEqual(10.0, parameters.Double);
            Assert.AreEqual(11.0, parameters.Decimal);
            Assert.AreEqual(true, parameters.Bool);
            Assert.AreEqual(Stage.File, parameters.Enum);
            CollectionAssert.AreEqual(new [] { "a", "b", "c" }, parameters.Array);
        }

        [Test]
        public void CheckCli_DuplicateParams_CorrectlyParsed()
        {
            var parser = new CliParametersParser<CliTestsParameters>();

            string[] inputArgs = "--int -1 --int 1".SplitArguments();
            parser.CheckDuplicates = false;
            var result = parser.Parse(inputArgs);
            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(1, result.Parameters.Int);

            parser.CheckDuplicates = true;
            result = parser.Parse(inputArgs);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(-1, result.Parameters.Int);
        }

        [Test]
        public void CheckCli_BoolValues_CorrectlyParsed()
        {
            var parser = new CliParametersParser<CliTestsParameters>();

            var result = parser.Parse(new[] { "--bool1", "--bool2" });
            Assert.IsTrue(result.Parameters.Bool1);
            Assert.IsTrue(result.Parameters.Bool2);

            result = parser.Parse(new[] { "--bool1", "true", "--bool2", "false" });
            Assert.IsTrue(result.Parameters.Bool1);
            Assert.IsFalse(result.Parameters.Bool2);

            result = parser.Parse(new[] { "--bool" });
            Assert.IsTrue(result.Parameters.Bool);

            result = parser.Parse(new[] { "--bool", "--bool1" });
            Assert.IsTrue(result.Parameters.Bool);
            Assert.IsTrue(result.Parameters.Bool1);

            result = parser.Parse(new[] { "--bool", "true" });
            Assert.IsTrue(result.Parameters.Bool);

            result = parser.Parse(new[] { "--bool", "false" });
            Assert.IsFalse(result.Parameters.Bool);
        }

        [Test]
        public void CheckCli_ShouldParseVersionAndHelpTextParams()
        {
            var parser = new CliParametersParser<CliTestsParameters>();
            var result = parser.Parse(new[] { "--version", "--help" });
            Assert.IsTrue(result.ShowVersion);
            Assert.IsTrue(result.ShowHelp);
        }

        [Test]
        public void CheckCli_Patterns_CorrectErrorMessages()
        {
            var cliProcessor = new TestsCliProcessor();
            cliProcessor.Process($"--stage {Stage.Pattern} --patterns \"{patterns}\"");

            var errors = (cliProcessor.Logger as TestLogger)?.Errors;
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
