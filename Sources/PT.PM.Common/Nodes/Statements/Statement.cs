namespace PT.PM.Common.Nodes.Statements
{
    public abstract class Statement : Ust
    {
        protected Statement(TextSpan textSpan)
            : base(textSpan)
        {
        }

        protected Statement()
        {
        }
    }
}
