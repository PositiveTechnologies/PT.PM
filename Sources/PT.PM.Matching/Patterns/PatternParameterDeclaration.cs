using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.TypeMembers;

namespace PT.PM.Matching.Patterns
{
    public class PatternParameterDeclaration : PatternUst
    {
        public PatternUst Type { get; set; }

        public PatternUst Name { get; set; }

        public PatternUst Initializer { get; set; }

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

        public override MatchContext Match(Ust ust, MatchContext context)
        {
            var parameterDeclaration = ust as ParameterDeclaration;
            if (parameterDeclaration == null)
            {
                return context.Fail();
            }
            
            MatchContext newContext = Type.Match(parameterDeclaration.Type, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = Name.Match(parameterDeclaration.Name, newContext);

            return newContext.AddUstIfSuccess(parameterDeclaration);
        }
    }
}
