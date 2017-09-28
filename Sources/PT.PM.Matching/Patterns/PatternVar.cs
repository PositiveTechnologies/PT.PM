using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Matching.Patterns
{
    public class PatternVar : PatternBase
    {
        public string Id { get; set; }

        public PatternVar()
        {
        }

        public PatternVar(string id, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Id = id;
        }

        public PatternBase Value { get; set; } = new PatternIdRegexToken();

        public override Ust[] GetChildren() => new Ust[] { Value };

        public override string ToString() => $"{Id}: {Value}";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is IdToken idToken)
            {
                newContext = context;
                if (ust.Parent is AssignmentExpression)
                {
                    if (Value != null)
                    {
                        newContext = Value.Match(ust, newContext);
                        if (newContext.Success)
                        {
                            newContext.Vars[Id] = idToken;
                        }
                    }
                    else
                    {
                        newContext.Vars[Id] = idToken;
                        newContext = newContext.AddMatch(ust);
                    }
                }
                else
                {
                    if (newContext.Vars.ContainsKey(Id))
                    {
                        newContext = newContext.AddMatch(ust);
                    }
                    else
                    {
                        newContext = newContext.Fail();
                    }
                }
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext;
        }
    }
}
