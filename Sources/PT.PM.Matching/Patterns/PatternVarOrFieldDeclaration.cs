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

        public PatternAssignmentExpression Assignment { get; set; }

        public PatternVarOrFieldDeclaration()
        {
        }

        public PatternVarOrFieldDeclaration(bool localVariable, IEnumerable<PatternBase> modifiers,
            PatternBase type, PatternAssignmentExpression assignment, TextSpan textSpan)
            : base(textSpan)
        {
            LocalVariable = localVariable;
            Modifiers = modifiers?.ToList() ?? new List<PatternBase>();
            Type = type;
            Assignment = assignment;
        }

        public override string ToString() => $"{string.Join(", ", Modifiers)} {Type} {Assignment}";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
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
            if (LocalVariable == true || fieldDeclaration.Variables.Count() != 1)
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

            newContext = EnumerateVarsOrFiels(fieldDeclaration.Variables, newContext);

            return newContext;
        }

        private MatchingContext MatchVariableDeclaration(VariableDeclarationExpression variableDeclaration, MatchingContext context)
        {
            if (LocalVariable == false || Modifiers.Count() != 0)
            {
                return context.Fail();
            }

            MatchingContext newContext = Type.Match(variableDeclaration.Type, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = EnumerateVarsOrFiels(variableDeclaration.Variables, newContext);

            return newContext;
        }

        private MatchingContext EnumerateVarsOrFiels(List<AssignmentExpression> variables, MatchingContext context)
        {
            var matchedTextSpans = new List<TextSpan>();
            foreach (AssignmentExpression variable in variables)
            {
                var altContext = MatchingContext.CreateWithInputParamsAndVars(context);
                MatchingContext match = Assignment.Match(variable, altContext);
                if (match.Success)
                {
                    matchedTextSpans.AddRange(match.Locations);
                    if (!context.FindAllAlternatives)
                    {
                        break;
                    }
                }
            }
            return context.AddMatches(matchedTextSpans);
        }
    }
}
