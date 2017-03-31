using PT.PM.Common.Nodes.Tokens;
namespace PT.PM.Common.Nodes.Expressions
{
    public class ThisReferenceToken : Token
    {
        public override NodeType NodeType => NodeType.ThisReferenceToken;

        public ThisReferenceToken(TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
        }

        public ThisReferenceToken()
        {
        }

        public override string TextValue => "this";
    }
}
