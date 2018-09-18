using Esprima.Ast;
using PT.PM.Common;

namespace PT.PM.JavaScriptParseTreeUst
{
    public class JavaScriptEsprimaParseTree : ParseTree
    {
        public override Language SourceLanguage => JavaScript.Language;

        public Program SyntaxTree { get; }

        public JavaScriptEsprimaParseTree()
        {
        }

        public JavaScriptEsprimaParseTree(Program syntaxTree)
        {
            SyntaxTree = syntaxTree;
        }
    }
}
