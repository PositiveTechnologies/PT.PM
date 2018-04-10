using PT.PM.AntlrUtils;
using Antlr4.Runtime;
using PT.PM.PlSqlParseTreeUst;
using PT.PM.Common;

namespace PT.PM.SqlParseTreeUst
{
    public class PlSqlAntlrParser : AntlrParser
    {
        public override Language Language => PlSql.Language;

        public override CaseInsensitiveType CaseInsensitiveType => CaseInsensitiveType.UPPER;

        public PlSqlAntlrParser()
        {
        }

        protected override int CommentsChannel => PlSqlLexer.Hidden;

        protected override IVocabulary Vocabulary => PlSqlLexer.DefaultVocabulary;

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree)
        {
            return new PlSqlAntlrParseTree((PlSqlParser.Sql_scriptContext)syntaxTree);
        }

        protected override Lexer InitLexer(ICharStream inputStream) => new PlSqlLexer(inputStream);

        protected override Antlr4.Runtime.Parser InitParser(ITokenStream inputStream) => new PlSqlParser(inputStream);

        protected override ParserRuleContext Parse(Antlr4.Runtime.Parser parser) => ((PlSqlParser)parser).sql_script();

        protected override string LexerSerializedATN => PlSqlLexer._serializedATN;

        protected override string ParserSerializedATN => PlSqlParser._serializedATN;
    }
}
