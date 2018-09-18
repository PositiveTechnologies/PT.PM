using Esprima;
using Esprima.Ast;
using PT.PM.Common;
using System.IO;

namespace PT.PM.JavaScriptParseTreeUst
{
    public class JavaScriptEsprimaParser : ILanguageParser
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public JavaScriptType JavaScriptType { get; set; } = JavaScriptType.Undefined;

        public Language Language => JavaScript.Language;

        public bool IsActive => false;

        public ParseTree Parse(CodeFile sourceCodeFile)
        {
            JavaScriptEsprimaParseTree result = null;

            var filePath = Path.Combine(sourceCodeFile.RelativePath, sourceCodeFile.Name);
            if (sourceCodeFile.Code != null)
            {
                // TODO: implement errors handler
                var parserOptions = new ParserOptions(sourceCodeFile.FullName)
                {
                    Loc = true,
                    Comment = true,
                };
                var parser = new Esprima.JavaScriptParser(sourceCodeFile.Code, parserOptions);
                Program ast = parser.ParseProgram(JavaScriptType == JavaScriptType.Strict);
                // TODO: implement comments handling
                result = new JavaScriptEsprimaParseTree(ast);
            }
            else
            {
                result = new JavaScriptEsprimaParseTree();
            }
            result.SourceCodeFile = sourceCodeFile;

            return result;
        }
    }
}
