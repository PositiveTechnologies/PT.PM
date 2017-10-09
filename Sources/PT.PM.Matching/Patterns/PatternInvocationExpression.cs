using System;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;

namespace PT.PM.Matching.Patterns
{
    public class PatternInvocationExpression : PatternExpression
    {
        public override Type UstType => typeof(InvocationExpression);

        public PatternUst Target { get; set; }

        public PatternArgs Arguments { get; set; }

        public PatternInvocationExpression()
        {
        }

        public PatternInvocationExpression(PatternUst target, PatternArgs arguments,
            TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Target = target;
            Arguments = arguments;
        }

        public override PatternUst[] GetArgs()
        {
            var result = new List<PatternUst>();
            result.Add(Target);
            result.AddRange(Arguments.Args);
            return result.ToArray();
        }

        public override string ToString() => $"{Target}({Arguments})";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is InvocationExpression invocation)
            {
                newContext = Target.Match(invocation.Target, context);
                if (!newContext.Success)
                {
                    return newContext;
                }
                newContext = Arguments.Match(invocation.Arguments, newContext);
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext.AddUstIfSuccess(ust);
        }
    }
}
