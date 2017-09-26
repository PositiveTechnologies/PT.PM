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

        public PatternExpressionInside Body { get; set; }

        public PatternClassDeclaration()
        {
            Modifiers = new List<PatternBase>();
            BaseTypes = new List<PatternBase>();
        }

        public PatternClassDeclaration(IEnumerable<PatternBase> modifiers,
            PatternBase name, IEnumerable<PatternBase> baseTypes,
            PatternExpressionInside body, TextSpan textSpan = default(TextSpan))
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

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext match;

            if (ust is TypeDeclaration typeDeclaration)
            {
                match = Modifiers.MatchSubset(typeDeclaration.Modifiers, context);
                if (!match.Success)
                {
                    return match;
                }

                if (Name != null)
                {
                    match = Name.Match(typeDeclaration.Name, match);
                    if (!match.Success)
                    {
                        return match;
                    }
                }

                match = BaseTypes.MatchSubset(typeDeclaration.BaseTypes, match);

                var baseTypesToMatch = new List<Ust>(BaseTypes);
                if (!match.Success)
                {
                    return match;
                }

                if (Body != null)
                {
                    if (!typeDeclaration.TypeMembers.Any(m => Body.Match(m, match).Success))
                    {
                        return match.Fail();
                    }
                }
            }
            else
            {
                match = context.Fail();
            }

            return match.AddUstIfSuccess(ust);
        }
    }
}
