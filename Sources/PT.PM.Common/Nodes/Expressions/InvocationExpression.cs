using PT.PM.Common.Nodes.Collections;

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
