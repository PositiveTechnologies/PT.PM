using PT.PM.Common;
using Microsoft.CodeAnalysis;

namespace PT.PM.CSharpParseTreeUst
{
    public static class RoslynHelper
    {
        public static TextSpan GetTextSpan(this SyntaxNodeOrToken node)
        {
            return ConvertTextSpan(node.GetLocation());
        }

        public static TextSpan GetTextSpan(this SyntaxTrivia node)
        {
            return ConvertTextSpan(node.GetLocation());
        }

        public static TextSpan GetTextSpan(this SyntaxToken node)
        {
            return ConvertTextSpan(node.GetLocation());
        }

        public static TextSpan GetTextSpan(this SyntaxNode node)
        {
            return ConvertTextSpan(node.GetLocation());
        }

        internal static TextSpan ConvertTextSpan(Location location)
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
