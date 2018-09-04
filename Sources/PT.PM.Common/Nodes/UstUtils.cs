using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Nodes.TypeMembers;
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


        public static BlockStatement GetCurrentBlockStatement(this Ust ust)
        {
            if (ust.Parent == null)
            {
                return null;
            }

            if (ust.Parent is BlockStatement block)
            {
                return block;
            }

            return ust.Parent.GetCurrentBlockStatement();
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

        internal static string GenerateSignature(string id, List<ParameterDeclaration> parameters)
        {
            string paramsString = string.Join(",", parameters.Select(p => p.Type?.TypeText ?? "Any"));
            return $"{id}({paramsString})";
        }

        public static List<TextSpan> GetAlignedTextSpan(Ust ust, TextSpan[] matchesLocations)
        {
            List<TextSpan> result = new List<TextSpan>();
            var initialTextSpans = ust.InitialTextSpans.OrderBy(x => x).ToList();
            var escapeLength = (ust as StringLiteral)?.EscapeCharsLength ?? 1;

            foreach (TextSpan location in matchesLocations)
            {
                int offset = 0;
                int leftBound = 1;
                int rightBound =
                    initialTextSpans[0].Length - 2 * escapeLength + 1; // - quotes length and then + 1
                TextSpan textSpan = TextSpan.Zero;

                // Check first initial textspan separately
                if (location.Start < rightBound && location.End > rightBound)
                {
                    textSpan = location;
                }

                for (int i = 1; i < initialTextSpans.Count; i++)
                {
                    var initTextSpan = initialTextSpans[i];
                    var prevTextSpan = initialTextSpans[i - 1];
                    leftBound += prevTextSpan.Length - 2 * escapeLength;
                    rightBound += initTextSpan.Length - 2 * escapeLength;
                    offset += initTextSpan.Start - prevTextSpan.End + 2 * escapeLength;

                    if (location.Start < leftBound && location.End < leftBound)
                    {
                        break;
                    }

                    if (location.Start >= leftBound && location.Start < rightBound)
                    {
                        textSpan = location.AddOffset(offset);
                        if (location.End <= rightBound)
                        {
                            result.Add(textSpan);
                            break;
                        }
                    }

                    if (!textSpan.IsZero && location.End <= rightBound)
                    {
                        result.Add(new TextSpan(textSpan.Start, location.Length + offset, textSpan.CodeFile));
                        break;
                    }
                }

                if (textSpan.IsZero)
                {
                    result.Add(location);
                }
            }
            return result;
        }

        public static bool IsInsideSingleBlockStatement(this Ust ust)
        {
            Ust parent = ust;
            Ust prev = ust;
            while (parent != null && !(parent is Statement) &&
                 !(parent is ConditionalExpression conditionalExpression && !ReferenceEquals(conditionalExpression.Condition, prev)))
            {
                prev = parent;
                parent = parent.Parent;
            }

            if (parent == null)
            {
                return false;
            }

            if (parent is ConditionalExpression)
            {
                return true;
            }

            Statement statement = (Statement)parent;
            return statement.Parent is Statement parentStatement && !(parentStatement is BlockStatement);
        }

        public static AssignmentExpression CreateAssignExpr(Expression left, Expression right, TextSpan textSpan, string assignExprOpText, TextSpan assignOpTextSpan)
        {
            BinaryOperatorLiteral binaryOperator = null;

            if (assignExprOpText != null && assignExprOpText.Length > 1)
            {
                var operatorText = assignExprOpText.Remove(assignExprOpText.Length - 1);
                binaryOperator = new BinaryOperatorLiteral(operatorText, assignOpTextSpan);
            }

            return new AssignmentExpression(left, right, textSpan)
            {
                Operator = binaryOperator
            };
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
                result = ust.WhereDescendantsOrSelf(
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
                return usts.First().TextSpan.Union(usts.LastOrDefault(x => !x.TextSpan.IsZero)?.TextSpan ?? TextSpan.Zero);
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
