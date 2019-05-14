using System;
using System.Linq;
using PT.PM.Common.Nodes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.CSharpParseTreeUst.RoslynUstVisitor
{
    public partial class CSharpRoslynParseTreeConverter
    {
        public override Ust VisitAccessorList(AccessorListSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitArgumentList(ArgumentListSyntax node)
        {
            Expression[] args = node.Arguments.Select(arg => (Expression)VisitAndReturnNullIfError(arg))
                .ToArray();

            var result = new ArgsUst(args, node.GetTextSpan());
            return result;
        }

        public override Ust VisitAttributeArgumentList(AttributeArgumentListSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override Ust VisitAttributeList(AttributeListSyntax node)
        {
            return null;
        }

        public override Ust VisitBracketedArgumentList(BracketedArgumentListSyntax node)
        {
            Expression[] args = node.Arguments.Select(arg => (Expression)VisitAndReturnNullIfError(arg))
                .ToArray();

            var result = new ArgsUst(args, node.GetTextSpan());
            return result;
        }

        public override Ust VisitBracketedParameterList(BracketedParameterListSyntax node)
        {
            return base.VisitBracketedParameterList(node);
        }

        public override Ust VisitCrefBracketedParameterList(CrefBracketedParameterListSyntax node)
        {
            return base.VisitCrefBracketedParameterList(node);
        }

        public override Ust VisitCrefParameterList(CrefParameterListSyntax node)
        {
            return base.VisitCrefParameterList(node);
        }

        public override Ust VisitTypeArgumentList(TypeArgumentListSyntax node)
        {
            return base.VisitTypeArgumentList(node);
        }

        public override Ust VisitTypeParameterList(TypeParameterListSyntax node)
        {
            return base.VisitTypeParameterList(node);
        }

        public override Ust VisitParameterList(ParameterListSyntax node)
        {
            throw new InvalidOperationException();
        }
    }
}
