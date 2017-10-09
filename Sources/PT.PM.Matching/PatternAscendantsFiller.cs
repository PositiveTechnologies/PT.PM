using PT.PM.Matching.Patterns;
using System.Collections.Generic;

namespace PT.PM.Matching
{
    public class PatternAscendantsFiller : PatternVisitor<PatternUst>
    {
        private PatternRoot patternRoot;
        private Stack<PatternUst> parents;

        public PatternAscendantsFiller(PatternRoot patternRoot)
        {
            this.patternRoot = patternRoot;
            parents = new Stack<PatternUst>();
        }

        public void FillAscendants()
        {
            Visit(patternRoot.Node);
        }

        public override PatternUst Visit(PatternUst patternBase)
        {
            if (patternBase == null)
            {
                return null;
            }

            patternBase.Parent = parents.Count > 0 ? parents.Peek() : null;
            patternBase.Root = patternRoot;
            parents.Push(patternBase);

            VisitChildren(patternBase);

            parents.Pop();
            return patternBase;
        }
    }
}
