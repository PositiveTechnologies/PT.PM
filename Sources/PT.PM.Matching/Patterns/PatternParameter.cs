using PT.PM.Common;
using PT.PM.Common.Nodes.TypeMembers;

namespace PT.PM.Matching.Patterns
{
    public class PatternParameter : PatternUst<Parameter>
    {
        public PatternUst Type { get; set; }

        public PatternUst Name { get; set; }

        public PatternUst Initializer { get; set; }

        public PatternParameter()
        {
        }

        public PatternParameter(PatternUst type, PatternUst name, TextSpan textSpan = default)
            : base(textSpan)
        {
            Type = type;
            Name = name;
        }

        public override string ToString() => Type != null ? $"{Type} {Name}" : Name.ToString();

        public override MatchContext Match(Parameter parameterDeclaration, MatchContext context)
        {
            MatchContext newContext = Type.MatchUst(parameterDeclaration.Type, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = Name.MatchUst(parameterDeclaration.Name, newContext);

            return newContext.AddUstIfSuccess(parameterDeclaration);
        }
    }
}
