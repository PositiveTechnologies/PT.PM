using PT.PM.Common.Nodes.Collections;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Expressions
{
    public class InvocationExpression : Expression
    {
        public override NodeType NodeType => NodeType.InvocationExpression;

        public Expression Target { get; set; }

        public ArgsNode Arguments { get; set; }

        public InvocationExpression(Expression target, ArgsNode arguments, TextSpan textSpan)
            : base(textSpan)
        {
            Target = target;
            Arguments = arguments;
        }

        public InvocationExpression()
        {
        }

        public int GetIndexOfArg(UstNode ustNode)
        {
            if (ReferenceEquals(ustNode, Target))
                return 0;

            for (int i = 0; i < Arguments.Collection.Count; i++)
            {
                if (ReferenceEquals(ustNode, Arguments.Collection[i]))
                    return i + 1;
            }

            return -1;
        }

        public Expression[] GetTargetAndArgs()
        {
            var result = new List<Expression> { Target };
            result.AddRange(Arguments.Collection);
            return result.ToArray();
        }

        public override UstNode[] GetChildren()
        {
            var result = new UstNode[] { Target, Arguments };
            return result;
        }

        public override string ToString()
        {
            string argsString = Arguments.ToString();
            return argsString.StartsWith("(") && argsString.EndsWith(")")
              ? Target.ToString() + argsString
              : $"{Target}({argsString})";
        }
    }
}
