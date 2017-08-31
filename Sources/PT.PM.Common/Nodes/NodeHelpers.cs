using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes
{
    public static class NodeHelper
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
                else if (node != null)
                {
                    result = new WrapperStatement(node);
                }
                else
                {
                    result = null;
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

        public static UstNode[] SelectAnalyzedNodes(this UstNode ustNode, Language language, HashSet<Language> analyzedLanguages)
        {
            UstNode[] result;
            if (analyzedLanguages.Contains(language))
            {
                result = new UstNode[] { ustNode };
            }
            else
            {
                result = ustNode.GetAllDescendants(
                    node => node is RootNode rootUstNode && analyzedLanguages.Contains(rootUstNode.Language))
                    .Cast<RootNode>()
                    .ToArray();
            }
            return result;
        }

        public static void FillAscendants(this UstNode ustNode)
        {
            if (ustNode == null)
            {
                return;
            }

            FillAscendants(ustNode, ustNode as RootNode);
        }

        private static void FillAscendants(UstNode node, RootNode root)
        {
            foreach (UstNode child in node.Children)
            {
                if (child != null)
                {
                    child.Parent = node;
                    child.Root = root;
                    if (child is RootNode rootUstChild)
                    {
                        FillAscendants(rootUstChild);
                    }
                    else
                    {
                        FillAscendants(child, root);
                    }
                }
            }
        }
    }
}
