using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Statements;
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

        public PatternMethodDeclaration(IEnumerable<PatternBase> modifiers, PatternBase name,
            PatternBase body, TextSpan textSpan)
            : base(textSpan)
        {
            Modifiers = modifiers?.ToList()
                ?? new List<PatternBase>();
            Name = name;
            AnyBody = false;
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

        public PatternMethodDeclaration()
        {
            Modifiers = new List<PatternBase>();
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.AddRange(Modifiers);
            result.Add(Name);
            result.Add(Body);
            return result.ToArray();
        }

        public override string ToString()
        {
            var result = $"{string.Join(", ", Modifiers)} {Name}() ";
            result += " { " + (Body?.ToString() ?? "") + " }";

            return result;
        }

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            if (ust?.Kind != UstKind.MethodDeclaration)
            {
                return context.Fail();
            }

            var methodDeclaration = (MethodDeclaration)ust;

            MatchingContext match = Modifiers.MatchSubset(methodDeclaration.Modifiers, context);
            if (!match.Success)
            {
                return match;
            }

            match = Name.Match(methodDeclaration.Name, match);
            if (!match.Success)
            {
                return match;
            }

            if (!AnyBody)
            {
                if (Body != null)
                {
                    match = Body.Match(methodDeclaration.Body, context);
                    if (!match.Success)
                    {
                        return match;
                    }
                }
                else if (methodDeclaration.Body != null)
                {
                    return match.Fail();
                }
            }

            return match;
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
