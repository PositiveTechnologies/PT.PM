using System;

namespace PT.PM.Common.Nodes.Tokens
{
    public class IdToken : Token
    {
        public override NodeType NodeType => NodeType.IdToken;

        public virtual string Id { get; set; }

        public override string TextValue => Id;

        public IdToken(string id, TextSpan textSpan, RootNode rootNode)
            : base(textSpan, rootNode)
        {
            Id = id;
        }

        public IdToken(string id)
        {
            Id = id;
        }

        public IdToken()
        {
        }

        public override int CompareTo(UstNode other)
        {
            var baseCompareResult = base.CompareTo(other);
            if (baseCompareResult != 0)
            {
                return baseCompareResult;
            }

            return String.Compare(Id, ((IdToken)other).Id, StringComparison.Ordinal);
        }
    }
}
