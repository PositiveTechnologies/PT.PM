using PT.PM.Common.Nodes;
using PT.PM.Common;

namespace PT.PM.Dsl
{
    public class LangCodeNode : UstNode
    {
        public override NodeType NodeType => NodeType.LangCodeNode;

        public Language Language { get; set; }

        public string Code { get; set; }

        public LangCodeNode(Language language, string code, TextSpan textSpan)
            : base(textSpan)
        {
            Language = language;
            Code = code;
        }

        public override UstNode[] GetChildren()
        {
            return ArrayUtils<UstNode>.EmptyArray;
        }
    }
}
