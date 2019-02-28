using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.MySqlParseTreeUst;

namespace PT.PM.SqlParseTreeUst
{
    public class MySqlAntlrParser : AntlrParser
    {
        public override Language Language => Language.MySql;

        public override string[] RuleNames => MySqlParser.ruleNames;

        public static MySqlAntlrParser Create() => new MySqlAntlrParser();

        protected override int CommentsChannel => MySqlLexer.Hidden;

        protected override string ParserSerializedATN => MySqlParser._serializedATN;

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree) =>
            new MySqlAntlrParseTree((MySqlParser.RootContext) syntaxTree);

        protected override Parser InitParser(ITokenStream inputStream) =>
            new MySqlParser(inputStream);

        protected override ParserRuleContext Parse(Parser parser) =>
            ((MySqlParser) parser).root();
    }
}