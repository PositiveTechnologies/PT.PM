namespace PT.PM.Common.Nodes
{
    public class NotImplementedUst : Ust
    {
        public override UstKind Kind => UstKind.NotImplementedUst;

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
