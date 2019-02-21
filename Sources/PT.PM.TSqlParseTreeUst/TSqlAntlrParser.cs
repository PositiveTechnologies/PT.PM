using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.TSqlParseTreeUst;
using Antlr4.Runtime;

namespace PT.PM.SqlParseTreeUst
{
    public class TSqlAntlrParser : AntlrParser
    {
        public override Language Language => Language.TSql;

        public static TSqlAntlrParser Create() => new TSqlAntlrParser();

        public TSqlAntlrParser()
        {
        }

        protected override int CommentsChannel => TSqlLexer.Hidden;

        protected override Antlr4.Runtime.Parser InitParser(ITokenStream inputStream) => new TSqlParser(inputStream);
        
        public override AntlrLexer InitAntlrLexer()
            => new TSqlAntlrLexer();

        protected override ParserRuleContext Parse(Antlr4.Runtime.Parser parser) => ((TSqlParser) parser).tsql_file();

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree) =>
            new TSqlAntlrParseTree((TSqlParser.Tsql_fileContext)syntaxTree);

        protected override string ParserSerializedATN => TSqlParser._serializedATN;
    }
}
