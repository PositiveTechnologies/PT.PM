using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PythonParseTree;

namespace PT.PM.PythonParseTreeUst
{
    public class PythonAntlrParser : AntlrParser
    {
        public override Language Language => Language.Python;

        public override string[] RuleNames => PythonParser.ruleNames;

        protected override string ParserSerializedATN => PythonParser._serializedATN;

        protected override int CommentsChannel => PythonLexer.Hidden;

        protected override Parser InitParser(ITokenStream inputStream)
            => new PythonParser(inputStream);

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree)
            => new PythonAntlrParseTree((PythonParser.RootContext) syntaxTree);

        public static PythonAntlrParser Create() => new PythonAntlrParser();

        protected override ParserRuleContext Parse(Parser parser)
            => ((PythonParser) parser).root();
    }
}