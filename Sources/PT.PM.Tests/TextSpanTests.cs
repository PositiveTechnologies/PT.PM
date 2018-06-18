using NUnit.Framework;
using PT.PM.Common;

namespace PT.PM.Tests
{
    [TestFixture]
    public class TextSpanTests
    {
        [Test]
        public void Parse_TextSpan()
        {
            var textSpan = new TextSpan(42, 0);
            string textSpanString = textSpan.ToString();
            Assert.AreEqual(textSpan, TextUtils.ParseTextSpan(textSpanString));

            textSpan = new TextSpan(42, 5);
            textSpanString = textSpan.ToString();
            Assert.AreEqual(textSpan, TextUtils.ParseTextSpan(textSpanString));
        }

        [Test]
        [TestCase(42, 1, 42, 1)]
        [TestCase(42, 1, 41, 5)]
        [TestCase(42, 5, 43, 5)]
        [TestCase(42, 5, 42, 8)]
        public void Parse_LineColumnTextSpan(int beginLine, int beginCol, int endLine, int endCol)
        {
            var lcTextSpan = new LineColumnTextSpan(beginLine, beginCol, endLine, endCol);
            string textSpanString = lcTextSpan.ToString();
            Assert.AreEqual(lcTextSpan, TextUtils.ParseLineColumnTextSpan(textSpanString));
        }
    }
}
