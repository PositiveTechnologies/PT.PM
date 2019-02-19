using System;
using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.LanguageDetectors;

namespace PT.PM
{
    public class SqlDialectAntlrParser : AntlrParser
    {
        private Language detectedLanguage = Language.PlSql;
        public override Language Language => detectedLanguage;
        public override CaseInsensitiveType CaseInsensitiveType => CaseInsensitiveType.UPPER;

        public static SqlDialectAntlrParser Create() => new SqlDialectAntlrParser();

        protected override IVocabulary Vocabulary => SqlDialectsLexer.DefaultVocabulary;

        protected override int CommentsChannel => SqlDialectsLexer.Hidden;

        protected override string LexerSerializedATN => SqlDialectsLexer._serializedATN;

        protected override string ParserSerializedATN => SqlDialectsLexer._serializedATN;

        protected override Lexer InitLexer(ICharStream inputStream) =>
            new SqlDialectsLexer(inputStream);

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree) =>
            throw new NotImplementedException();

        protected override Parser InitParser(ITokenStream inputStream) =>
            throw new NotImplementedException();

        protected override ParserRuleContext Parse(Parser parser) =>
            throw new NotImplementedException();
    }
}