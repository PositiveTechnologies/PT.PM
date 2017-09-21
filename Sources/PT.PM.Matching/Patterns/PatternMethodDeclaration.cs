using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.TypeMembers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternMethodDeclaration : Expression
    {
        public override UstKind Kind => UstKind.PatternMethodDeclaration;

        public List<Token> Modifiers { get; set; }

        public Token Name { get; set; }

        public bool AnyBody { get; set; }

        public Ust Body { get; set; }

        public PatternMethodDeclaration(List<Token> modifiers, Token name,
            PatternExpressionInsideNode body, TextSpan textSpan)
            : base(textSpan)
        {
            Modifiers = modifiers;
            Name = name;
            AnyBody = false;
            Body = body ?? throw new ArgumentNullException("body should not be null");
        }

        public PatternMethodDeclaration(List<Token> modifiers, Token name, bool anyBody,
            TextSpan textSpan) : base(textSpan)
        {
            initFields(modifiers, name, anyBody);
        }

        public PatternMethodDeclaration(List<Token> modifiers, Token name, bool anyBody)
        {
            initFields(modifiers, name, anyBody);
        }

        public PatternMethodDeclaration()
        {
            Modifiers = new List<Token>();
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.AddRange(Modifiers);
            result.Add(Name);
            result.Add(Body);
            return result.ToArray();
        }

        public override int CompareTo(Ust other)
        {
            if (other == null)
            {
                return (int)Kind;
            }

            if (other.Kind == UstKind.PatternMethodDeclaration)
            {
                var otherPatternMethodDeclaration = (PatternMethodDeclaration)other;
                return GetChildren().CompareTo(otherPatternMethodDeclaration.GetChildren());
            }

            if (other.Kind != UstKind.MethodDeclaration)
            {
                return UstKind.MethodDeclaration - other.Kind;
            }

            var methodDeclaration = (MethodDeclaration)other;

            int compareRes = Modifiers.CompareSubset(methodDeclaration.Modifiers);
            if (compareRes != 0)
            {
                return compareRes;
            }

            compareRes = Name.CompareTo(methodDeclaration.Name);
            if (compareRes != 0)
            {
                return compareRes;
            }

            if (!AnyBody)
            {
                if (Body != null)
                {
                    compareRes = Body.CompareTo(methodDeclaration.Body);
                    if (compareRes != 0)
                    {
                        return compareRes;
                    }
                }
                else if (methodDeclaration.Body != null)
                {
                    return -methodDeclaration.Body.CompareTo(Body);
                }
            }

            return 0;
        }

        public override string ToString()
        {
            var result = $"{string.Join(", ", Modifiers)} {Name}() ";
            result += " { " + (Body?.ToString() ?? "") + " }";

            return result;
        }

        private void initFields(List<Token> modifiers, Token name, bool anyBody)
        {
            Modifiers = modifiers;
            Name = name;
            AnyBody = anyBody;
            if (anyBody)
            {
                Body = null;
            }
            else
            {
                Body = new BlockStatement(Enumerable.Empty<Statement>());
            }
        }

        public override Expression[] GetArgs()
        {
            return ArrayUtils<Expression>.EmptyArray;
        }
    }
}
