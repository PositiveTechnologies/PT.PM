using PT.PM.Common;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternAssignmentExpression : PatternUst<AssignmentExpression>
    {
        public PatternUst Left { get; set; }

        public PatternUst Right { get; set; }

        public PatternAssignmentExpression()
        {
        }

        public PatternAssignmentExpression(PatternUst left, PatternUst right,
            TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return Right == null ? Left.ToString() : $"{Left} = {Right}";
        }

        public override MatchContext Match(AssignmentExpression assign, MatchContext context)
        {
            MatchContext newContext = Left.MatchUst(assign.Left, context);
            if (newContext.Success)
            {
                if (Right != null && assign.Right != null)
                {
                    newContext = Right.MatchUst(assign.Right, newContext);
                }
                else if ((Right != null && assign.Right == null) ||
                         (Right == null && assign.Right != null))
                {
                    newContext = newContext.Fail();
                }
            }

            return newContext.AddUstIfSuccess(assign);
        }
    }
}
