using MessagePack;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Statements
{
    [MessagePackObject]
    public class ReturnStatement : Statement
    {
        [Key(0)] public override UstType UstType => UstType.ReturnStatement;

        [Key(UstFieldOffset)]
        public Expression Return { get; set; }

        public ReturnStatement(Expression returnExpression, TextSpan textSpan)
            : base(textSpan)
        {
            Return = returnExpression;
        }

        public ReturnStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] {Return};
        }

        public override string ToString()
        {
            return $"return {Return};";
        }
    }
}
