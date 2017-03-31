using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes
{
    public static class NodeHelpers
    {
        public static Statement ToStatementIfRequired(this UstNode node)
        {
            Statement result = node as Statement;
            if (result == null)
            {
                Expression expr = node as Expression;
                if (expr != null)
                {
                    result = new ExpressionStatement(expr);
                }
                else
                {
                    result = new WrapperStatement(node);
                }
            }
            return result;
        }

        public static Expression ToExpressionIfRequired(this UstNode node)
        {
            if (node == null)
            {
                return null;
            }

            Expression result = node as Expression;
            if (result == null)
            {
                result = new WrapperExpression(node);
            }
            return result;
        }

        public static UstNode RemoveNotIncludedNodes(this UstNode root, Language mainLanguage, LanguageFlags dependentLanguages)
        {
            UstNode result;
            if (dependentLanguages.Is(mainLanguage))
            {
                result = root;
            }
            else
            {
                var includedChildren = root.GetAllDescendants(node =>
                    node.NodeType == NodeType.NamespaceDeclaration &&
                    dependentLanguages.Is(((NamespaceDeclaration)node).Language) &&
                    dependentLanguages.ToString().Contains(((NamespaceDeclaration)node).Name.Text));
                result = CreateRootNamespace(includedChildren, mainLanguage, root.FileNode);
            }
            return result;
        }
        public static NamespaceDeclaration CreateRootNamespace(this IEnumerable<UstNode> members, Language language, FileNode fileNode)
        {
            return new NamespaceDeclaration(new StringLiteral("root"), members, language, members.GetTextSpan(), fileNode);
        }

        public static NamespaceDeclaration CreateLanguageNamespace(this IEnumerable<UstNode> members, Language language, FileNode fileNode)
        {
            return new NamespaceDeclaration(new StringLiteral(language.ToString()), members, language, members.GetTextSpan(), fileNode);
        }

        public static NamespaceDeclaration CreateLanguageNamespace(this UstNode member, Language language, FileNode fileNode)
        {
            return new NamespaceDeclaration(new StringLiteral(language.ToString()), new UstNode[] { member }, language, member.TextSpan, fileNode);
        }
    }
}
