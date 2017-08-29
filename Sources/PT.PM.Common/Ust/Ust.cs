using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Ust
{
    public class Ust
    {
        public string FileName { get; set; }

        public LanguageFlags SourceLanguages { get; set; }

        public FileNode Root { get; set; }

        public CommentLiteral[] Comments { get; set; } = ArrayUtils<CommentLiteral>.EmptyArray;

        public Ust()
        {
        }

        public Ust(FileNode root, LanguageFlags sourceLanguages)
        {
            Root = root;
            SourceLanguages = sourceLanguages;
        }
    }
}
