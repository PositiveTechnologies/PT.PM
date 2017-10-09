using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternAssignmentExpression : PatternUst
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

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is AssignmentExpression assign)
            {
                newContext = Left.Match(assign.Left, context);
                if (newContext.Success)
                {
                    if (Right != null && assign.Right != null)
                    {
                        newContext = Right.Match(assign.Right, newContext);
                    }
                    else if ((Right != null && assign.Right == null) ||
                             (Right == null && assign.Right != null))
                    {
                        newContext = newContext.Fail();
                    }
                }
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext.AddUstIfSuccess(ust);
        }
    }
}
