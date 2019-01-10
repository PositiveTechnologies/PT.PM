using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    public class NullLiteral : Literal
    {
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
