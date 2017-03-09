using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Patterns;
using PT.PM.Patterns.Nodes;
using PT.PM.Common;

namespace PT.PM.Matching
{
    public class PatternVarRefEnumerator : IEnumerator<UstNode>
    {
        private readonly UstNode current;
        private readonly int[] varLiteralInds;
        private readonly PatternVarDef[] patternVars;
        private readonly IList<PatternVarRef> varRefs;

        private int flag;

        public Pattern Pattern { get; private set; }
        public int CurrentSetIndex { get; private set; }

        public PatternVarRefEnumerator(Pattern pattern)
        {
            Pattern = pattern;
            current = pattern.Data.Node.Clone();

            UstNode[] children = current.GetAllDescendants(ustNode => ustNode != null && ustNode.NodeType == NodeType.PatternVarRef);
            varRefs = new List<PatternVarRef>();
            varLiteralInds = new int[pattern.Data.Vars?.Count ?? 0];

            patternVars = pattern.Data.Vars?.Select(var => (PatternVarDef)var.Clone()).ToArray() ?? ArrayUtils<PatternVarDef>.EmptyArray;

            foreach (UstNode child in children)
            {
                var varRef = (PatternVarRef)child;
                for (int i = 0; i < patternVars.Length; i++)
                {
                    if (patternVars[i].Id == varRef.VarId)
                    {
                        varRef.PatternVar = patternVars[i];
                        varRef.PatternVarIndex = i;
                        varRefs.Add(varRef);
                        break;
                    }
                }
            }
            ClearFlag();
        }

        public UstNode Current => current;

        object IEnumerator.Current => current;

        public bool MoveNext()
        {
            if (flag == 1)
            {
                ClearFlag();
                return false;
            }

            foreach (PatternVarRef varRef in varRefs)
            {
                varRef.CurrentValue = varRef.PatternVar.Values[varLiteralInds[varRef.PatternVarIndex]];
            }

            flag = 1;
            for (int i = 0; i < patternVars.Length; i++)
            {
                varLiteralInds[i] += flag;
                if (varLiteralInds[i] == patternVars[i].Values.Count)
                {
                    varLiteralInds[i] = 0;
                }
                else
                {
                    flag = 0;
                    break;
                }
            }
            CurrentSetIndex++;

            return true;
        }

        public void Reset()
        {
            for (int i = 0; i < varLiteralInds.Length; i++)
            {
                varLiteralInds[i] = 0;
            }
            foreach (var patternVar in patternVars)
            {
                patternVar.PinValue = null;
            }
            ClearFlag();
        }

        public void Dispose()
        {
            Reset();
        }

        public override string ToString()
        {
            return Current.ToString();
        }

        private void ClearFlag()
        {
            CurrentSetIndex = -1;
            flag = 0;
        }
    }
}
