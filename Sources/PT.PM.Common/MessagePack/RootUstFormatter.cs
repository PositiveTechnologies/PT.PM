using System;
using MessagePack;
using MessagePack.Formatters;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.MessagePack
{
    public class RootUstFormatter : IMessagePackFormatter<RootUst>
    {
        public RootUst CurrentRoot { get; internal set; }

        public BinaryFile SerializedFile { get; private set; }

        public static RootUstFormatter CreateWriter()
        {
            return new RootUstFormatter();
        }

        public static RootUstFormatter CreateReader(BinaryFile serializedFile)
        {
            if (serializedFile == null)
            {
                throw new ArgumentNullException(nameof(serializedFile));
            }

            return new RootUstFormatter
            {
                SerializedFile = serializedFile
            };
        }

        private RootUstFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, RootUst value, IFormatterResolver formatterResolver)
        {
            int newOffset = offset;
            CurrentRoot = value;

            var languageFormatter = formatterResolver.GetFormatter<Language>();
            newOffset += languageFormatter.Serialize(ref bytes, newOffset, value.Language, formatterResolver);

            var fileFormatter = formatterResolver.GetFormatter<TextFile>();
            newOffset += fileFormatter.Serialize(ref bytes, newOffset, value.SourceFile, formatterResolver);

            var ustsFormatter = formatterResolver.GetFormatter<Ust[]>();
            newOffset += ustsFormatter.Serialize(ref bytes, newOffset, value.Nodes, formatterResolver);

            var commentsFormatter = formatterResolver.GetFormatter<CommentLiteral[]>();
            newOffset += commentsFormatter.Serialize(ref bytes, newOffset, value.Comments, formatterResolver);

            newOffset += MessagePackBinary.WriteInt32(ref bytes, newOffset, value.LineOffset);

            var textSpanFormatter = formatterResolver.GetFormatter<TextSpan>();
            newOffset += textSpanFormatter.Serialize(ref bytes, newOffset, value.TextSpan, formatterResolver);

            newOffset += MessagePackBinary.WriteInt32(ref bytes, newOffset, value.Key);

            return newOffset - offset;
        }

        public RootUst Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            int newOffset = offset;
            try
            {
                var languageFormatter = formatterResolver.GetFormatter<Language>();
                Language language = languageFormatter.Deserialize(bytes, newOffset, formatterResolver, out int size);
                newOffset += size;

                var fileFormatter = formatterResolver.GetFormatter<TextFile>();
                TextFile sourceFile = fileFormatter.Deserialize(bytes, newOffset, formatterResolver, out size);
                newOffset += size;

                var textSpanFormatter = (TextSpanFormatter) formatterResolver.GetFormatter<TextSpan>();
                if (textSpanFormatter is TextSpanFormatter formatter)
                {
                    formatter.SourceFile = sourceFile;
                }

                var ustsFormatter = formatterResolver.GetFormatter<Ust[]>();
                Ust[] nodes = ustsFormatter.Deserialize(bytes, newOffset, formatterResolver, out size);
                newOffset += size;

                var commentsFormatter = formatterResolver.GetFormatter<CommentLiteral[]>();
                CommentLiteral[] comments =
                    commentsFormatter.Deserialize(bytes, newOffset, formatterResolver, out size);
                newOffset += size;

                int lineOffset = MessagePackBinary.ReadInt32(bytes, newOffset, out size);
                newOffset += size;

                TextSpan textSpan =
                    textSpanFormatter.Deserialize(bytes, newOffset, formatterResolver, out size);
                newOffset += size;

                int key = MessagePackBinary.ReadInt32(bytes, newOffset, out size);
                newOffset += size;

                CurrentRoot = new RootUst(sourceFile, language)
                {
                    Key = key,
                    Nodes = nodes,
                    Comments = comments,
                    LineOffset = lineOffset,
                    TextSpan = textSpan
                };

                readSize = newOffset - offset;

                return CurrentRoot;
            }
            catch (InvalidOperationException ex) // Catch incorrect format exceptions
            {
                throw new ReadException(SerializedFile, ex, $"Error during reading {nameof(RootUst)} at {newOffset} offset; Message: {ex.Message}");
            }
        }
    }
}