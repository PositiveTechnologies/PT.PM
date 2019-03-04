using PT.PM.Common;
using System;
using Esprima;
using Esprima.Ast;
using Collections = System.Collections.Generic;

namespace PT.PM.JavaScriptParseTreeUst
{
    public class JavaScriptEsprimaParseTree : ParseTree
    {
        public override Language SourceLanguage => Language.JavaScript;

        public Program SyntaxTree { get; }

        public Collections.List<Comment> Comments { get; }

        public JavaScriptEsprimaParseTree()
        {
        }

        public JavaScriptEsprimaParseTree(Program syntaxTree, Collections.List<Comment> comments)
        {
            SyntaxTree = syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree));
            Comments = comments ?? throw new ArgumentNullException(nameof(comments));
        }
    }
}
