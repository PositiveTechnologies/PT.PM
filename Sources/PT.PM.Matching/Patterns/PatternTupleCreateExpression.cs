using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using System;
using Newtonsoft.Json;

namespace PT.PM.Matching.Patterns
{
    public class PatternTupleCreateExpression : PatternUst, IPatternExpression
    {
        private PatternUst[] args = new PatternUst[0];

        [JsonIgnore]
        public Type UstType => typeof(TupleCreateExpression);

        public PatternUst[] GetArgs() => args;

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            if (ust is TupleCreateExpression)
            {
                return context.AddMatch(ust);
            }
            else
            {
                return context.Fail();
            }
        }
    }
}
