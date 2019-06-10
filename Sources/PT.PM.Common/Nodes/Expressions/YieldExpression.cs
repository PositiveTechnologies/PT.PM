using MessagePack;

namespace PT.PM.Common.Nodes.Expressions
{
    [MessagePackObject]
    public class YieldExpression : Expression
    {
        [Key(0)] public override UstType UstType => UstType.YieldExpression;

        // if null: yield return Argument
        // if not null: yield break
        [Key(UstFieldOffset)]
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
