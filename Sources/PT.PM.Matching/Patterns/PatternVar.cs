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

        public override string ToString()
        {
            string valueString = "";
            if (Parent is PatternAssignmentExpression parentAssignment &&
                ReferenceEquals(this, parentAssignment.Left))
            {
                if (!(Value is PatternIdRegexToken patternIdRegexToken && patternIdRegexToken.Any))
                {
                    valueString = ": " + Value.ToString();
                }
            }

            return Id + valueString;
        }

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is IdToken idToken)
            {
                newContext = context;
                if (ust.Parent is AssignmentExpression parentAssignment &&
                    ReferenceEquals(ust, parentAssignment.Left))
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
