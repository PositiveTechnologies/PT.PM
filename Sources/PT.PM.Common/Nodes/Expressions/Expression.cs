namespace PT.PM.Common.Nodes.Expressions
{
    public abstract class Expression : Ust
    {
        protected Expression(TextSpan textSpan)
            : base(textSpan)
        {
        }

        protected Expression()
        {
        }

        public abstract Expression[] GetArgs();
    }
}
