using PT.PM.Common;
using PT.PM.Common.Nodes.GeneralScope;
using System.Collections.Generic;
using System.Linq;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public class PatternClassDeclaration : PatternUst
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

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            var typeDeclaration = ust as TypeDeclaration;
            if (typeDeclaration == null)
            {
                return context.Fail();
            }
            
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
                if (!typeDeclaration.TypeMembers.Any(m => Body.MatchUst(m, newContext).Success))
                {
                    return newContext.Fail();
                }
            }

            return newContext.AddUstIfSuccess(typeDeclaration);
        }
    }
}
