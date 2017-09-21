using PT.PM.Common.Nodes;

namespace PT.PM.Common
{
    public interface IUstNodeSerializer
    {
        ILogger Logger { get; set; }
        UstNodeSerializationFormat DataFormat { get; }
        Ust Deserialize(string data);
        string Serialize(Nodes.Ust node);
    }
}
