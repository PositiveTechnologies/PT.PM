using PT.PM.Common.Nodes;

namespace PT.PM.Common
{
    public interface IUstSerializer
    {
        ILogger Logger { get; set; }
        UstSerializeFormat DataFormat { get; }
        Ust Deserialize(string data);
        string Serialize(Ust node);
    }
}
