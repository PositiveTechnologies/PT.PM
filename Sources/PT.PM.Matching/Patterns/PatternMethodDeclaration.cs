using PT.PM.Common;
using PT.PM.Common.Nodes.TypeMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public class PatternMethodDeclaration : PatternUst
    {
        public bool AnyBody { get; set; }

        public List<PatternUst> Modifiers { get; set; }

        public PatternUst Name { get; set; }

        public PatternUst Body { get; set; }

        public PatternMethodDeclaration()
        {
            Modifiers = new List<PatternUst>();
        }

        public PatternMethodDeclaration(IEnumerable<PatternUst> modifiers, PatternUst name,
            PatternUst body, TextSpan textSpan)
            : base(textSpan)
        {
            Modifiers = modifiers?.ToList()
                ?? new List<PatternUst>();
            Name = name;
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public PatternMethodDeclaration(IEnumerable<PatternUst> modifiers, PatternUst name, bool anyBody,
            TextSpan textSpan)
            : base(textSpan)
        {
            InitFields(modifiers, name, anyBody);
        }

        public PatternMethodDeclaration(IEnumerable<PatternUst> modifiers, PatternUst name, bool anyBody)
        {
            InitFields(modifiers, name, anyBody);
        }

        public override string ToString()
        {
            var result = $"{string.Join(", ", Modifiers)} {Name}() ";
            result += " { " + (Body?.ToString() ?? "") + " }";

            return result;
        }

        public override MatchContext Match(Ust ust, MatchContext context)
        {
            var methodDeclaration = ust as MethodDeclaration;
            if (methodDeclaration == null)
            {
                return context.Fail();
            } 
            
            MatchContext newContext = Modifiers.MatchSubset(methodDeclaration.Modifiers, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = Name.Match(methodDeclaration.Name, newContext);
            if (!newContext.Success)
            {
                return newContext;
            }

            if (!AnyBody)
            {
                if (Body != null)
                {
                    if (Body is PatternArbitraryDepth || Body is PatternStatements)
                    {
                        newContext = Body.Match(methodDeclaration.Body, newContext);
                    }
                    else
                    {
                        var otherStatements = methodDeclaration.Body.Statements;
                        if (otherStatements.Count == 1)
                        {
                            newContext = Body.Match(otherStatements.First(), newContext);
                        }
                        else
                        {
                            return newContext.Fail();
                        }
                    }

                    if (!newContext.Success)
                    {
                        return newContext;
                    }
                }
                else if (methodDeclaration.Body != null)
                {
                    return newContext.Fail();
                }
            }

            return newContext.AddUstIfSuccess(methodDeclaration);
        }

        private void InitFields(IEnumerable<PatternUst> modifiers, PatternUst name, bool anyBody)
        {
            Modifiers = modifiers?.ToList() ?? new List<PatternUst>();
            Name = name;
            AnyBody = anyBody;
            if (anyBody)
            {
                Body = null;
            }
            else
            {
                Body = new PatternStatements();
            }
        }
    }
}
