using System;
using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PythonParseTree;
using static PythonParseTree.PythonLexer;

namespace PT.PM.PythonParseTreeUst
{
    public class PythonAntlrParser : AntlrParser
    {
        public static PythonAntlrParser Create() => new PythonAntlrParser();

        public override Language Language => Language.Python;

        public override IVocabulary LexerVocabulary { get; } = DefaultVocabulary;

        public override string[] RuleNames => PythonParser.ruleNames;

        public override int[] IdentifierTokenTypes { get; } = {NAME};

        public override Type[] IdentifierRuleType { get; } = {typeof(PythonParser.NameContext)};

        public override int[] StringTokenTypes { get; } = {STRING};

        public override int NullTokenType { get; } = NONE;

        public override int TrueTokenType { get; } = TRUE;

        public override int FalseTokenType { get; } = FALSE;

        public override int ThisTokenType { get; } = ZeroTokenType; // TODO: SELF;

        public override int DecNumberTokenType { get; } = DECIMAL_INTEGER;

        public override int HexNumberTokenType { get; } = HEX_INTEGER;

        public override int OctNumberTokenType { get; } = OCT_INTEGER;

        public override int BinNumberTokenType { get; } = BIN_INTEGER;

        public override int FloatNumberTokenType { get; } = FLOAT_NUMBER;

        public override int[] CommentTokenTypes { get; } = {COMMENT};

        protected override string ParserSerializedATN => PythonParser._serializedATN;

        protected override Parser InitParser(ITokenStream inputStream)
            => new PythonParser(inputStream);

        protected override AntlrParseTree CreateParseTree(ParserRuleContext syntaxTree)
            => new PythonAntlrParseTree((PythonParser.RootContext) syntaxTree);

        protected override ParserRuleContext Parse(Parser parser)
            => ((PythonParser) parser).root();
    }
}