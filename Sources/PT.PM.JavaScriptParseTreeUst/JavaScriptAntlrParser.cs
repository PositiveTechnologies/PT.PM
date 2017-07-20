using PT.PM.AntlrUtils;
using PT.PM.Common;
using Antlr4.Runtime;
using PT.PM.JavaScriptParseTreeUst.Parser;

namespace PT.PM.JavaScriptParseTreeUst
{
    public class JavaScriptAntlrParser: AntlrParser
    {
        private bool useStrict = false;

        public JavaScriptType JavaScriptType { get; set; } = JavaScriptType.Undefined;

        public override Language Language => Language.JavaScript;

        protected override int CommentsChannel => JavaScriptLexer.Hidden;

        protected override IVocabulary Vocabulary => JavaScriptParser.DefaultVocabulary;

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree)
        {
            return new JavaScriptAntlrParseTree((JavaScriptParser.ProgramContext)syntaxTree);
        }

        protected override Lexer InitLexer(ICharStream inputStream)
        {
            var lexer = new JavaScriptLexer(inputStream);
            lexer.UseStrict = JavaScriptType == JavaScriptType.Undefined
                ? useStrict
                : JavaScriptType == JavaScriptType.Strict;

            return lexer;
        }

        protected override Antlr4.Runtime.Parser InitParser(ITokenStream inputStream)
        {
            return new JavaScriptParser(inputStream);
        }

        protected override ParserRuleContext Parse(Antlr4.Runtime.Parser parser)
        {
            return ((JavaScriptParser)parser).program();
        }

        protected override string PreprocessText(SourceCodeFile file)
        {
            var result = base.PreprocessText(file);
            if (JavaScriptType == JavaScriptType.Undefined)
            {
                useStrict = result.StartsWith("\"use strict\"");
            }
            return result;
        }
    }
}
