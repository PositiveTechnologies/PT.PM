using PT.PM.AstConversion;
using PT.PM.AstParsing;
using PT.PM.Common;
using PT.PM.CSharpAstConversion;
using PT.PM.Dsl;
using PT.PM.JavaAstConversion;
using PT.PM.Matching;
using PT.PM.Patterns;
using PT.PM.SqlAstConversion;
using NUnit.Framework;

namespace PT.PM.Tests
{
    [TestFixture]
    public class BuildersTests
    {
        [Test]
        public void Build_LangType_ProperlyParserConverter()
        {
            var parserConverterPair = ParserConverterBuilder.GetParserConverterSet(Language.CSharp);
            Assert.IsInstanceOf(typeof(CSharpRoslynParser), parserConverterPair.Parser);
            Assert.IsInstanceOf(typeof(CSharpRoslynParseTreeConverter), parserConverterPair.Converter);
            
            parserConverterPair = ParserConverterBuilder.GetParserConverterSet(Language.Java);
            Assert.IsInstanceOf(typeof(JavaAntlrParser), parserConverterPair.Parser);
            Assert.IsInstanceOf(typeof(JavaAntlrParseTreeConverter), parserConverterPair.Converter);

            parserConverterPair = ParserConverterBuilder.GetParserConverterSet(Language.Php);
            Assert.IsInstanceOf(typeof(PhpAntlrParser), parserConverterPair.Parser);
            Assert.IsInstanceOf(typeof(PhpAntlrParseTreeConverter), parserConverterPair.Converter);

            parserConverterPair = ParserConverterBuilder.GetParserConverterSet(Language.Php);
            Assert.IsInstanceOf(typeof(PhpAntlrParser), parserConverterPair.Parser);
            Assert.IsInstanceOf(typeof(PhpAntlrParseTreeConverter), parserConverterPair.Converter);

            parserConverterPair = ParserConverterBuilder.GetParserConverterSet(Language.PlSql);
            Assert.IsInstanceOf(typeof(PlSqlAntlrParser), parserConverterPair.Parser);
            Assert.IsInstanceOf(typeof(PlSqlAntlrConverter), parserConverterPair.Converter);

            parserConverterPair = ParserConverterBuilder.GetParserConverterSet(Language.TSql);
            Assert.IsInstanceOf(typeof(TSqlAntlrParser), parserConverterPair.Parser);
            Assert.IsInstanceOf(typeof(TSqlAntlrConverter), parserConverterPair.Converter);
        }
    }
}
