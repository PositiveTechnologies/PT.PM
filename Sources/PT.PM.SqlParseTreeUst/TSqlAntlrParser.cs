using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.TSqlParseTreeUst;
using Antlr4.Runtime;

namespace PT.PM.SqlParseTreeUst
{
    public class TSqlAntlrParser : AntlrParser
    {
        public override LanguageInfo Language => TSql.Language;

        public override CaseInsensitiveType CaseInsensitiveType => CaseInsensitiveType.UPPER;

        public TSqlAntlrParser()
        {
        }

        protected override int CommentsChannel => TSqlLexer.Hidden;

        protected override IVocabulary Vocabulary => TSqlLexer.DefaultVocabulary;

        public override Lexer InitLexer(ICharStream inputStream)
        {
            return new TSqlLexer(inputStream);
        }

        public override Antlr4.Runtime.Parser InitParser(ITokenStream inputStream)
        {
            return new TSqlParser(inputStream);
        }

        protected override ParserRuleContext Parse(Antlr4.Runtime.Parser parser)
        {
            return ((TSqlParser) parser).tsql_file();
        }

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree)
        {
            return new TSqlAntlrParseTree((TSqlParser.Tsql_fileContext)syntaxTree);
        }
    }
}
