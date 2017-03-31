using System.Collections.Generic;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Ust
{
    public abstract class Ust
    {
        public abstract UstType Type { get; }

        public string FileName { get; set; }

        public LanguageFlags SourceLanguages { get; set; }

        public FileNode Root { get; set; }

        public CommentLiteral[] Comments { get; set; } = ArrayUtils<CommentLiteral>.EmptyArray;

        protected Ust()
        {
        }

        protected Ust(FileNode root, LanguageFlags sourceLanguages)
        {
            Root = root;
            SourceLanguages = sourceLanguages;
        }
    }
}
