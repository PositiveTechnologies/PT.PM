using PT.PM.Common.Nodes.Tokens;
namespace PT.PM.Common.Nodes.Expressions
{
    public class ThisReferenceToken : Token
    {
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
