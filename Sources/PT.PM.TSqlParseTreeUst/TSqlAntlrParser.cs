using System;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.TSqlParseTreeUst;
using Antlr4.Runtime;
using static PT.PM.TSqlParseTreeUst.TSqlLexer;

namespace PT.PM.SqlParseTreeUst
{
    public class TSqlAntlrParser : AntlrParser
    {
        public static TSqlAntlrParser Create() => new TSqlAntlrParser();

        public override Language Language => Language.TSql;

        public override IVocabulary LexerVocabulary { get; } = DefaultVocabulary;

        public override string[] RuleNames => TSqlParser.ruleNames;

        public override int[] IdentifierTokenTypes { get; } = {DOUBLE_QUOTE_ID, SQUARE_BRACKET_ID, LOCAL_ID, ID};

        public override Type[] IdentifierRuleType { get; } = {typeof(TSqlParser.Simple_idContext)};

        public override int[] StringTokenTypes { get; } = {DOUBLE_QUOTE_ID, STRING, QUOTED_URL, QUOTED_HOST_AND_PORT};

        public override int NullTokenType { get; } = NULL;

        public override int TrueTokenType { get; } = ZeroTokenType;

        public override int FalseTokenType { get; } = ZeroTokenType;

        public override int ThisTokenType { get; } = ZeroTokenType;

        public override int DecNumberTokenType { get; } = DECIMAL;

        public override int HexNumberTokenType { get; } = BINARY;

        public override int OctNumberTokenType { get; } = ZeroTokenType;

        public override int BinNumberTokenType { get; } = ZeroTokenType;

        public override int FloatNumberTokenType { get; } = REAL;

        public override int[] CommentTokenTypes { get; } = {COMMENT, LINE_COMMENT};

        protected override Parser InitParser(ITokenStream inputStream) => new TSqlParser(inputStream);

        protected override ParserRuleContext Parse(Parser parser) => ((TSqlParser) parser).tsql_file();

        protected override AntlrParseTree CreateParseTree(ParserRuleContext parserRuleContext) =>
            new TSqlAntlrParseTree((TSqlParser.Tsql_fileContext)parserRuleContext);

        protected override string ParserSerializedATN => TSqlParser._serializedATN;
    }
}
