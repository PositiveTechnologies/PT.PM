using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes
{
    public static class UstUtils
    {
        public static Statement ToStatementIfRequired(this Ust node)
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

        public static Expression ToExpressionIfRequired(this Ust node)
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

        public static BlockStatement ToBlockStatementIfRequired(this Statement statement)
        {
            if (statement is BlockStatement blockStatement)
            {
                return blockStatement;
            }
            else
            {
                return new BlockStatement(new Statement[] { statement });
            }
        }

        public static Ust[] SelectAnalyzedNodes(this Ust ustNode, Language language, HashSet<Language> analyzedLanguages)
        {
            Ust[] result;
            if (analyzedLanguages.Contains(language))
            {
                result = new Ust[] { ustNode };
            }
            else
            {
                result = ustNode.WhereDescendants(
                    node => node is RootUst rootUstNode && analyzedLanguages.Contains(rootUstNode.Language))
                    .Cast<RootUst>()
                    .ToArray();
            }
            return result;
        }

        public static void FillAscendants(this Ust ustNode)
        {
            if (ustNode == null)
            {
                return;
            }

            FillAscendantsHelper(ustNode, ustNode as RootUst);

            void FillAscendantsHelper(Ust node, RootUst root)
            {
                foreach (Ust child in node.Children)
                {
                    if (child != null)
                    {
                        child.Parent = node;
                        child.Root = root;
                        if (child is RootUst rootUstChild)
                        {
                            FillAscendants(rootUstChild);
                        }
                        else
                        {
                            FillAscendantsHelper(child, root);
                        }
                    }
                }
            }
        }

        public static TextSpan GetTextSpan(this IEnumerable<Ust> nodes)
        {
            if (nodes.Count() == 0)
            {
                return default(TextSpan);
            }
            else
            {
                return nodes.First().TextSpan.Union(nodes.Last().TextSpan);
            }
        }

        public static int CompareTo<T>(this IEnumerable<T> collection1, IEnumerable<T> collection2) where T : Ust
        {
            var list1 = collection1 as IList<T> ?? new List<T>(collection1 ?? Enumerable.Empty<T>());
            var list2 = collection2 as IList<T> ?? new List<T>(collection2 ?? Enumerable.Empty<T>());

            var collectionCountCompareResult = list1.Count - list2.Count;
            if (collectionCountCompareResult != 0)
            {
                return collectionCountCompareResult;
            }

            for (int i = 0; i < list1.Count; i++)
            {
                var element = list1[i];
                if (element == null)
                {
                    if (list2[i] != null)
                    {
                        return -list2[i].Kind;
                    }
                }
                else if (!(element is RootUst))
                {
                    var elementCompareResult = element.CompareTo(list2[i]);
                    if (elementCompareResult != 0)
                    {
                        return elementCompareResult;
                    }
                }
            }

            return 0;
        }
    }
}
