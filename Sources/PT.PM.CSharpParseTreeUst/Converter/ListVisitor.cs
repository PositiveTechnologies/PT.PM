using System;
using System.Linq;
using PT.PM.Common.Nodes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.CSharpParseTreeUst.RoslynUstVisitor
{
    public partial class RoslynUstCommonConverterVisitor
    {
        public override UstNode VisitAccessorList(AccessorListSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitArgumentList(ArgumentListSyntax node)
        {
            Expression[] args = node.Arguments.Select(arg => (Expression)VisitAndReturnNullIfError(arg))
                .ToArray();

            var result = new ArgsNode(args, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitAttributeArgumentList(AttributeArgumentListSyntax node)
        {
            throw new InvalidOperationException();
        }

        public override UstNode VisitAttributeList(AttributeListSyntax node)
        {
            return null;
        }

        public override UstNode VisitBaseList(BaseListSyntax node)
        {
            return base.VisitBaseList(node);
        }

        public override UstNode VisitBracketedArgumentList(BracketedArgumentListSyntax node)
        {
            Expression[] args = node.Arguments.Select(arg => (Expression)VisitAndReturnNullIfError(arg))
                .ToArray();

            var result = new ArgsNode(args, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitBracketedParameterList(BracketedParameterListSyntax node)
        {
            return base.VisitBracketedParameterList(node);
        }

        public override UstNode VisitCrefBracketedParameterList(CrefBracketedParameterListSyntax node)
        {
            return base.VisitCrefBracketedParameterList(node);
        }

        public override UstNode VisitCrefParameterList(CrefParameterListSyntax node)
        {
            return base.VisitCrefParameterList(node);
        }

        public override UstNode VisitTypeArgumentList(TypeArgumentListSyntax node)
        {
            return base.VisitTypeArgumentList(node);
        }

        public override UstNode VisitTypeParameterList(TypeParameterListSyntax node)
        {
            return base.VisitTypeParameterList(node);
        }

        public override UstNode VisitParameterList(ParameterListSyntax node)
        {
            throw new InvalidOperationException();
        }
    }
}
