using PT.PM.Common;
using System;

namespace PT.PM.Matching.Patterns
{
    public abstract class PatternExpression : PatternUst
    {
        public abstract Type UstType { get; }

        public abstract PatternUst[] GetArgs();

        public PatternExpression(TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
        }
    }
}
