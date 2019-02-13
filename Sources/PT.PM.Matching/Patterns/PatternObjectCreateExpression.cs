using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternObjectCreateExpression : PatternUst, IPatternExpression
    {
        [JsonIgnore]
        public Type UstType => typeof(ObjectCreateExpression);

        public PatternUst Type { get; set; }

        public PatternArgs Arguments { get; set; }

        public PatternObjectCreateExpression()
        {
        }

        public PatternObjectCreateExpression(PatternUst type, PatternArgs args, TextSpan textSpan = default)
            : base(textSpan)
        {
            Type = type;
            Arguments = args;
        }

        public override string ToString() => $"new {Type}({Arguments})";

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            var objectCreateExpression = ust as ObjectCreateExpression;
            if (objectCreateExpression == null)
            {
                return context.Fail();
            }

            MatchContext newContext = Type.MatchUst(objectCreateExpression.Type, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = Arguments.MatchUst(objectCreateExpression.Arguments, newContext);

            return newContext.AddUstIfSuccess(objectCreateExpression);
        }

        public PatternUst[] GetArgs()
        {
            var result = new List<PatternUst>();
            result.Add(Type);
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
    }
}
