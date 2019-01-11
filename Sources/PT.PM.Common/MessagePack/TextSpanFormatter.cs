using System;
using MessagePack;
using MessagePack.Formatters;
using PT.PM.Common.Exceptions;
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
            int newOffset = offset;
            if (!IsLineColumn)
            {
                newOffset += MessagePackBinary.WriteInt32(ref bytes, newOffset, value.Start);
                newOffset += MessagePackBinary.WriteInt32(ref bytes, newOffset, value.Length);
            }
            else
            {
                TextFile sourceFile = value.GetSourceFile(SourceFile);
                LineColumnTextSpan lcTextSpan = sourceFile.GetLineColumnTextSpan(value);

                newOffset += MessagePackBinary.WriteInt32(ref bytes, newOffset, lcTextSpan.BeginLine);
                newOffset += MessagePackBinary.WriteInt32(ref bytes, newOffset, lcTextSpan.BeginColumn);
                newOffset += MessagePackBinary.WriteInt32(ref bytes, newOffset, lcTextSpan.EndLine);
                newOffset += MessagePackBinary.WriteInt32(ref bytes, newOffset, lcTextSpan.EndColumn);
            }

            var fileFormatter = formatterResolver.GetFormatter<TextFile>();
            newOffset += fileFormatter.Serialize(ref bytes, newOffset, value.File, formatterResolver);

            return newOffset - offset;
        }

        public TextSpan Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            int newOffset = offset;
            try
            {
                int size;
                int start, length;
                TextFile sourceFile;
                var sourceFileFormatter = formatterResolver.GetFormatter<TextFile>();

                if (!IsLineColumn)
                {
                    start = MessagePackBinary.ReadInt32(bytes, newOffset, out size);
                    newOffset += size;
                    length = MessagePackBinary.ReadInt32(bytes, newOffset, out size);
                    newOffset += size;

                    sourceFile = sourceFileFormatter.Deserialize(bytes, newOffset, formatterResolver, out size)
                                 ?? SourceFile;
                }
                else
                {
                    int beginLine = MessagePackBinary.ReadInt32(bytes, newOffset, out size);
                    newOffset += size;
                    int beginColumn = MessagePackBinary.ReadInt32(bytes, newOffset, out size);
                    newOffset += size;
                    int endLine = MessagePackBinary.ReadInt32(bytes, newOffset, out size);
                    newOffset += size;
                    int endColumn = MessagePackBinary.ReadInt32(bytes, newOffset, out size);
                    newOffset += size;

                    sourceFile = sourceFileFormatter.Deserialize(bytes, newOffset, formatterResolver, out size)
                                 ?? SourceFile;

                    try
                    {
                        start = sourceFile.GetLinearFromLineColumn(beginLine, beginColumn);
                        length = sourceFile.GetLinearFromLineColumn(endLine, endColumn) - start;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(new ReadException(SerializedFile, ex, $"Error during linear to line-column TextSpan conversion; Message: {ex.Message}"));
                        start = 0;
                        length = 0;
                    }
                }

                newOffset += size;
                readSize = newOffset - offset;

                return new TextSpan(start, length, sourceFile == SourceFile ? null : sourceFile);
            }
            catch (InvalidOperationException ex) // Catch incorrect format exceptions
            {
                throw new ReadException(SerializedFile, ex, $"Error during reading {nameof(TextSpan)} at {newOffset} offset; Message: {ex.Message}");
            }
        }
    }
}