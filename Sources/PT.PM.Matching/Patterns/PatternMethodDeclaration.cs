using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.TypeMembers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternMethodDeclaration : PatternBase
    {
        public bool AnyBody { get; set; }

        public List<PatternBase> Modifiers { get; set; }

        public PatternBase Name { get; set; }

        public PatternBase Body { get; set; }

        public PatternMethodDeclaration()
        {
            Modifiers = new List<PatternBase>();
        }

        public PatternMethodDeclaration(IEnumerable<PatternBase> modifiers, PatternBase name,
            PatternBase body, TextSpan textSpan)
            : base(textSpan)
        {
            Modifiers = modifiers?.ToList()
                ?? new List<PatternBase>();
            Name = name;
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public PatternMethodDeclaration(IEnumerable<PatternBase> modifiers, PatternBase name, bool anyBody,
            TextSpan textSpan)
            : base(textSpan)
        {
            InitFields(modifiers, name, anyBody);
        }

        public PatternMethodDeclaration(IEnumerable<PatternBase> modifiers, PatternBase name, bool anyBody)
        {
            InitFields(modifiers, name, anyBody);
        }

        public override string ToString()
        {
            var result = $"{string.Join(", ", Modifiers)} {Name}() ";
            result += " { " + (Body?.ToString() ?? "") + " }";

            return result;
        }

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is MethodDeclaration methodDeclaration)
            {
                newContext = Modifiers.MatchSubset(methodDeclaration.Modifiers, context);
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
                        newContext = Body.Match(methodDeclaration.Body, newContext);
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
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext.AddUstIfSuccess(ust);
        }

        private void InitFields(IEnumerable<PatternBase> modifiers, PatternBase name, bool anyBody)
        {
            Modifiers = modifiers?.ToList() ?? new List<PatternBase>();
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
