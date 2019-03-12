using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;

namespace PT.PM.Python3ParseTreeUst
{
    public class Python3AntlrParser : AntlrParser
    {
        public override Language Language => Language.Python3;
        public override string[] RuleNames => Python3Parser.ruleNames;
        protected override string ParserSerializedATN => Python3Parser._serializedATN;
        protected override int CommentsChannel => Python3Lexer.Hidden;

        protected override Parser InitParser(ITokenStream inputStream)
            => new Python3Parser(inputStream);

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree)
            => new Python3AntlrParseTree((Python3Parser.RootContext) syntaxTree);

        public static Python3AntlrParser Create() => new Python3AntlrParser();

        protected override ParserRuleContext Parse(Parser parser)
            => ((Python3Parser) parser).root();
    }
}