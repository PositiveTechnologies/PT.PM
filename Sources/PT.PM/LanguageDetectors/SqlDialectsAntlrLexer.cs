using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.LanguageDetectors;

namespace PT.PM
{
    public class SqlDialectsAntlrLexer : AntlrLexer
    {
        public override Language Language => Language.PlSql;

        public override CaseInsensitiveType CaseInsensitiveType => CaseInsensitiveType.UPPER;

        protected override string LexerSerializedATN => SqlDialectsLexer._serializedATN;

        public override IVocabulary Vocabulary => SqlDialectsLexer.DefaultVocabulary;

        public override Lexer InitLexer(ICharStream inputStream) =>
            new SqlDialectsLexer(inputStream);
    }
}
