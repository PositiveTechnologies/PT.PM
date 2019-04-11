using PT.PM.Common.Nodes.Collections;
using System;
using System.Collections.Generic;
using MessagePack;

namespace PT.PM.Common.Nodes.Expressions
{
    [MessagePackObject]
    public class InvocationExpression : Expression
    {
        [Key(UstFieldOffset)]
        public Expression Target { get; set; }

        [Key(UstFieldOffset + 1)]
        public ArgsUst Arguments { get; set; } = new ArgsUst();

        public InvocationExpression(Expression target, ArgsUst arguments, TextSpan textSpan)
            : base(textSpan)
        {
            Target = target;
            Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        }

        public InvocationExpression(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public InvocationExpression()
        {
        }

        public int GetIndexOfArg(Ust ustNode)
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

        public override Expression[] GetArgs()
        {
            var result = new List<Expression> { Target };
            result.AddRange(Arguments.Collection);
            return result.ToArray();
        }

        public override Ust[] GetChildren()
        {
            var result = new Ust[] { Target, Arguments };
            return result;
        }

        public override string ToString()
        {
            string argsString = Arguments.ToString();
            return argsString.StartsWith("(") && argsString.EndsWith(")")
              ? Target + argsString
              : $"{Target}({argsString})";
        }
    }
}
