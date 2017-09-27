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

            if (ust is FieldDeclaration fieldDeclaration)
            {
                return MatchFieldDeclaration(fieldDeclaration, context);
            }

            if (ust is VariableDeclarationExpression variableDeclarationExpression)
            {
                return MatchVariableDeclaration(variableDeclarationExpression, context);
            }

            return context.Fail();
        }

        private MatchingContext MatchFieldDeclaration(FieldDeclaration fieldDeclaration, MatchingContext context)
        {
            if (LocalVariable == true)
            {
                return context.Fail();
            }

            MatchingContext newContext = Modifiers.MatchSubset(fieldDeclaration.Modifiers, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = Type.Match(fieldDeclaration.Type, newContext);
            if (!newContext.Success)
            {
                return newContext;
            }

            if (fieldDeclaration.Variables.Count() != 1)
            {
                return newContext.Fail();
            }

            var assigment = fieldDeclaration.Variables.First();
            newContext = ((IPatternUst)Name).Match(assigment.Left, newContext);

            return newContext;
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

            MatchingContext newContext = Type.Match(variableDeclaration.Type, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            int matchedVarsCount = variableDeclaration.Variables.Where(v =>
            {
                newContext = Name.Match(v.Left, newContext);
                if (!newContext.Success)
                {
                    return false;
                }
                return Right?.Match(v.Right, newContext).Success ?? true;
            }).Count();

            if (matchedVarsCount == 0)
            {
                return context.Fail();
            }

            return context.AddMatchIfSuccess(variableDeclaration);
        }
    }
}
