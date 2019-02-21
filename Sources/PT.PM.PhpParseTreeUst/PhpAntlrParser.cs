using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using System.IO;
using PT.PM.Common.Files;

namespace PT.PM.PhpParseTreeUst
{
    public class PhpAntlrParser : AntlrParser
    {
        public override Language Language => Language.Php;

        public static PhpAntlrParser Create() => new PhpAntlrParser();

        public PhpAntlrParser()
        {
        }

        protected override int CommentsChannel => PhpLexer.PhpComments;

        protected override Parser InitParser(ITokenStream inputStream) => new PhpParser(inputStream);
        
        public override AntlrLexer InitAntlrLexer()
            => new PhpAntlrLexer();

        protected override ParserRuleContext Parse(Parser parser) => ((PhpParser) parser).htmlDocument();

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree) =>
            new PhpAntlrParseTree((PhpParser.HtmlDocumentContext) syntaxTree);

        protected override string ParserSerializedATN => PhpParser._serializedATN;
    }
}