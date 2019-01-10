using System;
using System.Threading;
using MessagePack;
using MessagePack.Formatters;
using PT.PM.Common.Files;

namespace PT.PM.Common.MessagePack
{
    public class TextSpanFormatter : IMessagePackFormatter<TextSpan>, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool IsLineColumn { get; set; }

        public CodeFile CodeFile { get; internal set; }

        public BinaryFile SerializedFile { get; private set; }

        public static TextSpanFormatter CreateWriter(CodeFile codeFile)
        {
            if (codeFile == null)
            {
                throw new ArgumentNullException(nameof(codeFile));
            }

            return new TextSpanFormatter
            {
                CodeFile = codeFile
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
                CodeFile codeFile = value.GetCodeFile(CodeFile);
                LineColumnTextSpan lcTextSpan;

                if (codeFile != null)
                {
                    lcTextSpan = codeFile.GetLineColumnTextSpan(value);
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

            var fileFormatter = formatterResolver.GetFormatter<CodeFile>();
            writeSize += fileFormatter.Serialize(ref bytes, offset + writeSize, value.CodeFile, formatterResolver);

            return writeSize;
        }

        public TextSpan Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            readSize = 0;
            TextSpan result = TextSpan.Zero;

            int size;
            int start, length;
            CodeFile codeFile;
            var codeFileFormatter = formatterResolver.GetFormatter<CodeFile>();

            if (!IsLineColumn)
            {
                start = MessagePackBinary.ReadInt32(bytes, offset, out size);
                readSize += size;
                length = MessagePackBinary.ReadInt32(bytes, offset + readSize, out size);
                readSize += size;

                codeFile = codeFileFormatter.Deserialize(bytes, offset + readSize, formatterResolver, out size)
                           ?? CodeFile;
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

                codeFile = codeFileFormatter.Deserialize(bytes, offset + readSize, formatterResolver, out size)
                           ?? CodeFile;

                start = codeFile.GetLinearFromLineColumn(beginLine, beginColumn);
                length = codeFile.GetLinearFromLineColumn(endLine, endColumn) - start;
            }

            readSize += size;
            result = new TextSpan(start, length, codeFile == CodeFile ? null : codeFile);

            return result;
        }
    }
}