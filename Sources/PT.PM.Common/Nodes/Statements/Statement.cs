using MessagePack;

namespace PT.PM.Common.Nodes.Statements
{
    [MessagePackObject]
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
