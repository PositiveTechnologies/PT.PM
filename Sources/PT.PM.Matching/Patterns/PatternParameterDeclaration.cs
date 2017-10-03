using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;

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

        public PatternParameterDeclaration(PatternUst type, PatternUst name, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Type = type;
            Name = name;
        }

        public override string ToString() => Type != null ? $"{Type} {Name}" : Name.ToString();

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is ParameterDeclaration parameterDeclaration)
            {
                newContext = Type.Match(parameterDeclaration.Type, context);
                if (!newContext.Success)
                {
                    return newContext;
                }

                newContext = Name.Match(parameterDeclaration.Name, newContext);
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext.AddUstIfSuccess(ust);
        }
    }
}
