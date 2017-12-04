using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternVarOrFieldDeclaration : PatternUst<Ust>
    {
        public bool LocalVariable { get; set; }

        public List<PatternUst> Modifiers { get; set; } = new List<PatternUst>();

        public PatternUst Type { get; set; }

        public PatternAssignmentExpression Assignment { get; set; }

        public PatternVarOrFieldDeclaration()
        {
        }

        public PatternVarOrFieldDeclaration(bool localVariable, IEnumerable<PatternUst> modifiers,
            PatternUst type, PatternAssignmentExpression assignment, TextSpan textSpan)
            : base(textSpan)
        {
            LocalVariable = localVariable;
            Modifiers = modifiers?.ToList() ?? new List<PatternUst>();
            Type = type;
            Assignment = assignment;
        }

        public override string ToString() => $"{string.Join(", ", Modifiers)} {Type} {Assignment}";

        public override MatchContext Match(Ust ust, MatchContext context)
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

        private MatchContext MatchFieldDeclaration(FieldDeclaration fieldDeclaration, MatchContext context)
        {
            if (LocalVariable == true || fieldDeclaration.Variables.Count() != 1)
            {
                return context.Fail();
            }

            MatchContext newContext = Modifiers.MatchSubset(fieldDeclaration.Modifiers, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = Type.MatchUst(fieldDeclaration.Type, newContext);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = EnumerateVarsOrFiels(fieldDeclaration.Variables, newContext);

            return newContext;
        }

        private MatchContext MatchVariableDeclaration(VariableDeclarationExpression variableDeclaration, MatchContext context)
        {
            if (LocalVariable == false || Modifiers.Count() != 0)
            {
                return context.Fail();
            }

            MatchContext newContext = Type.MatchUst(variableDeclaration.Type, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = EnumerateVarsOrFiels(variableDeclaration.Variables, newContext);

            return newContext;
        }

        private MatchContext EnumerateVarsOrFiels(List<AssignmentExpression> variables, MatchContext context)
        {
            var matchedTextSpans = new List<TextSpan>();

            bool success = false;
            foreach (AssignmentExpression variable in variables)
            {
                var altContext = MatchContext.CreateWithInputParamsAndVars(context);
                MatchContext match;
                if (Assignment.Right != null)
                {
                    match = Assignment.Match(variable, altContext);
                }
                else
                {
                    match = Assignment.Left.MatchUst(variable.Left, altContext);
                }
                if (match.Success)
                {
                    success = true;
                    matchedTextSpans.AddRange(match.Locations);
                    if (!context.FindAllAlternatives)
                    {
                        break;
                    }
                }
            }

            if (success)
            {
                context = context.AddMatches(matchedTextSpans);
            }
            else
            {
                context = context.Fail();
            }

            return context;
        }
    }
}
