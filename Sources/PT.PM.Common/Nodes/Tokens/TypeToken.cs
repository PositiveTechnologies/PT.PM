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
            : this(type, default(TextSpan), null)
        {
        }

        public TypeToken(string type, TextSpan textSpan, FileNode fileNode)
            : this(new List<string>() {type},textSpan, fileNode)
        {
        }

        public TypeToken(IList<string> complexType, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
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
