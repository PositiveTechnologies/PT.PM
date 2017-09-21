using System;
using PT.PM.Common.Nodes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PT.PM.CSharpParseTreeUst.RoslynUstVisitor
{
    public partial class CSharpRoslynParseTreeConverter
    {
        public override Ust VisitBadDirectiveTrivia(BadDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitDefineDirectiveTrivia(DefineDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitElifDirectiveTrivia(ElifDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitElseDirectiveTrivia(ElseDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitEndIfDirectiveTrivia(EndIfDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitEndRegionDirectiveTrivia(EndRegionDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitErrorDirectiveTrivia(ErrorDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitLineDirectiveTrivia(LineDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitPragmaChecksumDirectiveTrivia(PragmaChecksumDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitPragmaWarningDirectiveTrivia(PragmaWarningDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitReferenceDirectiveTrivia(ReferenceDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitSkippedTokensTrivia(SkippedTokensTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitUndefDirectiveTrivia(UndefDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitWarningDirectiveTrivia(WarningDirectiveTriviaSyntax node)
        {
            throw new InvalidOperationException();
        }
    }
}
