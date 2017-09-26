using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;

namespace PT.PM.Matching.Patterns
{
    public class PatternParameterDeclaration : PatternBase
    {
        public PatternBase Type { get; set; }

        public PatternBase Name { get; set; }

        public PatternBase Initializer { get; set; }

        public PatternParameterDeclaration()
        {
        }

        public PatternParameterDeclaration(PatternBase type, PatternBase name, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Type = type;
            Name = name;
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust> { Name };
            return result.ToArray();
        }

        public override string ToString() => Type != null ? $"{Type} {Name}" : Name.ToString();

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext match;
            if (ust is ParameterDeclaration parameterDeclaration)
            {
                match = Type.Match(parameterDeclaration.Type, context);
                if (!match.Success)
                {
                    return match;
                }

                match = Name.Match(parameterDeclaration.Name, match);
            }
            else
            {
                match = context.Fail();
            }
            return match.AddUstIfSuccess(ust);
        }
    }
}
