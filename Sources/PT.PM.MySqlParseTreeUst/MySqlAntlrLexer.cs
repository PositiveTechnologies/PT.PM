using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.MySqlParseTreeUst;

namespace PT.PM.SqlParseTreeUst
{
    public class MySqlAntlrLexer : AntlrLexer
    {
        public static MySqlAntlrLexer Create() => new MySqlAntlrLexer();
        
        public override Language Language => Language.MySql;

        public override CaseInsensitiveType CaseInsensitiveType => CaseInsensitiveType.UPPER;

        protected override IVocabulary Vocabulary => MySqlLexer.DefaultVocabulary;

        public override Lexer InitLexer(ICharStream inputStream) => new MySqlLexer(inputStream);

        protected override string LexerSerializedATN => MySqlLexer._serializedATN;
    }
}