using PT.PM.Common.Nodes;

namespace PT.PM.Common
{
    public interface IUstSerializer
    {
        ILogger Logger { get; set; }
        UstFormat DataFormat { get; }
        Ust Deserialize(string data);
        string Serialize(Nodes.Ust node);
    }
}
