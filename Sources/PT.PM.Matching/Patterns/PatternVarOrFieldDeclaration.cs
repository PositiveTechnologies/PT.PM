using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternVarOrFieldDeclaration : PatternBase
    {
        public bool LocalVariable { get; set; }

        public List<PatternBase> Modifiers { get; set; } = new List<PatternBase>();

        public PatternBase Type { get; set; }

        public PatternBase Name { get; set; }

        public PatternBase Right { get; set; }

        public PatternVarOrFieldDeclaration(bool localVariable, IEnumerable<PatternBase> modifiers,
            PatternBase type, PatternBase name, TextSpan textSpan)
            : base(textSpan)
        {
            LocalVariable = localVariable;
            Modifiers = modifiers?.ToList() ?? new List<PatternBase>();
            Type = type;
            Name = name;
            Right = null;
        }

        public PatternVarOrFieldDeclaration()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.AddRange(Modifiers);
            result.Add(Type);
            result.Add(Name);
            result.Add(Right);
            return result.ToArray();
        }

        public override string ToString() => $"{string.Join(", ", Modifiers)} {Type} {Name}";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            if (ust == null)
            {
                return context;
            }

            if (ust.Kind == UstKind.FieldDeclaration)
            {
                return MatchFieldDeclaration((FieldDeclaration)ust, context);
            }

            if (ust.Kind == UstKind.VariableDeclarationExpression)
            {
                return MatchVariableDeclaration((VariableDeclarationExpression)ust, context);
            }

            return context.Fail();
        }

        private MatchingContext MatchFieldDeclaration(FieldDeclaration fieldDeclaration, MatchingContext context)
        {
            if (LocalVariable == true)
            {
                return context.Fail();
            }

            MatchingContext match = Modifiers.MatchSubset(fieldDeclaration.Modifiers, context);
            if (!match.Success)
            {
                return match;
            }

            match = Type.Match(fieldDeclaration.Type, match);
            if (!match.Success)
            {
                return match;
            }

            if (fieldDeclaration.Variables.Count() != 1)
            {
                return match.Fail();
            }

            var assigment = fieldDeclaration.Variables.First();
            match = ((IPatternUst)Name).Match(assigment.Left, match);

            return match;
        }

        private MatchingContext MatchVariableDeclaration(VariableDeclarationExpression variableDeclaration, MatchingContext context)
        {
            if (LocalVariable == false)
            {
                return context.Fail();
            }

            if (Modifiers.Count() != 0)
            {
                return context.Fail();
            }

            MatchingContext match = Type.Match(variableDeclaration.Type, context);
            if (!match.Success)
            {
                return match;
            }

            int matchedVarsCount = variableDeclaration.Variables.Where(v =>
            {
                match = Name.Match(v.Left, match);
                if (!match.Success)
                {
                    return false;
                }
                return Right?.Match(v.Right, match).Success ?? true;
            }).Count();

            if (matchedVarsCount == 0)
            {
                return context.Fail();
            }

            return context.AddUstIfSuccess(variableDeclaration);
        }
    }
}
