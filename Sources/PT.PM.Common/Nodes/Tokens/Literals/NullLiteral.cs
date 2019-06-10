using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    public class NullLiteral : Literal
    {
        [Key(0)] public override UstType UstType => UstType.NullLiteral;

        public NullLiteral(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public NullLiteral()
        {
        }

        [IgnoreMember]
        public override string TextValue => "null";
    }
}
