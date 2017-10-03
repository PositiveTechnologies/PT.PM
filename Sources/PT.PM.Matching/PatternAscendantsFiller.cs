using PT.PM.Matching.Patterns;
using System.Collections.Generic;

namespace PT.PM.Matching
{
    public class PatternAscendantsFiller : PatternVisitor<PatternBase>
    {
        private PatternRoot patternRoot;
        private Stack<PatternBase> parents;

        public PatternAscendantsFiller(PatternRoot patternRoot)
        {
            this.patternRoot = patternRoot;
            parents = new Stack<PatternBase>();
        }

        public void FillAscendants()
        {
            Visit(patternRoot.Node);
        }

        public override PatternBase Visit(PatternBase patternBase)
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
