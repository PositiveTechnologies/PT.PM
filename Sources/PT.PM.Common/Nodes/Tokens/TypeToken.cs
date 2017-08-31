using System;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Tokens
{
    public class TypeToken : Token
    {
        public override NodeType NodeType => NodeType.TypeToken;

        public string TypeText { get; set; }

        public override string TextValue => TypeText;

        public TypeToken(string type)
            : this(type, default(TextSpan))
        {
        }

        public TypeToken(string type, TextSpan textSpan)
            : this(new List<string>() {type},textSpan)
        {
        }

        public TypeToken(IList<string> complexType, TextSpan textSpan)
            : base(textSpan)
        {
            TypeText = string.Join(".", complexType);
        }

        public TypeToken()
        {
        }

        public override int CompareTo(UstNode other)
        {
            var baseCompareResult = base.CompareTo(other);
            if (baseCompareResult != 0)
            {
                return baseCompareResult;
            }

            TypeToken otherTypeToken = (TypeToken)other;
            return String.Compare(TextValue, otherTypeToken.TextValue, StringComparison.Ordinal);
        }
    }
}
