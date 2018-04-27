namespace PT.PM.Common.Nodes.Expressions
{
    public class ArgumentExpression : Expression
    {
        public ArgumentModifier Modifier { get; set; }

        public Expression Argument { get; set; }

        public ArgumentExpression(ArgumentModifier argumentModifier, Expression argument)
        {
            Modifier = argumentModifier;
            Argument = argument;
        }

        public ArgumentExpression()
        {
        }

        public override Expression[] GetArgs() => new[] { Argument };

        public override Ust[] GetChildren() => new[] { Argument };

        public override string ToString()
        {
            return $"{Modifier} {Argument}";
        }
    }
}
