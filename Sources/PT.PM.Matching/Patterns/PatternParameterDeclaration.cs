using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;

namespace PT.PM.Matching.Patterns
{
    public class PatternParameterDeclaration : PatternUst, IPatternAttributable
    {
        public PatternUst Type { get; set; }

        public PatternUst Name { get; set; }

        public PatternUst Initializer { get; set; }

        public List<PatternUst> Attributes { get; set; } = new List<PatternUst>();

        public PatternParameterDeclaration()
        {
        }

        public PatternParameterDeclaration(PatternUst type, PatternUst name, TextSpan textSpan = default)
            : base(textSpan)
        {
            Type = type;
            Name = name;
        }

        public override string ToString() => Type != null ? $"{Type} {Name}" : Name.ToString();

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            var parameterDeclaration = ust as ParameterDeclaration;
            if (parameterDeclaration == null)
            {
                return context.Fail();
            }

            MatchContext newContext;
            if (Attributes.Count > 0)
            {
                if (!(ust.Parent is IAttributable attributable))
                {
                    return context.Fail();
                }
                bool matchAttribute = false;
                foreach (var patternAttribute in Attributes)
                {
                    matchAttribute = false;
                    foreach (var attribute in attributable.Attributes)
                    {
                        matchAttribute = patternAttribute.MatchUst(attribute.Expression, context);
                        if (matchAttribute)
                        {
                            break;
                        }
                    }
                }
                if (!matchAttribute)
                {
                    return context.Fail();
                }
            }

            newContext = Type.MatchUst(parameterDeclaration.Type, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = Name is PatternVar
                ? newContext
                : Name.MatchUst(parameterDeclaration.Name, newContext);

            return newContext.AddUstIfSuccess(parameterDeclaration);
        }
    }
}
