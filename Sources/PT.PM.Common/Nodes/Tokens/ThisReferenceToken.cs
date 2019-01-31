using MessagePack;

namespace PT.PM.Common.Nodes.Tokens
{
    [MessagePackObject]
    public class ThisReferenceToken : Token
    {
        public ThisReferenceToken(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public ThisReferenceToken()
        {
        }

        [IgnoreMember]
        public override string TextValue => "this";
    }
}
