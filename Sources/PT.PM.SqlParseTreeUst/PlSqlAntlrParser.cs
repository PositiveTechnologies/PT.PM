using PT.PM.AntlrUtils;
using Antlr4.Runtime;
using PT.PM.SqlParseTreeUst.Parser;
using PT.PM.Common;

namespace PT.PM.SqlParseTreeUst
{
    public class PlSqlAntlrParser : AntlrParser
    {
        public override Language Language => Language.PlSql;

        public PlSqlAntlrParser()
        {
        }

        protected override int CommentsChannel => plsqlLexer.Hidden;

        protected override IVocabulary Vocabulary => plsqlLexer.DefaultVocabulary;

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree)
        {
            return new PlSqlAntlrParseTree((plsqlParser.Compilation_unitContext)syntaxTree);
        }

        protected override Lexer InitLexer(ICharStream inputStream)
        {
            return new plsqlLexer(inputStream);
        }

        protected override Antlr4.Runtime.Parser InitParser(ITokenStream inputStream)
        {
            return new plsqlParser(inputStream);
        }

        protected override ParserRuleContext Parse(Antlr4.Runtime.Parser parser)
        {
            return ((plsqlParser)parser).compilation_unit();
        }
    }
}
