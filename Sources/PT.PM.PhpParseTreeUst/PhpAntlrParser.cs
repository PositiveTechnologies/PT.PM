using System;
using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using static PT.PM.PhpParseTreeUst.PhpLexer;

namespace PT.PM.PhpParseTreeUst
{
    public class PhpAntlrParser : AntlrParser
    {
        public static PhpAntlrParser Create() => new PhpAntlrParser();

        public override Language Language => Language.Php;

        public override IVocabulary LexerVocabulary { get; } = DefaultVocabulary;

        public override string[] RuleNames => PhpParser.ruleNames;

        public override int[] IdentifierTokenTypes { get; } = {VarName, Label};

        public override Type[] IdentifierRuleType { get; } = {typeof(PhpParser.IdentifierContext)};

        public override int[] StringTokenTypes { get; } = {SingleQuoteString, StringPart};

        public override int NullTokenType { get; } = Null;

        public override int TrueTokenType { get; } = BooleanConstant; // TODO: split in grammar (true and false)

        public override int FalseTokenType { get; } = BooleanConstant; // TODO: split in grammar (true and false)

        public override int ThisTokenType { get; } = -1;

        public override int DecNumberTokenType { get; } = PhpLexer.Decimal;

        public override int HexNumberTokenType { get; } = Hex;

        public override int OctNumberTokenType { get; } = Octal;

        public override int BinNumberTokenType { get; } = Binary;

        public override int FloatNumberTokenType { get; } = Real;

        public override int[] CommentTokenTypes { get; } =
            {SingleLineComment, MultiLineComment, ShellStyleComment, Comment};

        protected override Parser InitParser(ITokenStream inputStream) => new PhpParser(inputStream);

        protected override ParserRuleContext Parse(Parser parser) => ((PhpParser) parser).htmlDocument();

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree) =>
            new PhpAntlrParseTree((PhpParser.HtmlDocumentContext) syntaxTree);

        protected override string ParserSerializedATN => PhpParser._serializedATN;
    }
}