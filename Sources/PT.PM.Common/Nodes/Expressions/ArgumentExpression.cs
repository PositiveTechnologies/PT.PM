using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.Expressions
{
    public class ArgumentExpression : Expression
    {
        public InOutModifierLiteral Modifier { get; set; }

        public Expression Argument { get; set; }

        public ArgumentExpression(InOutModifierLiteral argumentModifier, Expression argument,
            TextSpan textSpan)
            : base(textSpan)
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
            return $"{Modifier.ToStringWithTrailSpace()}{Argument}";
        }
    }
}
