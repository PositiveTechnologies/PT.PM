using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternObjectCreateExpression : PatternUst
    {
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

        public override MatchContext Match(Ust ust, MatchContext context)
        {
            var objectCreateExpression = ust as ObjectCreateExpression;
            if (objectCreateExpression == null)
            {
                return context.Fail();
            }
            
            MatchContext newContext = Type.Match(objectCreateExpression.Type, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = Arguments.Match(objectCreateExpression.Arguments, newContext);

            return newContext.AddUstIfSuccess(objectCreateExpression);
        }
    }
}
