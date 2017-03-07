using PT.PM.Common.Nodes;

namespace PT.PM.Common
{
    public interface IAstNodeSerializer
    {
        ILogger Logger { get; set; }
        UstNodeSerializationFormat DataFormat { get; }
        UstNode Deserialize(string data, LanguageFlags sourceLanguage);
        string Serialize(UstNode node);
    }
}
