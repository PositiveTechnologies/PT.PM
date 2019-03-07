using NUnit.Framework;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.TestUtils;
using System.IO;
using PT.PM.Common;
using PT.PM.Common.Files;
using static PT.PM.Common.UstLinq;

namespace PT.PM.Tests
{
    [TestFixture]
    public class NumberTests
    {
        static readonly string testFileName = Path.Combine(TestUtility.TestsDataPath, "Numbers.php");
        
        [Test]
        public void Number_Recognition_Check()
        {
            TestUtility.CheckFile(testFileName, Stage.Ust, out RootUst ust);
            Assert.IsTrue(ust.AnyDescendantOrSelf(d => d is IntLiteral));
            Assert.IsTrue(ust.AnyDescendantOrSelf(d => d is LongLiteral));
            Assert.IsTrue(ust.AnyDescendantOrSelf(d => d is BigIntLiteral));
        }

        [Test]
        public void Match_Big_Integer()
        {
            var sourceFile = TextFile.Read(testFileName);
            var matches = PatternMatchingUtils.GetMatches(sourceFile, "<[9223372036854775807123456]>", Language.Php);
            Assert.AreEqual(1, matches.Length);
        }
    }
}
