using PT.PM.Common;
using PT.PM.Common.Nodes.Expressions;
using System;

namespace PT.PM.Matching.Patterns
{
    public abstract class PatternExpression<TExpression> : PatternUst<TExpression>
        where TExpression : Expression
    {
        public abstract Type UstType { get; }

        public abstract PatternUst[] GetArgs();

        public PatternExpression(TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
        }
    }
}
