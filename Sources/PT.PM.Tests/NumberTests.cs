using NUnit.Framework;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.TestUtils;
using System.IO;
using static PT.PM.Common.UstLinq;

namespace PT.PM.Tests
{
    [TestFixture]
    public class NumberTests
    {
        [Test]
        public void Number_Recognition_Check()
        {
            string path = Path.Combine(TestUtility.TestsDataPath, "Numbers.php");

            TestUtility.CheckFile(path, Stage.Ust, out RootUst ust);

            Assert.IsTrue(ust.AnyDescendantOrSelf(descendant =>
            {
                return descendant is IntLiteral;
            }));

            Assert.IsTrue(ust.AnyDescendantOrSelf(descendant =>
            {
                return descendant is LongLiteral;
            }));

            Assert.IsTrue(ust.AnyDescendantOrSelf(descendant =>
            {
                return descendant is BigIntLiteral;
            }));
        }

    }
}
