using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;

namespace PT.PM.PhpParseTreeUst
{
    public class PhpAntlrLexer : AntlrLexer
    {
        public override Language Language => Language.Php;

        public override CaseInsensitiveType CaseInsensitiveType => CaseInsensitiveType.lower;

        protected override string LexerSerializedATN => PhpLexer._serializedATN;

        protected override IVocabulary Vocabulary => PhpLexer.DefaultVocabulary;

        public override Lexer InitLexer(ICharStream inputStream) => new PhpLexer(inputStream);
    }
}