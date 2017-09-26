namespace PT.PM.Common.Nodes
{
    public class NotImplementedUst : Ust
    {
        public NotImplementedUst(TextSpan textSpan)
            : base(textSpan)
        {
            TextSpan = textSpan;
        }

        public NotImplementedUst()
        {
        }

        public override Ust[] GetChildren()
        {
            return ArrayUtils<Ust>.EmptyArray;
        }
    }
}
