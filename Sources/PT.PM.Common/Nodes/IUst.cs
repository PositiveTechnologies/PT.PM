namespace PT.PM.Common.Nodes
{
    public interface IUst
    {
    }

    public interface IUst<T, RootT>
    {
        int KindId { get; }

        RootT Root { get; set; }

        T Parent { get; set; }

        TextSpan TextSpan { get; set; }
    }
}
