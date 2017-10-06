using PT.PM.Common;
using Microsoft.CodeAnalysis;

namespace PT.PM.CSharpParseTreeUst
{
    public class CSharpRoslynParseTree : ParseTree
    {
        public override Language SourceLanguage => CSharp.Language;

        public SyntaxTree SyntaxTree { get; }

        public SyntaxTrivia[] Comments = ArrayUtils<SyntaxTrivia>.EmptyArray;

        public CSharpRoslynParseTree()
        {
        }

        public CSharpRoslynParseTree(SyntaxTree syntaxTree)
        {
            SyntaxTree = syntaxTree;
        }
    }
}
