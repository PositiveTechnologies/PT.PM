namespace PT.PM.Common.Nodes.Expressions
{
    public class YieldExpression : Expression
    {
        // if null: yield return Argument
        // if not null: yield break
        public Expression Argument { get; set; }

        public YieldExpression(Expression argument, TextSpan textSpan)
            : base(textSpan)
        {
            Argument = argument;
        }

        public YieldExpression()
        {
        }

        public override Expression[] GetArgs() => new[] { Argument };

        public override Ust[] GetChildren() => new[] { Argument };

        public override string ToString() => $"yield {Argument}";
    }
}
