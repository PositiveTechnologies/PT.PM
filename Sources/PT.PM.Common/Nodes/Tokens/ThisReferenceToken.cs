using PT.PM.Common.Nodes.Tokens;
namespace PT.PM.Common.Nodes.Expressions
{
    public class ThisReferenceToken : Token
    {
        public override UstKind Kind => UstKind.ThisReferenceToken;

        public ThisReferenceToken(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public ThisReferenceToken()
        {
        }

        public override string TextValue => "this";
    }
}
