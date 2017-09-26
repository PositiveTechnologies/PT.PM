using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternAssignmentExpression : PatternBase
    {
        public PatternBase Left { get; set; }

        public PatternBase Right { get; set; }

        public PatternAssignmentExpression()
        {
        }

        public PatternAssignmentExpression(PatternBase left, PatternBase right,
            TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Left = left;
            Right = right;
        }

        public override Ust[] GetChildren() => new Ust[] { Left, Right };

        public override string ToString()
        {
            return Right == null ? Left.ToString() : $"{Left} = {Right}";
        }

        public override bool Match(Ust ust, MatchingContext context)
        {
            if (ust?.Kind != UstKind.AssignmentExpression)
            {
                return false;
            }

            var assignmentExpression = (AssignmentExpression)ust;
            return Left.Match(assignmentExpression.Left, context) &&
                   Right.Match(assignmentExpression.Right, context);
        }
    }
}
