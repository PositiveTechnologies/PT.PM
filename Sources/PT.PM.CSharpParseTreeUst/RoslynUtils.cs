using PT.PM.Common;
using Microsoft.CodeAnalysis;

namespace PT.PM.CSharpParseTreeUst
{
    public static class RoslynUtils
    {
        public static TextSpan GetTextSpan(this SyntaxNodeOrToken node)
        {
            return ToTextSpan(node.GetLocation());
        }

        public static TextSpan GetTextSpan(this SyntaxTrivia node)
        {
            return ToTextSpan(node.GetLocation());
        }

        public static TextSpan GetTextSpan(this SyntaxToken node)
        {
            return ToTextSpan(node.GetLocation());
        }

        public static TextSpan GetTextSpan(this SyntaxNode node)
        {
            return ToTextSpan(node.GetLocation());
        }

        internal static TextSpan ToTextSpan(this Location location)
        {
            var lineSpan = location.GetLineSpan();
            var startLineColumnPos = lineSpan.StartLinePosition;
            var endLineColumnPos = lineSpan.EndLinePosition;

            var result = new TextSpan(location.SourceSpan.Start, location.SourceSpan.Length);
            return result;
        }

        internal static TextSpan GetTextSpan(this global::AspxParser.Location location)
        {
            return new TextSpan(location.Start, location.Length);
        }
    }
}
