using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.SqlParseTreeUst.Parser;
using PT.PM.TSqlParseTreeUst.Parser;
using Antlr4.Runtime;

namespace PT.PM.SqlParseTreeUst
{
    public class TSqlAntlrParser : AntlrParser
    {
        public override Language Language => Language.TSql;

        public TSqlAntlrParser()
        {
        }

        protected override int CommentsChannel => tsqlLexer.Hidden;

        protected override IVocabulary Vocabulary => tsqlLexer.DefaultVocabulary;

        protected override Lexer InitLexer(ICharStream inputStream)
        {
            return new tsqlLexer(inputStream);
        }

        protected override Antlr4.Runtime.Parser InitParser(ITokenStream inputStream)
        {
            return new tsqlParser(inputStream);
        }

        protected override ParserRuleContext Parse(Antlr4.Runtime.Parser parser)
        {
            return ((tsqlParser) parser).tsql_file();
        }

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree)
        {
            return new TSqlAntlrParseTree((tsqlParser.Tsql_fileContext)syntaxTree);
        }
    }
}
