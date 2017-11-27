using PT.PM.Common;
using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;

namespace PT.PM.Matching.Patterns
{
    public class PatternIndexerExpression : PatternUst<IndexerExpression>, IPatternExpression
    {
        public PatternUst Target { get; set; }

        public PatternArgs Arguments { get; set; }

        public PatternIndexerExpression()
        {
        }

        public PatternIndexerExpression(PatternUst target, PatternArgs arguments, TextSpan textSpan = default(TextSpan))
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

        public override MatchingContext Match(IndexerExpression indexerExpression, MatchingContext context)
        {
            MatchingContext newContext;

            newContext = Target.MatchUst(indexerExpression.Target, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = Arguments.Match(indexerExpression.Arguments, newContext);

            return newContext.AddUstIfSuccess(indexerExpression);
        }
    }
}
