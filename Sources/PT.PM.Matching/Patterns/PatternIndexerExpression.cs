using System;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;

namespace PT.PM.Matching.Patterns
{
    public class PatternIndexerExpression : PatternExpression
    {
        public override Type UstType => typeof(IndexerExpression);

        public PatternBase Target { get; set; }

        public PatternArgs Arguments { get; set; }

        public PatternIndexerExpression()
        {
        }

        public PatternIndexerExpression(PatternBase target, PatternArgs arguments, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Target = target;
            Arguments = arguments;
        }

        public override PatternBase[] GetArgs()
        {
            var result = new List<PatternBase>();
            result.Add(Target);
            result.AddRange(Arguments.Args);
            return result.ToArray();
        }

        public override string ToString() => $"{Target}[{Arguments}]";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is IndexerExpression invocationExpression)
            {
                newContext = Target.Match(invocationExpression.Target, context);
                if (!newContext.Success)
                {
                    return newContext;
                }

                newContext = Arguments.Match(invocationExpression.Arguments, newContext);
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext.AddUstIfSuccess(ust);
        }
    }
}
