using System;
using System.Collections.Generic;
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

        public HashSet<IFile> SourceFiles { get; private set; }

        public static RootUstFormatter CreateWriter()
        {
            return new RootUstFormatter();
        }

        public static RootUstFormatter CreateReader(BinaryFile serializedFile, HashSet<IFile> sourceFiles)
        {
            return new RootUstFormatter
            {
                SerializedFile = serializedFile ?? throw new ArgumentNullException(nameof(serializedFile)),
                SourceFiles = sourceFiles ?? throw new ArgumentNullException(nameof(sourceFiles))
            };
        }

        private RootUstFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, RootUst value, IFormatterResolver formatterResolver)
        {
            int newOffset = offset;
            CurrentRoot = value;

            var localSourceFiles = new List<TextFile> {CurrentRoot.SourceFile};
            value.ApplyActionToDescendantsAndSelf(ust =>
            {
                foreach (TextSpan textSpan in ust.TextSpans)
                {
                    if (textSpan.File != null && !localSourceFiles.Contains(textSpan.File))
                    {
                        localSourceFiles.Add(textSpan.File);
                    }
                }
            });

            var textSpanFormatter = formatterResolver.GetFormatter<TextSpan>() as TextSpanFormatter;

            if (textSpanFormatter == null)
            {
                throw new InvalidOperationException($"{nameof(TextSpan)} formatter should be {nameof(TextSpanFormatter)}");
            }

            textSpanFormatter.LocalSourceFiles = localSourceFiles.ToArray();
            newOffset += MessagePackBinary.WriteBoolean(ref bytes, offset, textSpanFormatter.IsLineColumn);

            var languageFormatter = formatterResolver.GetFormatter<Language>();
            newOffset += languageFormatter.Serialize(ref bytes, newOffset, value.Language, formatterResolver);

            newOffset += WriteArray(ref bytes, formatterResolver, newOffset, localSourceFiles);

            newOffset += WriteArray(ref bytes, formatterResolver, newOffset, value.Nodes);

            newOffset += WriteArray(ref bytes, formatterResolver, newOffset, value.Comments);

            newOffset += MessagePackBinary.WriteInt32(ref bytes, newOffset, value.LineOffset);

            newOffset += textSpanFormatter.Serialize(ref bytes, newOffset, value.TextSpan, formatterResolver);

            newOffset += MessagePackBinary.WriteInt32(ref bytes, newOffset, value.Key);

            return newOffset - offset;
        }

        public RootUst Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            int newOffset = offset;
            try
            {
                var textSpanFormatter = formatterResolver.GetFormatter<TextSpan>() as TextSpanFormatter;

                if (textSpanFormatter == null)
                {
                    throw new InvalidOperationException($"{nameof(TextSpan)} formatter should be {nameof(TextSpanFormatter)}");
                }

                textSpanFormatter.IsLineColumn = MessagePackBinary.ReadBoolean(bytes, newOffset, out int size);
                newOffset += size;

                var languageFormatter = formatterResolver.GetFormatter<Language>();
                Language language = languageFormatter.Deserialize(bytes, newOffset, formatterResolver, out size);
                newOffset += size;

                TextFile[] localSourceFiles = ReadArray<TextFile>(bytes, formatterResolver, newOffset, out size);
                newOffset += size;

                textSpanFormatter.LocalSourceFiles = localSourceFiles;

                lock (SourceFiles)
                {
                    foreach (TextFile localSourceFile in localSourceFiles)
                    {
                        SourceFiles.Add(localSourceFile);
                    }
                }

                var nodes = ReadArray<Ust>(bytes, formatterResolver, newOffset, out size);
                newOffset += size;

                var comments = ReadArray<CommentLiteral>(bytes, formatterResolver, newOffset, out size);
                newOffset += size;

                int lineOffset = MessagePackBinary.ReadInt32(bytes, newOffset, out size);
                newOffset += size;

                TextSpan textSpan =
                    textSpanFormatter.Deserialize(bytes, newOffset, formatterResolver, out size);
                newOffset += size;

                int key = MessagePackBinary.ReadInt32(bytes, newOffset, out size);
                newOffset += size;

                CurrentRoot = new RootUst(localSourceFiles[0], language, textSpan)
                {
                    Key = key,
                    Nodes = nodes,
                    Comments = comments,
                    LineOffset = lineOffset
                };

                readSize = newOffset - offset;

                return CurrentRoot;
            }
            catch (InvalidOperationException ex) // Catch incorrect format exceptions
            {
                throw new ReadException(SerializedFile, ex, $"Error during reading {nameof(RootUst)} at {newOffset} offset; Message: {ex.Message}");
            }
        }

        private static int WriteArray<T>(ref byte[] bytes, IFormatterResolver formatterResolver, int offset,
            IList<T> collection)
        {
            int newOffset = offset;
            newOffset += MessagePackBinary.WriteArrayHeader(ref bytes, newOffset, collection.Count);
            var formatter = formatterResolver.GetFormatter<T>();
            foreach (T item in collection)
            {
                newOffset += formatter.Serialize(ref bytes, newOffset, item, formatterResolver);
            }

            return newOffset - offset;
        }

        private static T[] ReadArray<T>(byte[] bytes, IFormatterResolver formatterResolver, int offset, out int size)
        {
            int newOffset = offset;
            int arrayLength = MessagePackBinary.ReadArrayHeader(bytes, newOffset, out int size2);
            newOffset += size2;
            var formatter = formatterResolver.GetFormatter<T>();
            var array = new T[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                array[i] = formatter.Deserialize(bytes, newOffset, formatterResolver, out size2);
                newOffset += size2;
            }

            size = newOffset - offset;
            return array;
        }
    }
}