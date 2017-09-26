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

        public List<PatternBase> Modifiers { get; set; }

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
            Modifiers = new List<PatternBase>();
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

        public override bool Match(Ust ust, MatchingContext context)
        {
            if (ust == null)
            {
                return false;
            }

            if (ust.Kind == UstKind.FieldDeclaration)
            {
                return MatchFieldDeclaration((FieldDeclaration)ust, context);
            }

            if (ust.Kind == UstKind.VariableDeclarationExpression)
            {
                return MatchVariableDeclaration((VariableDeclarationExpression)ust, context);
            }

            return false;
        }

        private bool MatchFieldDeclaration(FieldDeclaration fieldDeclaration, MatchingContext context)
        {
            if (LocalVariable == true)
            {
                return false;
            }

            bool match = Modifiers.MatchSubset(fieldDeclaration.Modifiers, context);
            if (!match)
            {
                return match;
            }

            match = Type.Equals(fieldDeclaration.Type);
            if (!match)
            {
                return match;
            }

            if (fieldDeclaration.Variables.Count() != 1)
            {
                return false;
            }

            var assigment = fieldDeclaration.Variables.First();
            match = ((IPatternUst)Name).Match(assigment.Left, context);

            return match;
        }

        private bool MatchVariableDeclaration(VariableDeclarationExpression variableDeclaration, MatchingContext context)
        {
            if (LocalVariable == false)
            {
                return false;
            }

            if (Modifiers.Count() != 0)
            {
                return false;
            }

            bool match = Type.Match(variableDeclaration.Type, context);
            if (!match)
            {
                return match;
            }

            var matchedVarsNumber = variableDeclaration.Variables.Where(v =>
            {
                match = Name.Match(v.Left, context);
                if (!match)
                {
                    return match;
                }
                return Right?.Match(v.Right, context) ?? true;
            }).Count();

            if (matchedVarsNumber == 0)
            {
                return false;
            }

            return true;
        }
    }
}
