using MessagePack;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.Expressions
{
    [MessagePackObject]
    public class ArgumentExpression : Expression
    {
        [Key(UstFieldOffset)]
        public InOutModifierLiteral Modifier { get; set; }

        [Key(UstFieldOffset + 1)]
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
