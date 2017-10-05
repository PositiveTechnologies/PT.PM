using PT.PM.AntlrUtils;
using Antlr4.Runtime;
using PT.PM.PlSqlParseTreeUst;
using PT.PM.Common;

namespace PT.PM.SqlParseTreeUst
{
    public class PlSqlAntlrParser : AntlrParser
    {
        public override LanguageInfo Language => PlSql.Language;

        public override CaseInsensitiveType CaseInsensitiveType => CaseInsensitiveType.UPPER;

        public PlSqlAntlrParser()
        {
        }

        protected override int CommentsChannel => PlSqlLexer.Hidden;

        protected override IVocabulary Vocabulary => PlSqlLexer.DefaultVocabulary;

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree)
        {
            return new PlSqlAntlrParseTree((PlSqlParser.Compilation_unitContext)syntaxTree);
        }

        public override Lexer InitLexer(ICharStream inputStream)
        {
            return new PlSqlLexer(inputStream);
        }

        public override Antlr4.Runtime.Parser InitParser(ITokenStream inputStream)
        {
            return new PlSqlParser(inputStream);
        }

        protected override ParserRuleContext Parse(Antlr4.Runtime.Parser parser)
        {
            return ((PlSqlParser)parser).compilation_unit();
        }
    }
}
