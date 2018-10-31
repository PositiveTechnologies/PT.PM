using System;
using PT.PM.Common;
using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public class PatternIndexerExpression : PatternUst, IPatternExpression
    {
        public Type UstType => typeof(IndexerExpression);
        
        public PatternUst Target { get; set; }

        public PatternArgs Arguments { get; set; }

        public PatternIndexerExpression()
        {
        }

        public PatternIndexerExpression(PatternUst target, PatternArgs arguments, TextSpan textSpan = default)
            : base(textSpan)
        {
            Target = target;
            Arguments = arguments;
        }

        public PatternUst[] GetArgs()
        {
            var result = new List<PatternUst>();
            result.Add(Target);
            result.AddRange(Arguments.Args);
            return result.ToArray();
        }

        public override string ToString() => $"{Target}[{Arguments}]";

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            var indexerExpression = ust as IndexerExpression;
            if (indexerExpression == null)
            {
                return context.Fail();
            }
            
            MatchContext newContext = Target.MatchUst(indexerExpression.Target, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = Arguments.MatchUst(indexerExpression.Arguments, newContext);

            return newContext.AddUstIfSuccess(indexerExpression);
        }
    }
}
