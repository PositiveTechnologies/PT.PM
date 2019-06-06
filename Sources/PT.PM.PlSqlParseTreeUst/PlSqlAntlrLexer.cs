using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.PlSqlParseTreeUst;

namespace PT.PM.PlSqlParseTreeUst
{
    public class PlSqlAntlrLexer : AntlrLexer
    {

        public static PlSqlAntlrLexer Create() => new PlSqlAntlrLexer();

        public override Language Language => Language.PlSql;

        public override CaseInsensitiveType CaseInsensitiveType => CaseInsensitiveType.UPPER;

        public override IVocabulary Vocabulary => PlSqlLexer.DefaultVocabulary;

        public override Lexer InitLexer(ICharStream inputStream) => new PlSqlLexer(inputStream);

        protected override string LexerSerializedATN => PlSqlLexer._serializedATN;
    }
}
