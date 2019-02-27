using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;

namespace PT.PM.PhpParseTreeUst
{
    public class PhpAntlrParser : AntlrParser
    {
        public override Language Language => Language.Php;

        public static PhpAntlrParser Create() => new PhpAntlrParser();

        protected override int CommentsChannel => PhpLexer.PhpComments;

        protected override Parser InitParser(ITokenStream inputStream) => new PhpParser(inputStream);

        protected override ParserRuleContext Parse(Parser parser) => ((PhpParser) parser).htmlDocument();

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree) =>
            new PhpAntlrParseTree((PhpParser.HtmlDocumentContext) syntaxTree);

        protected override string ParserSerializedATN => PhpParser._serializedATN;
    }
}