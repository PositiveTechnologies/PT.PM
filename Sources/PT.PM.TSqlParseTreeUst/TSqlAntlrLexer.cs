using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.TSqlParseTreeUst;

namespace PT.PM.SqlParseTreeUst
{
    public class TSqlAntlrLexer : AntlrLexer
    {
        public override Language Language => Language.TSql;

        public override CaseInsensitiveType CaseInsensitiveType => CaseInsensitiveType.UPPER;

        protected override IVocabulary Vocabulary => TSqlLexer.DefaultVocabulary;

        public override Lexer InitLexer(ICharStream inputStream) => new TSqlLexer(inputStream);

        protected override string LexerSerializedATN => TSqlLexer._serializedATN;
    }
}