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
        public void Parse_LineColumnTextSpan()
        {
            var lcTextSpan = new LineColumnTextSpan(42, 1, 42, 1);
            string textSpanString = lcTextSpan.ToString();
            Assert.AreEqual(lcTextSpan, TextUtils.ParseLineColumnTextSpan(textSpanString));

            lcTextSpan = new LineColumnTextSpan(42, 1, 41, 5);
            textSpanString = lcTextSpan.ToString();
            Assert.AreEqual(lcTextSpan, TextUtils.ParseLineColumnTextSpan(textSpanString));
        }
    }
}
