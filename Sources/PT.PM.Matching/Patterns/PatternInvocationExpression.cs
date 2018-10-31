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

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            var invocation = ust as InvocationExpression;
            if (invocation == null)
            {
                return context.Fail();
            }

            context = Target.MatchUst(invocation.Target, context);
            if (!context.Success)
            {
                return context;
            }

            context = Arguments.MatchUst(invocation.Arguments, context);

            return context.AddUstIfSuccess(invocation);
        }
    }
}
