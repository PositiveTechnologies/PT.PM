using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Tokens;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PT.PM.Patterns.Nodes
{
    public class PatternClassDeclaration : Expression
    {
        public override UstKind Kind => UstKind.PatternClassDeclaration;

        public List<Token> Modifiers { get; set; }

        public Token Name { get; set; }

        public List<Token> BaseTypes { get; set; }

        public PatternExpressionInsideNode Body { get; set; }

        public PatternClassDeclaration(List<Token> modifiers, Token name, List<Token> baseTypes,
            PatternExpressionInsideNode body, TextSpan textSpan)
            : base(textSpan)
        {
            Modifiers = modifiers;
            Name = name;
            BaseTypes = baseTypes;
            Body = body;
        }

        public PatternClassDeclaration()
        {
            Modifiers = new List<Token>();
            BaseTypes = new List<Token>();
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.AddRange(Modifiers);
            result.Add(Name);
            result.AddRange(BaseTypes);
            result.Add(Body);
            return result.ToArray();
        }

        public override int CompareTo(Ust other)
        {
            if (other == null)
            {
                return (int)Kind;
            }

            if (other.Kind == UstKind.PatternClassDeclaration)
            {
                var otherPatternClassDeclaration = (PatternClassDeclaration)other;
                return GetChildren().CompareTo(otherPatternClassDeclaration.GetChildren());
            }

            if (other.Kind != UstKind.TypeDeclaration)
            {
                return UstKind.TypeDeclaration - other.Kind;
            }

            var typeDeclaration = (TypeDeclaration)other;

            int compareRes = Modifiers.CompareSubset(typeDeclaration.Modifiers);
            if (compareRes != 0)
            {
                return compareRes;
            }

            if (Name != null)
            {
                compareRes = Name.CompareTo(typeDeclaration.Name);
                if (compareRes != 0)
                {
                    return compareRes;
                }
            }

            compareRes = BaseTypes.CompareSubset(typeDeclaration.BaseTypes);

            var baseTypesToMatch = new List<Ust>(BaseTypes);
            if (compareRes != 0)
            {
                return compareRes;
            }

            if (Body != null)
            {
                if (!typeDeclaration.TypeMembers.Any(m => Body.CompareTo(m) == 0))
                {
                    return -1;
                }
            }

            return 0;
        }

        public override string ToString()
        {
            var result = $"{string.Join(", ", Modifiers)} class {Name} ";
            if (BaseTypes.Any())
            {
                result += $": {string.Join(", ", BaseTypes.Select(t => t.ToString()))}";
            }
            result += " { " + (Body?.ToString() ?? "") + " }";

            return result;
        }

        public override Expression[] GetArgs()
        {
            return ArrayUtils<Expression>.EmptyArray;
        }
    }
}
