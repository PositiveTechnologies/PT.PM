using System;
using PT.PM.Common;
using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public class PatternInvocationExpression : PatternUst, IPatternExpression
    {
        public Type UstType => typeof(InvocationExpression);
        
        public PatternUst Target { get; set; }

        public PatternUst Arguments { get; set; }

        public PatternInvocationExpression()
        {
        }

        public PatternInvocationExpression(PatternUst target, PatternArgs arguments,
            TextSpan textSpan = default)
            : base(textSpan)
        {
            Target = target;
            Arguments = arguments;
        }

        public PatternUst[] GetArgs()
        {
            var result = new List<PatternUst>();
            result.Add(Target);
            if (Arguments is PatternArgs patternArgs)
            {
                result.AddRange(patternArgs.Args);
            }
            else
            {
                result.Add(Arguments);
            }

            return result.ToArray();
        }

        public override string ToString() => $"{Target}({Arguments})";

        public override MatchContext Match(Ust ust, MatchContext context)
        {
            var invocation = ust as InvocationExpression;
            if (invocation == null)
            {
                return context.Fail();
            }
            
            MatchContext newContext = Target.Match(invocation.Target, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            if (Arguments is PatternArgs patternArgs)
            {
                newContext = patternArgs.Match(invocation.Arguments, newContext);
            }
            else
            {
                newContext = Arguments.Match(invocation.Arguments, newContext);
            }

            return newContext.AddUstIfSuccess(invocation);
        }
    }
}
