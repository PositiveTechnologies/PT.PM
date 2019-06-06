using System;
using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using static PT.PM.JavaParseTreeUst.JavaLexer;

namespace PT.PM.JavaParseTreeUst
{
    public class JavaAntlrParser : AntlrParser
    {
        public static JavaAntlrParser Create() => new JavaAntlrParser();

        public override Language Language => Language.Java;

        public override IVocabulary LexerVocabulary { get; } = DefaultVocabulary;

        public override string[] RuleNames => JavaParser.ruleNames;

        public override int[] IdentifierTokenTypes { get; } = {IDENTIFIER};

        public override Type[] IdentifierRuleType { get; } = { };

        public override int[] StringTokenTypes { get; } = {CHAR_LITERAL, STRING_LITERAL};

        public override int NullTokenType { get; } = NULL_LITERAL;

        public override int TrueTokenType { get; } = BOOL_LITERAL; // TODO: split

        public override int FalseTokenType { get; } = BOOL_LITERAL; // TODO: split

        public override int ThisTokenType { get; } = THIS;

        public override int DecNumberTokenType { get; } = DECIMAL_LITERAL;

        public override int HexNumberTokenType { get; } = HEX_LITERAL;

        public override int OctNumberTokenType { get; } = OCT_LITERAL;

        public override int BinNumberTokenType { get; } = BINARY_LITERAL;

        public override int FloatNumberTokenType { get; } = FLOAT_LITERAL;

        public override int[] CommentTokenTypes { get; } = {COMMENT, LINE_COMMENT};

        protected override Parser InitParser(ITokenStream inputStream) =>
            new JavaParser(inputStream);

        protected override ParserRuleContext Parse(Parser parser) =>
            ((JavaParser) parser).compilationUnit();

        protected override AntlrParseTree CreateParseTree(ParserRuleContext syntaxTree) =>
            new JavaAntlrParseTree((JavaParser.CompilationUnitContext) syntaxTree);

        protected override string ParserSerializedATN => JavaParser._serializedATN;
    }
}