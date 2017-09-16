using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PT.PM.Patterns.Nodes
{
    public class PatternVarOrFieldDeclaration : Expression
    {
        public override NodeType NodeType => NodeType.PatternVarOrFieldDeclaration;

        public bool LocalVariable { get; set; }

        public List<Token> Modifiers { get; set; }

        public Token Type { get; set; }

        // It has Expression type for backward compatibility
        public Expression Name { get; set; }

        public Expression Right { get; set; }

        public PatternVarOrFieldDeclaration(bool localVariable, List<Token> modifiers,
            Token type, Expression name, TextSpan textSpan)
            : base(textSpan)
        {
            LocalVariable = localVariable;
            Modifiers = modifiers;
            Type = type;
            Name = name;
            Right = null;
        }

        public PatternVarOrFieldDeclaration()
        {
            Modifiers = new List<Token>();
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
            result.AddRange(Modifiers);
            result.Add(Type);
            result.Add(Name);
            result.Add(Right);
            return result.ToArray();
        }

        public override int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return (int)NodeType;
            }

            if (other.NodeType == NodeType.PatternVarOrFieldDeclaration)
            {
                var otherPatternVarOrFieldDeclaration = (PatternVarOrFieldDeclaration)other;
                return GetChildren().CompareTo(otherPatternVarOrFieldDeclaration.GetChildren());
            }

            if (other.NodeType == NodeType.FieldDeclaration)
            {
                return CompareToFieldDeclaration((FieldDeclaration)other);
            }

            if (other.NodeType == NodeType.VariableDeclarationExpression)
            {
                return CompareToVariableDeclaration((VariableDeclarationExpression)other);
            }

            return (int)other.NodeType;
        }

        public override string ToString()
        {
            return $"{string.Join(", ", Modifiers)} {Type} {Name}";
        }

        private int CompareToFieldDeclaration(FieldDeclaration fieldDeclaration)
        {
            if (LocalVariable == true)
            {
                return NodeType.VariableDeclarationExpression - fieldDeclaration.NodeType;
            }

            int compareRes = Modifiers.CompareSubset(fieldDeclaration.Modifiers);
            if (compareRes != 0)
            {
                return compareRes;
            }

            compareRes = Type.CompareTo(fieldDeclaration.Type);
            if (compareRes != 0)
            {
                return compareRes;
            }

            if (fieldDeclaration.Variables.Count() != 1)
            {
                return 1 - fieldDeclaration.Variables.Count();
            }

            var assigment = fieldDeclaration.Variables.First();
            compareRes = Name.CompareTo(assigment.Left);

            return compareRes;
        }

        private int CompareToVariableDeclaration(VariableDeclarationExpression variableDeclaration)
        {
            if (LocalVariable == false)
            {
                return NodeType.FieldDeclaration - variableDeclaration.NodeType;
            }

            if (Modifiers.Count() != 0)
            {
                return Modifiers.Count();
            }

            int compareRes = Type.CompareTo(variableDeclaration.Type);
            if (compareRes != 0)
            {
                return compareRes;
            }

            var matchedVarsNum = variableDeclaration.Variables.Where(v =>
            {
                compareRes = Name.CompareTo(v.Left);
                if (compareRes != 0)
                {
                    return false;
                }
                return (Right?.CompareTo(v.Right) ?? 0) == 0;
            }).Count();

            if (matchedVarsNum == 0)
            {
                return variableDeclaration.Variables.Count() == 0 ? -1 : variableDeclaration.Variables.Count();
            }
            return 0;
        }

        public override Expression[] GetArgs()
        {
            return ArrayUtils<Expression>.EmptyArray;
        }
    }
}
