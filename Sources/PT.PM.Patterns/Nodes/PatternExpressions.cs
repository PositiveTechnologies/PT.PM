using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Patterns.Nodes
{
    public class PatternExpressions : ArgsNode
    {
        private HashSet<PatternVarDef> pinnedPatternVarDefs = new HashSet<PatternVarDef>();

        public override NodeType NodeType => NodeType.PatternExpressions;

        [JsonIgnore]
        public TextSpan MatchedTextSpan { get; private set; }

        public PatternExpressions()
        {
            Collection = new Expression[] { new PatternMultipleExpressions() };
        }

        public PatternExpressions(params Expression[] expressions)
            : base(expressions)
        {
        }

        public override int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return 1;
            }

            if (other.NodeType == NodeType.PatternExpressions)
            {
                return base.CompareTo(other);
            }

            var result = NodeType.ArgsNode - other.NodeType;
            if (result != 0)
            {
                return result;
            }
            if (Collection == null)
            {
                return 0;
            }

            IList<Expression> otherExpressions = ((ArgsNode)other).Collection;

            bool includeNegative = Collection.Count(item =>
                item.NodeType == NodeType.PatternExpression && ((PatternExpression)item).Not) > 0;
            // TODO: not all cases work correctly.
            bool match = Match(otherExpressions);
            
            result = match ? 0 : 1;
            if (includeNegative)
                result = 1 - result;
            ResetPinValues();
            //MatchedTextSpan = otherExpressions[endIndex].TextSpan; //TODO: otherExpressions[startIndex].TextSpan.UnionWith(otherExpressions[endIndex].TextSpan);

            return result;
        }

        private bool Match(IList<Expression> otherExpressions)
        {
            int nextStateIndex = 0;
            bool matched = false;
            int lastMatchIndex = int.MinValue;
            int i = -1;

            if (Collection.Count > 0)
            {
                if (otherExpressions.Count != 0 || (Collection.Count == 1 && Collection[0].NodeType == NodeType.PatternMultipleExpressions))
                {
                    while (i < otherExpressions.Count)
                    {
                        if (Collection[nextStateIndex].NodeType == NodeType.PatternMultipleExpressions)
                        {
                            matched = true;
                            lastMatchIndex = i;
                            nextStateIndex += 1;
                            if (nextStateIndex < Collection.Count)
                            {
                                // Compare with next not PatternMultipleExpressions (start or continue point).
                                if (Match(otherExpressions, i, ref nextStateIndex))
                                {
                                    if (!matched && i != 0)
                                    {
                                        break;
                                    }
                                    matched = true;
                                    nextStateIndex += 1; // Skip PatternMultipleExpressions and matched statement.
                                    lastMatchIndex = i;
                                }
                                else
                                {
                                    nextStateIndex -= 1;
                                }
                            }
                            // Else continue or exit. Match to any statement.
                        }
                        else if (Match(otherExpressions, i, ref nextStateIndex))
                        {
                            if (!matched && i != 0) // if firstMatch not at first position
                            {
                                break;
                            }
                            matched = true;
                            nextStateIndex += 1;
                            lastMatchIndex = i;
                        }

                        if (nextStateIndex == Collection.Count)
                        {
                            if (Collection[nextStateIndex - 1].NodeType == NodeType.PatternMultipleExpressions)
                            {
                                lastMatchIndex = otherExpressions.Count - 1;
                            }
                            break;
                        }

                        i++;
                    }
                }
            }
            else
            {
                lastMatchIndex = -1; // Match if function without arguments.
            }

            if (nextStateIndex < Collection.Count && Collection[nextStateIndex].NodeType == NodeType.PatternMultipleExpressions)
            {
                nextStateIndex++;
            }
    
            // if lastMatchIndex == -1 then function has 0 arguments.
            return nextStateIndex == Collection.Count && lastMatchIndex == otherExpressions.Count - 1; // if all states has been visited.
        }

        private bool Match(IList<Expression> otherExpressions, int otherExpressionIndex, ref int nextStateIndex)
        {
            // Compare with next not PatternMultipleExpressions (start or continue point).
            PatternExpression patternExpression = Collection[nextStateIndex] as PatternExpression;
            bool not = false;
            if (patternExpression != null)
            {
                not = patternExpression.Not;
                patternExpression.Not = false;
            }

            bool result;
            if (otherExpressionIndex >= 0 && otherExpressionIndex < otherExpressions.Count && Collection[nextStateIndex].Equals(otherExpressions[otherExpressionIndex]))
            {
                result = true;
            }
            else
            {
                result = false;
            }

            IEnumerable<PatternVarDef> patternVarDefs = Collection[nextStateIndex]
                .GetAllDescendants(child => child != null && child.NodeType == NodeType.PatternVarRef && ((PatternVarRef)child).PinValueAssigned)
                .Select(child => ((PatternVarRef)child).PatternVar);
            foreach (var patternVarDef in patternVarDefs)
                pinnedPatternVarDefs.Add(patternVarDef);

            if (patternExpression != null)
            {
                patternExpression.Not = not;
            }
            return result;
        }

        private void ResetPinValues()
        {
            foreach (var patternVarDef in pinnedPatternVarDefs)
            {
                patternVarDef.PinValue = null;
            }
        }
    }
}
