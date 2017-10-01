using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.GeneralScope;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternClassDeclaration : PatternBase
    {
        public List<PatternBase> Modifiers { get; set; }

        public PatternBase Name { get; set; }

        public List<PatternBase> BaseTypes { get; set; }

        public PatternArbitraryDepth Body { get; set; }

        public PatternClassDeclaration()
        {
            Modifiers = new List<PatternBase>();
            BaseTypes = new List<PatternBase>();
        }

        public PatternClassDeclaration(IEnumerable<PatternBase> modifiers,
            PatternBase name, IEnumerable<PatternBase> baseTypes,
            PatternArbitraryDepth body, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Modifiers = modifiers?.ToList() ?? new List<PatternBase>();
            Name = name;
            BaseTypes = baseTypes?.ToList() ?? new List<PatternBase>();
            Body = body;
        }

        public override string ToString()
        {
            var result = $"{string.Join(", ", Modifiers)} class {Name} ";
            if (BaseTypes.Any())
            {
                result += $": {string.Join(", ", BaseTypes.Select(t => t.ToString()))}";
            }
            result += " { " + (Body?.ToString() ?? "") + " }";

            return result;
        }

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is TypeDeclaration typeDeclaration)
            {
                newContext = Modifiers.MatchSubset(typeDeclaration.Modifiers, context);
                if (!newContext.Success)
                {
                    return newContext;
                }

                if (Name != null)
                {
                    newContext = Name.Match(typeDeclaration.Name, newContext);
                    if (!newContext.Success)
                    {
                        return newContext;
                    }
                }

                newContext = BaseTypes.MatchSubset(typeDeclaration.BaseTypes, newContext);

                if (!newContext.Success)
                {
                    return newContext;
                }

                if (Body != null)
                {
                    if (!typeDeclaration.TypeMembers.Any(m => Body.Match(m, newContext).Success))
                    {
                        return newContext.Fail();
                    }
                }
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext.AddMatchIfSuccess(ust);
        }
    }
}
