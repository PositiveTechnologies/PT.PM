using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Statements;

namespace PT.PM.Matching.Patterns
{
    public class PatternMultipleStatements : Statement
    {
        public override UstKind Kind => UstKind.PatternMultipleStatements;

        public PatternMultipleStatements()
        {
        }

        public override Ust[] GetChildren()
        {
            return ArrayUtils<Ust>.EmptyArray;
        }

        public override int CompareTo(Ust other)
        {
            if (other == null)
            {
                return (int)other.Kind;
            }

            if (other is Statement)
            {
                return 0;
            }
            else
            {
                return Kind - other.Kind;
            }
        }

        public override string ToString() => "...;";
    }
}
