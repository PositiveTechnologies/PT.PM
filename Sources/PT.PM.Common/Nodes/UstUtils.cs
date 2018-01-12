using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes
{
    public static class UstUtils
    {
        public static Statement ToStatementIfRequired(this Ust ust)
        {
            Statement result = ust as Statement;
            if (result == null)
            {
                if (ust is Expression expr)
                {
                    result = new ExpressionStatement(expr);
                }
                else if (ust != null)
                {
                    result = new WrapperStatement(ust);
                }
                else
                {
                    result = null;
                }
            }
            return result;
        }

        public static Expression ToExpressionIfRequired(this Ust ust)
        {
            if (ust == null)
            {
                return null;
            }

            Expression result = ust as Expression;
            if (result == null)
            {
                result = new WrapperExpression(ust);
            }
            return result;
        }

        public static bool IsInsideSingleBlockStatement(this Ust ust)
        {
            Ust parent = ust;
            while (parent != null && !(parent is Statement))
            {
                parent = parent.Parent;
            }

            if (parent == null)
            {
                return false;
            }

            Statement statement = (Statement)parent;

            return statement.Parent is Statement parentStatement && !(parentStatement is BlockStatement);
        }

        public static Ust[] SelectAnalyzedNodes(this Ust ust, Language language, HashSet<Language> analyzedLanguages)
        {
            Ust[] result;
            if (analyzedLanguages.Contains(language))
            {
                result = new Ust[] { ust };
            }
            else
            {
                result = ust.WhereDescendants(
                    node => node is RootUst rootUst && analyzedLanguages.Contains(rootUst.Language))
                    .Cast<RootUst>()
                    .ToArray();
            }
            return result;
        }

        public static void FillAscendants(this Ust ust)
        {
            if (ust == null)
            {
                return;
            }

            FillAscendantsLocal(ust, ust as RootUst);

            void FillAscendantsLocal(Ust localUst, RootUst root)
            {
                foreach (Ust child in localUst.Children)
                {
                    if (child != null)
                    {
                        child.Parent = localUst;
                        child.Root = root;
                        if (child is RootUst rootUstChild)
                        {
                            FillAscendants(rootUstChild);
                        }
                        else
                        {
                            FillAscendantsLocal(child, root);
                        }
                    }
                }
            }
        }

        public static TextSpan GetTextSpan(this IEnumerable<Ust> usts)
        {
            if (usts.Count() == 0)
            {
                return default(TextSpan);
            }
            else
            {
                return usts.First().TextSpan.Union(usts.Last().TextSpan);
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
                        return -list2[i].KindId;
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
