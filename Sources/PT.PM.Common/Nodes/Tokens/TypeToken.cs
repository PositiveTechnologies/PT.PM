using System;
using System.Collections.Generic;
using MessagePack;

namespace PT.PM.Common.Nodes.Tokens
{
    [MessagePackObject]
    public class TypeToken : Token
    {
        [Key(UstFieldOffset)]
        public string TypeText { get; set; }

        [IgnoreMember]
        public override string TextValue => TypeText;

        public TypeToken(string type)
            : this(type, default)
        {
        }

        public TypeToken(string type, TextSpan textSpan)
            : this(new List<string>() { type }, textSpan)
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

        public override int CompareTo(Ust other)
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
