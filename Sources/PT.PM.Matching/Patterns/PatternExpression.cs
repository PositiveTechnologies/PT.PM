using PT.PM.Common;
using System;

namespace PT.PM.Matching.Patterns
{
    public abstract class PatternExpression : PatternBase
    {
        public abstract Type UstType { get; }

        public abstract PatternBase[] GetArgs();

        public PatternExpression(TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
        }
    }
}
