using MessagePack;
using MessagePack.Formatters;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.MessagePack
{
    public class RootUstFormatter : IMessagePackFormatter<RootUst>
    {
        public RootUst CurrentRoot { get; internal set; }

        public int Serialize(ref byte[] bytes, int offset, RootUst value, IFormatterResolver formatterResolver)
        {
            CurrentRoot = value;

            int writeSize = 0;

            var languageFormatter = formatterResolver.GetFormatter<Language>();
            writeSize += languageFormatter.Serialize(ref bytes, offset + writeSize, value.Language, formatterResolver);

            var fileFormatter = formatterResolver.GetFormatter<CodeFile>();
            writeSize += fileFormatter.Serialize(ref bytes, offset + writeSize, value.SourceCodeFile, formatterResolver);

            var ustsFormatter = formatterResolver.GetFormatter<Ust[]>();
            writeSize += ustsFormatter.Serialize(ref bytes, offset + writeSize, value.Nodes, formatterResolver);

            var commentsFormatter = formatterResolver.GetFormatter<CommentLiteral[]>();
            writeSize += commentsFormatter.Serialize(ref bytes, offset + writeSize, value.Comments, formatterResolver);

            writeSize += MessagePackBinary.WriteInt32(ref bytes, offset + writeSize, value.LineOffset);

            var textSpanFormatter = formatterResolver.GetFormatter<TextSpan>();
            writeSize += textSpanFormatter.Serialize(ref bytes, offset + writeSize, value.TextSpan, formatterResolver);
            writeSize += MessagePackBinary.WriteString(ref bytes, offset + writeSize, value.Key);

            return writeSize;
        }

        public RootUst Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            readSize = 0;

            var languageFormatter = formatterResolver.GetFormatter<Language>();
            Language language = languageFormatter.Deserialize(bytes, offset, formatterResolver, out int size);
            readSize += size;

            var fileFormatter = formatterResolver.GetFormatter<CodeFile>();
            CodeFile codeFile = fileFormatter.Deserialize(bytes, offset + readSize, formatterResolver, out size);
            readSize += size;

            var textSpanFormatter = (TextSpanFormatter)formatterResolver.GetFormatter<TextSpan>();
            textSpanFormatter.CodeFile = codeFile;

            var ustsFormatter = formatterResolver.GetFormatter<Ust[]>();
            Ust[] nodes = ustsFormatter.Deserialize(bytes, offset + readSize, formatterResolver, out size);
            readSize += size;

            var commentsFormatter = formatterResolver.GetFormatter<CommentLiteral[]>();
            CommentLiteral[] comments =
                commentsFormatter.Deserialize(bytes, offset + readSize, formatterResolver, out size);
            readSize += size;

            int lineOffset = MessagePackBinary.ReadInt32(bytes, offset + readSize, out size);
            readSize += size;

            TextSpan textSpan = textSpanFormatter.Deserialize(bytes, offset + readSize, formatterResolver, out size);
            readSize += size;

            CurrentRoot = new RootUst(codeFile, language)
            {
                Nodes = nodes,
                Comments = comments,
                LineOffset = lineOffset,
                TextSpan = textSpan
            };

            return CurrentRoot;
        }
    }
}