using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Statements;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternStatements : BlockStatement, IAbsoluteLocationMatching
    {
        private HashSet<PatternVarDef> pinnedPatternVarDefs;

        public override UstKind Kind => UstKind.PatternStatements;

        public TextSpan MatchedLocation { get; set; }

        public PatternStatements()
        {
            pinnedPatternVarDefs = new HashSet<PatternVarDef>();
        }

        public PatternStatements(params Statement[] statements)
            : base(statements, default(TextSpan))
        {
        }

        public override int CompareTo(Ust other)
        {
            if (other == null)
            {
                return 1;
            }

            if (other.Kind == UstKind.PatternStatements)
            {
                return base.CompareTo(other);
            }
            
            int result = UstKind.BlockStatement - other.Kind;

            if (result != 0)
            {
                return result;
            }
            if (Statements == null)
            {
                return 0;
            }

            IList<Statement> otherStatements = ((BlockStatement)other).Statements;

            int i = 0;
            int startIndex;
            int endIndex = otherStatements.Count - 1;
            bool includeNegative = false;

            // TODO: get rid of double loop.
            do
            {
                startIndex = i;
                endIndex = otherStatements.Count - 1;
                bool withoutNegativeResult = Match(otherStatements, true, ref startIndex, ref endIndex, ref includeNegative);
                if (withoutNegativeResult)
                {
                    if (includeNegative)
                    {
                        // check with negative statements
                        if (Match(otherStatements, false, ref startIndex, ref endIndex, ref includeNegative))
                        {
                            result = 1;
                            endIndex = otherStatements.Count - 1;
                            ResetPinValues();
                        }
                        else
                        {
                            result = 0; // pattern found.
                            MatchedLocation = otherStatements[startIndex].TextSpan; //TODO: otherStatements[startIndex].TextSpan.UnionWith(otherStatements[endIndex].TextSpan);
                            break;
                        }
                    }
                    else
                    {
                        result = 0; // pattern found.
                        MatchedLocation = otherStatements[startIndex].TextSpan; //TODO: otherStatements[startIndex].TextSpan.UnionWith(otherStatements[endIndex].TextSpan);
                        break;
                    }
                }
                else
                {
                    result = 1;
                    endIndex = otherStatements.Count - 1;
                    ResetPinValues();
                }

                i++;
            }
            while (i < otherStatements.Count);

            return result;
        }

        private bool Match(IList<Statement> otherStatements, bool ignoreNegative, ref int startIndex, ref int endIndex, ref bool includeNegative)
        {
            int nextStateIndex = -1;
            int fitstMatchIndex = -1;
            int lastMatchIndex = -1;

            int i = startIndex;

            while (i <= endIndex)
            {
                PatternStatement patternStatement;
                if (nextStateIndex != -1 && ignoreNegative)
                {
                    if (nextStateIndex < Statements.Count && (patternStatement = Statements[nextStateIndex] as PatternStatement) != null && patternStatement.Not) // ignore negative patterns on first step
                    {
                        includeNegative = true;
                        nextStateIndex += 1;
                        continue;
                    }
                }

                if (nextStateIndex == -1 || (nextStateIndex < Statements.Count && Statements[nextStateIndex].Kind == UstKind.PatternMultipleStatements))
                {
                    nextStateIndex += 1;
                    if (nextStateIndex < Statements.Count)
                    {
                        if (ignoreNegative && (patternStatement = Statements[nextStateIndex] as PatternStatement) != null && patternStatement.Not)
                        {
                            includeNegative = true;
                            nextStateIndex += 1;
                            continue;
                        }

                        // Compare with next not PatternMultiStatements (start or continue point).
                        if (Match(otherStatements, i, ignoreNegative, ref nextStateIndex))
                        {
                            nextStateIndex += 1; // Skip PatternMultiStatements and matched statement.
                            if (fitstMatchIndex == -1)
                            {
                                fitstMatchIndex = i;
                            }
                            lastMatchIndex = i;
                        }
                        else
                        {
                            nextStateIndex -= 1;
                        }
                    }
                    // Else continue or exit. Match to any statement.
                }
                else if (Match(otherStatements, i, ignoreNegative, ref nextStateIndex))
                {
                    nextStateIndex += 1;
                    if (fitstMatchIndex == -1)
                    {
                        fitstMatchIndex = i;
                    }
                    lastMatchIndex = i;
                }
                else
                {
                    break; // not matched
                }

                if (nextStateIndex == Statements.Count)
                {
                    break;
                }

                i++;
            }

            if (ignoreNegative)
            {
                startIndex = fitstMatchIndex == -1 ? otherStatements.Count - 1 : fitstMatchIndex;
                endIndex = nextStateIndex == Statements.Count ? otherStatements.Count - 1 : lastMatchIndex;
            }

            return nextStateIndex == Statements.Count; // if all states has been visited.
        }

        private bool Match(IList<Statement> otherStatements, int otherStatementIndex, bool ignoreNegative, ref int nextStateIndex)
        {
            if (nextStateIndex >= Statements.Count)
            {
                return false;
            }

            // Compare with next not PatternMultiStatements (start or continue point).
            PatternStatement patternStatement = Statements[nextStateIndex] as PatternStatement;
            bool not = false;
            if (patternStatement != null)
            {
                not = patternStatement.Not;
                patternStatement.Not = false;
            }

            bool result;
            if (Statements[nextStateIndex].Equals(otherStatements[otherStatementIndex]))
            {
                if (!ignoreNegative && patternStatement != null && not)
                {
                    nextStateIndex = Statements.Count - 1; // matched negative node
                }
                result = true;
            }
            else
            {
                result = false;
            }

            IEnumerable<PatternVarDef> patternVarDefs = Statements[nextStateIndex]
                .GetAllDescendants(child => child != null && child.Kind == UstKind.PatternVarRef && ((PatternVarRef)child).PinValueAssigned)
                .Select(child => ((PatternVarRef)child).PatternVar);
            foreach (var patternVarDef in patternVarDefs)
                pinnedPatternVarDefs.Add(patternVarDef);

            if (patternStatement != null)
            {
                patternStatement.Not = not;
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
