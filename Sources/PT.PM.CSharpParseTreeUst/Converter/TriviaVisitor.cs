using System;
using PT.PM.Common.Nodes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PT.PM.CSharpParseTreeUst.RoslynUstVisitor
{
    public partial class CSharpRoslynParseTreeConverter
    {
        public override UstNode VisitBadDirectiveTrivia(BadDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitDefineDirectiveTrivia(DefineDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitElifDirectiveTrivia(ElifDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitElseDirectiveTrivia(ElseDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitEndIfDirectiveTrivia(EndIfDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitEndRegionDirectiveTrivia(EndRegionDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitErrorDirectiveTrivia(ErrorDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitLineDirectiveTrivia(LineDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitPragmaChecksumDirectiveTrivia(PragmaChecksumDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitPragmaWarningDirectiveTrivia(PragmaWarningDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitReferenceDirectiveTrivia(ReferenceDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitSkippedTokensTrivia(SkippedTokensTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitUndefDirectiveTrivia(UndefDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitWarningDirectiveTrivia(WarningDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }
    }
}
