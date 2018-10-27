using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Matching.Patterns
{
    public class PatternVar : PatternUst
    {
        public string Id { get; set; } = "";

        public PatternVar()
            : this("")
        {
        }

        public PatternVar(string id, TextSpan textSpan = default)
            : base(textSpan)
        {
            Id = id;
        }

        public PatternUst Value { get; set; } = new PatternIdRegexToken();

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

        public override MatchContext Match(Ust ust, MatchContext context)
        {
            var idToken = ust as IdToken;
            if (idToken == null)
            {
                return context.Fail();
            }
            
            MatchContext newContext = context;
            if (idToken.Parent is AssignmentExpression parentAssignment &&
                ReferenceEquals(idToken, parentAssignment.Left))
            {
                if (Value != null)
                {
                    newContext = Value.Match(idToken, newContext);
                    if (newContext.Success)
                    {
                        newContext.Vars[Id] = idToken;
                    }
                }
                else
                {
                    newContext.Vars[Id] = idToken;
                    newContext = newContext.AddMatch(idToken);
                }
            }
            else
            {
                newContext = newContext.Vars.TryGetValue(Id, out IdToken value) && value.Equals(idToken)
                    ? newContext.AddMatch(idToken)
                    : newContext.Fail();
            }

            return newContext;
        }
    }
}
