using Esprima;
using Esprima.Ast;
using PT.PM.Common;
using System;
using System.Collections.Generic;

namespace PT.PM.JavaScriptParseTreeUst
{
    public class JavaScriptEsprimaParseTree : ParseTree
    {
        public override Language SourceLanguage => JavaScript.Language;

        public Program SyntaxTree { get; }

        public List<Comment> Comments { get; }

        public JavaScriptEsprimaParseTree()
        {
        }

        public JavaScriptEsprimaParseTree(Program syntaxTree, List<Comment> comments)
        {
            SyntaxTree = syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree));
            Comments = comments ?? throw new ArgumentNullException(nameof(comments));
        }
    }
}
