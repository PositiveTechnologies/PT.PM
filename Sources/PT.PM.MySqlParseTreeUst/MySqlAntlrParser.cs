using System;
using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.MySqlParseTreeUst;
using static PT.PM.MySqlParseTreeUst.MySqlLexer;

namespace PT.PM.SqlParseTreeUst
{
    public class MySqlAntlrParser : AntlrParser
    {
        public static MySqlAntlrParser Create() => new MySqlAntlrParser();

        public override Language Language => Language.MySql;

        public override IVocabulary LexerVocabulary { get; } = DefaultVocabulary;

        public override string[] RuleNames => MySqlParser.ruleNames;

        public override int[] IdentifierTokenTypes { get; } = {ID, REVERSE_QUOTE_ID, CHARSET_REVERSE_QOUTE_STRING};

        public override Type[] IdentifierRuleType { get; } =
        {
            typeof(MySqlParser.UidContext),
            typeof(MySqlParser.SimpleIdContext),
            typeof(MySqlParser.CharsetNameContext),
            typeof(MySqlParser.TransactionLevelContext),
            typeof(MySqlParser.EngineNameContext),
            typeof(MySqlParser.PrivilegesBaseContext),
            typeof(MySqlParser.IntervalTypeContext),
            typeof(MySqlParser.DataTypeBaseContext),
            typeof(MySqlParser.KeywordsCanBeIdContext),
            typeof(MySqlParser.FunctionNameBaseContext),
            typeof(MySqlParser.DottedIdContext)
        };

        public override int[] StringTokenTypes { get; } = {START_NATIONAL_STRING_LITERAL, STRING_LITERAL};

        public override int NullTokenType { get; } = NULL_LITERAL;

        public override int TrueTokenType { get; } = TRUE;

        public override int FalseTokenType { get; } = FALSE;

        public override int ThisTokenType { get; } = ZeroTokenType;

        public override int DecNumberTokenType { get; } = DECIMAL_LITERAL;

        public override int HexNumberTokenType { get; } = HEXADECIMAL_LITERAL;

        public override int OctNumberTokenType { get; } = ZeroTokenType;

        public override int BinNumberTokenType { get; } = ZeroTokenType;

        public override int FloatNumberTokenType { get; } = REAL_LITERAL;

        public override int[] CommentTokenTypes { get; } = {SPEC_MYSQL_COMMENT, COMMENT_INPUT, LINE_COMMENT};

        protected override string ParserSerializedATN => MySqlParser._serializedATN;

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree) =>
            new MySqlAntlrParseTree((MySqlParser.RootContext) syntaxTree);

        protected override Parser InitParser(ITokenStream inputStream) =>
            new MySqlParser(inputStream);

        protected override ParserRuleContext Parse(Parser parser) =>
            ((MySqlParser) parser).root();
    }
}