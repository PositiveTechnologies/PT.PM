namespace PT.PM.Common.Nodes
{
    public class UstWithParent
    {
        public Ust Ust { get; }

        public UstWithParent Parent { get; }

        public UstWithParent(Ust ust, UstWithParent parent)
        {
            Ust = ust;
            Parent = parent;
        }

        public override string ToString()
        {
            return Ust.ToString();
        }
    }
}