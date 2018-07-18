using PT.PM.Common;
using PT.PM.Common.Nodes.GeneralScope;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternClassDeclaration : PatternUst<TypeDeclaration>
    {
        public List<PatternUst> Modifiers { get; set; }

        public PatternUst Name { get; set; }

        public List<PatternUst> BaseTypes { get; set; }

        public PatternArbitraryDepth Body { get; set; }

        public PatternClassDeclaration()
        {
            Modifiers = new List<PatternUst>();
            BaseTypes = new List<PatternUst>();
        }

        public PatternClassDeclaration(IEnumerable<PatternUst> modifiers,
            PatternUst name, IEnumerable<PatternUst> baseTypes,
            PatternArbitraryDepth body, TextSpan textSpan = default)
            : base(textSpan)
        {
            Modifiers = modifiers?.ToList() ?? new List<PatternUst>();
            Name = name;
            BaseTypes = baseTypes?.ToList() ?? new List<PatternUst>();
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

        public override MatchContext Match(TypeDeclaration typeDeclaration, MatchContext context)
        {
            MatchContext newContext = Modifiers.MatchSubset(typeDeclaration.Modifiers, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            if (Name != null)
            {
                newContext = Name.MatchUst(typeDeclaration.Name, newContext);
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

            return newContext.AddUstIfSuccess(typeDeclaration);
        }
    }
}
