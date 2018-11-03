using NUnit.Framework;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Matching;
using PT.PM.Patterns.PatternsRepository;
using PT.PM.PhpParseTreeUst;
using PT.PM.TestUtils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM.Dsl.Tests
{
    [TestFixture]
    public class DslParseTests
    {
        private DefaultPatternRepository patternsRepository;
        private List<PatternRoot> patterns;

        [SetUp]
        public void Init()
        {
            patternsRepository = new DefaultPatternRepository();
            var patternsConverter = new PatternConverter();
            patterns = patternsConverter.Convert(patternsRepository.GetAll());
        }

        [TestCase("HardcodedPassword.pattern")]
        [TestCase("InsecureTransport.pattern")]
        [TestCase("InsecureRandomness.pattern")]
        [TestCase("WeakCryptographicHash.pattern")]
        [TestCase("AndroidPermissionCheck.pattern")]
        [TestCase("MissingBroadcasterPermission.pattern")]
        [TestCase("CookieNotSentOverSSL.pattern")]
        [TestCase("CookieSecurityOverlyBroadDomain.pattern")]
        [TestCase("PasswordInComment.pattern")]
        [TestCase("InadequateRSAPadding.pattern")]
        [TestCase("DebugInfo.pattern")]
        [TestCase("XmlExternalEntity.pattern")]
        [TestCase("AndroidHostnameVerificationDisabled.pattern")]
        [TestCase("KeyManagementNullEncryptionKey.pattern")]
        [TestCase("AttributesCodeInsideElementEvent.pattern")]
        [TestCase("ExtendingSecurityManagerWithoutFinal.pattern")]
        [TestCase("ImproperValidationEmptyMethodFull.pattern")]
        [TestCase("UsingCloneWithoutCloneable.pattern")]
        [TestCase("PoorLoggingPractice.pattern")]
        public void Process_Dsl_EqualsToHardcoded(string fileName)
        {
            string data = File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, fileName));
            var logger = new LoggerMessageCounter();
            var processor = new DslProcessor { Logger = logger, PatternExpressionInsideStatement = false };
            PatternRoot result = processor.Deserialize(new CodeFile(data) { PatternKey = fileName });
            if (fileName == "DebugInfo.pattern")
            {
                new HashSet<Language> { Php.Language };
            }
            Assert.AreEqual(0, logger.ErrorCount, logger.ErrorsString);

            string patternName = Path.GetFileNameWithoutExtension(fileName);
            PatternRoot defaultPattern = patterns.FirstOrDefault(p => p.DebugInfo.StartsWith(patternName));
            if (defaultPattern == null)
            {
                Assert.Inconclusive($"Pattern {patternName} does not exists in DefaultPatternRepository");
            }

            var patternNormalizer = new PatternNormalizer();
            defaultPattern = patternNormalizer.Normalize(defaultPattern);

            Assert.AreEqual(defaultPattern.Node, result.Node);
        }

        [TestCase("Range.pattern")]
        public void Parse_Dsl_WithoutErrors(string fileName)
        {
            string data = File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, fileName));
            var logger = new LoggerMessageCounter();
            var processor = new DslProcessor { Logger = logger };
            PatternRoot result = processor.Deserialize(new CodeFile(data) { PatternKey = fileName });
            Assert.AreEqual(0, logger.ErrorCount, logger.ErrorsString);
        }

        [Test]
        public void ProcessDsl_SampleWithSyntaxError_HandleErrors()
        {
            var logger = new LoggerMessageCounter();
            var data = "(?i)password(?-i)]> = <[\"\\w*\" || null]>";
            var processor = new DslProcessor() { Logger = logger };
            PatternRoot result = processor.Deserialize(new CodeFile(data) { PatternKey = "ErrorneousPattern" });
            Assert.AreEqual(5, logger.ErrorCount);
        }

        [TestCase("<[@pwd:password]> = #; test_call(<[@pwd:^pass]>);")]
        [TestCase("<[@pwd]> = #; test_call(<[@pwd:password]>);")]
        public void ProcessDsl_PatternVarAlreadyDefined_HandleErrors(string data)
        {
            Assert.Throws(typeof(ConversionException), () =>
            {
                var processor = new DslProcessor();
                PatternRoot result = processor.Deserialize(new CodeFile(data) { PatternKey = "Unnamed" });
            });
        }
    }
}
