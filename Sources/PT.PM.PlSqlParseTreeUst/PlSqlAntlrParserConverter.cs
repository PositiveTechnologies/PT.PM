using System;
using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Files;
using PT.PM.PlSqlParseTreeUst;
using static PT.PM.PlSqlParseTreeUst.PlSqlLexer;

namespace PT.PM.SqlParseTreeUst
{
    public class PlSqlAntlrParserConverter : AntlrParserConverter
    {
        public static PlSqlAntlrParserConverter Create() => new PlSqlAntlrParserConverter();

        public override Language Language => Language.PlSql;

        public override IVocabulary LexerVocabulary { get; } = DefaultVocabulary;

        public override string[] RuleNames => PlSqlParser.ruleNames;

        public override int[] IdentifierTokenTypes { get; } = {REGULAR_ID, DELIMITED_ID, BINDVAR};

        public override Type[] IdentifierRuleType { get; } =
        {
            typeof(PlSqlParser.Id_expressionContext),
            typeof(PlSqlParser.Regular_idContext),
            typeof(PlSqlParser.Non_reserved_keywords_pre12cContext),
            typeof(PlSqlParser.Non_reserved_keywords_pre12cContext),
            typeof(PlSqlParser.String_function_nameContext),
            typeof(PlSqlParser.Numeric_function_nameContext),
        };

        public override int[] StringTokenTypes { get; } = {NATIONAL_CHAR_STRING_LIT, CHAR_STRING};

        public override int NullTokenType { get; } = NULL_;

        public override int TrueTokenType { get; } = TRUE;

        public override int FalseTokenType { get; } = FALSE;

        public override int ThisTokenType { get; } = ZeroTokenType;

        public override int DecNumberTokenType { get; } = UNSIGNED_INTEGER;

        public override int HexNumberTokenType { get; } = HEX_STRING_LIT;

        public override int OctNumberTokenType { get; } = ZeroTokenType;

        public override int BinNumberTokenType { get; } = BIT_STRING_LIT;

        public override int FloatNumberTokenType { get; } = APPROXIMATE_NUM_LIT;

        public override int[] CommentTokenTypes { get; } = {SINGLE_LINE_COMMENT, MULTI_LINE_COMMENT, REMARK_COMMENT};

        protected override string ParserSerializedATN => PlSqlParser._serializedATN;

        protected override Parser InitParser(ITokenStream inputStream) => new PlSqlParser(inputStream);

        protected override ParserRuleContext Parse(Parser parser) => ((PlSqlParser) parser).sql_script();

        protected override AntlrListenerConverter InitListener(TextFile sourceFile) => new PlSqlAntlrListenerConverter(sourceFile, this);
    }
}