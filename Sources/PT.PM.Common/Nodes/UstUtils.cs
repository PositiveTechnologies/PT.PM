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
            }

            return result;
        }

        public static Expression ToExpressionIfRequired(this Ust ust)
        {
            if (ust == null)
            {
                return null;
            }

            return ust as Expression
                   ?? new WrapperExpression(ust);
        }

        public static Expression GetArgWithoutModifier(Expression arg)
        {
            return arg is ArgumentExpression argExpression ? argExpression.Argument : arg;
        }

        internal static string GenerateSignature(string id, List<ParameterDeclaration> parameters)
        {
            string paramsString = string.Join(",", parameters.Select(p => p.Type?.TypeText ?? "Unknown"));
            return $"{id}({paramsString})";
        }

        public static List<TextSpan> GetAlignedTextSpan(int escapeLength, List<TextSpan> foldedTextSpans, List<TextSpan> matchesLocations, int startOffset)
        {
            List<TextSpan> result = new List<TextSpan>(matchesLocations.Count);

            foreach (TextSpan location in matchesLocations)
            {
                if (foldedTextSpans == null || foldedTextSpans.Count == 0)
                {
                    result.Add(location.AddOffset(startOffset));
                    continue;
                }

                int offset = 0;
                int leftBound = 1;
                int rightBound =
                    foldedTextSpans[0].Length - 2 * escapeLength + 1; // - quotes length and then + 1
                TextSpan textSpan = TextSpan.Zero;

                // Check first initial textspan separately
                if (location.Start < rightBound && location.End > rightBound)
                {
                    textSpan = location;
                }

                for (int i = 1; i < foldedTextSpans.Count; i++)
                {
                    var initTextSpan = foldedTextSpans[i];
                    var prevTextSpan = foldedTextSpans[i - 1];
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
                            result.Add(textSpan.AddOffset(startOffset));
                            break;
                        }
                    }

                    if (!textSpan.IsZero && location.End <= rightBound)
                    {
                        result.Add(new TextSpan(textSpan.Start + startOffset, location.Length + offset, textSpan.File));
                        break;
                    }
                }

                if (textSpan.IsZero)
                {
                    result.Add(location.AddOffset(startOffset));
                }
            }

            return result;
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
                        child.Root = root;
                        child.Parent = localUst;
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
            if (!usts.Any())
            {
                return default;
            }

            return usts.First().TextSpan.Union(usts.LastOrDefault(x => !x.TextSpan.IsZero)?.TextSpan ?? TextSpan.Zero);
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

        public static List<Expression> ExtractMultiChild(MultichildExpression multichild)
        {
            void ExtractMultiChildToLinear(MultichildExpression multichildExpression, List<Expression> result)
            {
                foreach (var expression in multichildExpression.Expressions)
                {
                    if (expression is MultichildExpression nested)
                    {
                        ExtractMultiChildToLinear(nested, result);
                    }
                    else
                    {
                        result.Add(expression);
                    }
                }
            }

            var linearResult = new List<Expression>();
            ExtractMultiChildToLinear(multichild, linearResult);

            return linearResult;
        }
    }
}
