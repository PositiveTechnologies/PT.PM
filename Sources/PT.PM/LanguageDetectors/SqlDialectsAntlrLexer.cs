using System;
using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.LanguageDetectors;

namespace PT.PM
{
    public class SqlDialectsAntlrLexer : AntlrLexer
    {
        private Language detectedLanguage = Language.PlSql;
        public override Language Language => detectedLanguage;

        public override CaseInsensitiveType CaseInsensitiveType => CaseInsensitiveType.UPPER;

        protected override string LexerSerializedATN => SqlDialectsLexer._serializedATN;
        
        protected override IVocabulary Vocabulary => SqlDialectsLexer.DefaultVocabulary;
        
        public override Lexer InitLexer(ICharStream inputStream) =>
            new SqlDialectsLexer(inputStream);

        public SqlDialectsAntlrLexer()
        {
            Lexer = InitLexer(null);
        }
    }
}
