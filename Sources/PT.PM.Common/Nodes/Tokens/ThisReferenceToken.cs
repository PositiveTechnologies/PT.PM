using MessagePack;

namespace PT.PM.Common.Nodes.Tokens
{
    [MessagePackObject]
    public class ThisReferenceToken : Token
    {
        [Key(0)] public override UstType UstType => UstType.ThisReferenceToken;

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
