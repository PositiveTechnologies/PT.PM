using System;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public class CommentLiteral : Literal
    {
        public ReadOnlyMemory<char> Comment { get; set; }

        //private string cachedString;
        //public string CachedString => cachedString ?? (cachedString = Comment.ToString());

        public override string TextValue => Comment.ToString();

        public CommentLiteral(CodeFile codeFile, TextSpan textSpan)
            : base(textSpan)
        {
            Comment = codeFile.Code.AsMemory(ref textSpan);
        }

        public CommentLiteral()
        {
        }
    }
}
