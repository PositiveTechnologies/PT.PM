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

        public PatternExpressionInsideNode Body { get; set; }

        public PatternClassDeclaration()
        {
            Modifiers = new List<PatternBase>();
            BaseTypes = new List<PatternBase>();
        }

        public PatternClassDeclaration(IEnumerable<PatternBase> modifiers,
            PatternBase name, IEnumerable<PatternBase> baseTypes,
            PatternExpressionInsideNode body, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Modifiers = modifiers?.ToList() ?? new List<PatternBase>();
            Name = name;
            BaseTypes = baseTypes?.ToList() ?? new List<PatternBase>();
            Body = body;
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.AddRange(Modifiers);
            result.Add(Name);
            result.AddRange(BaseTypes);
            result.Add(Body);
            return result.ToArray();
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

        public override bool Match(Ust ust, MatchingContext context)
        {
            if (ust?.Kind != UstKind.TypeDeclaration)
            {
                return false;
            }

            var typeDeclaration = (TypeDeclaration)ust;
            bool match = Modifiers.MatchSubset(typeDeclaration.Modifiers, context);
            if (!match)
            {
                return match;
            }

            if (Name != null)
            {
                match = Name.Match(typeDeclaration.Name, context);
                if (!match)
                {
                    return match;
                }
            }

            match = BaseTypes.MatchSubset(typeDeclaration.BaseTypes, context);

            var baseTypesToMatch = new List<Ust>(BaseTypes);
            if (!match)
            {
                return match;
            }

            if (Body != null)
            {
                if (!typeDeclaration.TypeMembers.Any(m => Body.Match(m, context)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
