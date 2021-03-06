﻿using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternMemberReferenceExpression : PatternUst
    {
        public PatternUst Target { get; set; }

        public PatternUst Name { get; set; }

        public PatternMemberReferenceExpression()
        {
        }

        public PatternMemberReferenceExpression(PatternUst target, PatternUst name,
            TextSpan textSpan = default)
            : base(textSpan)
        {
            Target = target;
            Name = name;
        }

        public override string ToString() => $"{Target}.{Name}";

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            var memberRef = ust as MemberReferenceExpression;
            if (memberRef == null)
            {
                return context.Fail();
            }
            
            MatchContext newContext = Target.MatchUst(memberRef.Target, context);
            if (!newContext.Success)
            {
                // Ugly hack to handle optional `this`
                if (Target is PatternMemberReferenceExpression patternMemberReference && patternMemberReference.Target is PatternThisReferenceToken)
                {
                    newContext = patternMemberReference.Name.MatchUst(memberRef.Target, context);
                }

                if (!newContext.Success)
                    return newContext;
            }

            newContext = Name.MatchUst(memberRef.Name, newContext);

            return newContext.AddUstIfSuccess(memberRef);
        }
    }
}
