using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.TSqlParseTreeUst;
using Antlr4.Runtime;

namespace PT.PM.SqlParseTreeUst
{
    public class TSqlAntlrParser : AntlrParser
    {
        public override Language Language => Language.TSql;

        public override string[] RuleNames => TSqlParser.ruleNames;

        public static TSqlAntlrParser Create() => new TSqlAntlrParser();

        protected override int CommentsChannel => TSqlLexer.Hidden;

        protected override Parser InitParser(ITokenStream inputStream) => new TSqlParser(inputStream);

        protected override ParserRuleContext Parse(Parser parser) => ((TSqlParser) parser).tsql_file();

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree) =>
            new TSqlAntlrParseTree((TSqlParser.Tsql_fileContext)syntaxTree);

        protected override string ParserSerializedATN => TSqlParser._serializedATN;
    }
}
