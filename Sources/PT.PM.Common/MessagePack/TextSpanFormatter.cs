using System;
using MessagePack;
using MessagePack.Formatters;
using PT.PM.Common.Files;

namespace PT.PM.Common.MessagePack
{
    public class TextSpanFormatter : IMessagePackFormatter<TextSpan>, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool IsLineColumn { get; set; }

        public TextFile SourceFile { get; internal set; }

        public BinaryFile SerializedFile { get; private set; }

        public static TextSpanFormatter CreateWriter(TextFile sourceFile)
        {
            if (sourceFile == null)
            {
                throw new ArgumentNullException(nameof(sourceFile));
            }

            return new TextSpanFormatter
            {
                SourceFile = sourceFile
            };
        }

        public static TextSpanFormatter CreateReader(BinaryFile serializedFile)
        {
            if (serializedFile == null)
            {
                throw new ArgumentNullException(nameof(serializedFile));
            }

            return new TextSpanFormatter
            {
                SerializedFile = serializedFile
            };
        }

        private TextSpanFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, TextSpan value, IFormatterResolver formatterResolver)
        {
            int writeSize = 0;

            if (!IsLineColumn)
            {
                writeSize += MessagePackBinary.WriteInt32(ref bytes, offset, value.Start);
                writeSize += MessagePackBinary.WriteInt32(ref bytes, offset + writeSize, value.Length);
            }
            else
            {
                TextFile sourceFile = value.GetSourceFile(SourceFile);
                LineColumnTextSpan lcTextSpan;

                if (sourceFile != null)
                {
                    lcTextSpan = sourceFile.GetLineColumnTextSpan(value);
                }
                else
                {
                    lcTextSpan = LineColumnTextSpan.Zero;
                    // TODO: Log error
                }

                writeSize += MessagePackBinary.WriteInt32(ref bytes, offset, lcTextSpan.BeginLine);
                writeSize += MessagePackBinary.WriteInt32(ref bytes, offset + writeSize, lcTextSpan.BeginColumn);
                writeSize += MessagePackBinary.WriteInt32(ref bytes, offset + writeSize, lcTextSpan.EndLine);
                writeSize += MessagePackBinary.WriteInt32(ref bytes, offset + writeSize, lcTextSpan.EndColumn);
            }

            var fileFormatter = formatterResolver.GetFormatter<TextFile>();
            writeSize += fileFormatter.Serialize(ref bytes, offset + writeSize, value.File, formatterResolver);

            return writeSize;
        }

        public TextSpan Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            readSize = 0;
            TextSpan result = TextSpan.Zero;

            int size;
            int start, length;
            TextFile sourceFile;
            var sourceFileFormatter = formatterResolver.GetFormatter<TextFile>();

            if (!IsLineColumn)
            {
                start = MessagePackBinary.ReadInt32(bytes, offset, out size);
                readSize += size;
                length = MessagePackBinary.ReadInt32(bytes, offset + readSize, out size);
                readSize += size;

                sourceFile = sourceFileFormatter.Deserialize(bytes, offset + readSize, formatterResolver, out size)
                           ?? SourceFile;
            }
            else
            {
                int beginLine = MessagePackBinary.ReadInt32(bytes, offset, out size);
                readSize += size;
                int beginColumn = MessagePackBinary.ReadInt32(bytes, offset + readSize, out size);
                readSize += size;
                int endLine = MessagePackBinary.ReadInt32(bytes, offset + readSize, out size);
                readSize += size;
                int endColumn = MessagePackBinary.ReadInt32(bytes, offset + readSize, out size);
                readSize += size;

                sourceFile = sourceFileFormatter.Deserialize(bytes, offset + readSize, formatterResolver, out size)
                           ?? SourceFile;

                start = sourceFile.GetLinearFromLineColumn(beginLine, beginColumn);
                length = sourceFile.GetLinearFromLineColumn(endLine, endColumn) - start;
            }

            readSize += size;
            result = new TextSpan(start, length, sourceFile == SourceFile ? null : sourceFile);

            return result;
        }
    }
}