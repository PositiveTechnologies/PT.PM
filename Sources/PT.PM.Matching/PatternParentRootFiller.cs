using PT.PM.Matching.Patterns;
using System.Collections.Generic;

namespace PT.PM.Matching
{
    public class PatternParentRootFiller : PatternVisitor<PatternUst>
    {
        private PatternRoot patternRoot;
        private Stack<PatternUst> parents;

        public PatternParentRootFiller(PatternRoot patternRoot)
        {
            this.patternRoot = patternRoot;
            parents = new Stack<PatternUst>();
        }

        public void FillParentAndRootForDescendantsAndSelf()
        {
            Visit(patternRoot.Node);
        }

        public override PatternUst Visit(PatternUst patternUst)
        {
            if (patternUst == null)
            {
                return null;
            }

            patternUst.Parent = parents.Count > 0 ? parents.Peek() : null;
            patternUst.Root = patternRoot;
            parents.Push(patternUst);

            VisitChildren(patternUst);

            parents.Pop();
            return patternUst;
        }
    }
}
