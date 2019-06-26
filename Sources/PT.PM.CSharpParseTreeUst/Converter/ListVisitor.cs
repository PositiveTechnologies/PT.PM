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

        public override Ust VisitParameterList(ParameterListSyntax node)
        {
            throw new InvalidOperationException();
        }
    }
}
