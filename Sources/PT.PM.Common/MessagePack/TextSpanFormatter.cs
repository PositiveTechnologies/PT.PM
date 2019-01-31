using System;
using MessagePack;
using MessagePack.Formatters;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Files;
using static MessagePack.MessagePackBinary;

namespace PT.PM.Common.MessagePack
{
    public class TextSpanFormatter : IMessagePackFormatter<TextSpan>, ILoggable
    {
        private TextFile[] localSourceFiles;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool IsLineColumn { get; set; }

        public TextFile[] LocalSourceFiles
        {
            get => localSourceFiles;
            internal set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(LocalSourceFiles));
                }

                if (value.Length == 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(LocalSourceFiles));
                }

                localSourceFiles = value;
            }
        }

        public BinaryFile SerializedFile { get; private set; }

        public static TextSpanFormatter CreateWriter()
        {
            return new TextSpanFormatter();
        }

        public static TextSpanFormatter CreateReader(BinaryFile serializedFile)
        {
            return new TextSpanFormatter
            {
                SerializedFile = serializedFile ?? throw new ArgumentNullException(nameof(serializedFile))
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
                newOffset += WriteInt32(ref bytes, newOffset, value.Start);
                newOffset += WriteInt32(ref bytes, newOffset, value.Length);
            }
            else
            {
                TextFile sourceFile = value.GetSourceFile(LocalSourceFiles[0]);
                LineColumnTextSpan lcTextSpan = sourceFile.GetLineColumnTextSpan(value);

                newOffset += WriteInt32(ref bytes, newOffset, lcTextSpan.BeginLine);
                newOffset += WriteInt32(ref bytes, newOffset, lcTextSpan.BeginColumn);
                newOffset += WriteInt32(ref bytes, newOffset, lcTextSpan.EndLine);
                newOffset += WriteInt32(ref bytes, newOffset, lcTextSpan.EndColumn);
            }

            int fileIndex = value.File == null ? 0 : Array.IndexOf(LocalSourceFiles, value.File);

            if (fileIndex == -1)
            {
                Logger.LogError(new ReadException(SerializedFile, message:
                    $"{nameof(TextSpan.File)} of {nameof(TextSpan)} {value} is not correctly mapped in {nameof(RootUstFormatter)}"));
            }

            newOffset += WriteInt32(ref bytes, newOffset, fileIndex);

            return newOffset - offset;
        }

        public TextSpan Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            int newOffset = offset;
            try
            {
                int size;
                int start = 0, length = 0;
                TextFile sourceFile = null;

                if (!IsLineColumn)
                {
                    start = ReadInt32(bytes, newOffset, out size);
                    newOffset += size;
                    length = ReadInt32(bytes, newOffset, out size);
                    newOffset += size;

                    int fileIndex = ReadInt32(bytes, newOffset, out size);
                    newOffset += size;

                    if (fileIndex < 0 || fileIndex >= LocalSourceFiles.Length)
                    {
                        Logger.LogError(new ReadException(SerializedFile,
                            message: $"Incorrect file index {fileIndex} for {nameof(TextSpan)} [{start}..{length}) at {newOffset} offset"));
                    }
                    else
                    {
                        sourceFile = LocalSourceFiles[fileIndex];
                    }
                }
                else
                {
                    int beginLine = ReadInt32(bytes, newOffset, out size);
                    newOffset += size;
                    int beginColumn = ReadInt32(bytes, newOffset, out size);
                    newOffset += size;
                    int endLine = ReadInt32(bytes, newOffset, out size);
                    newOffset += size;
                    int endColumn = ReadInt32(bytes, newOffset, out size);
                    newOffset += size;

                    int fileIndex = ReadInt32(bytes, newOffset, out size);
                    newOffset += size;

                    if (fileIndex < 0 || fileIndex >= LocalSourceFiles.Length)
                    {
                        Logger.LogError(new ReadException(SerializedFile,
                            message: $"Incorrect file index {fileIndex} for {nameof(TextSpan)} [{beginLine},{beginColumn}..{endLine},{endColumn}) at {newOffset} offset"));
                    }
                    else
                    {
                        sourceFile = LocalSourceFiles[fileIndex];

                        try
                        {
                            start = sourceFile.GetLinearFromLineColumn(beginLine, beginColumn);
                            length = sourceFile.GetLinearFromLineColumn(endLine, endColumn) - start;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(new ReadException(SerializedFile, ex,
                                $"Error during linear to line-column {nameof(LineColumnTextSpan)} [{beginLine},{beginColumn}..{endLine},{endColumn}) conversion; Message: {ex.Message}"));
                        }
                    }
                }

                readSize = newOffset - offset;

                return new TextSpan(start, length, sourceFile == LocalSourceFiles[0] ? null : sourceFile);;
            }
            catch (InvalidOperationException ex) // Catch incorrect format exceptions
            {
                throw new ReadException(SerializedFile, ex, $"Error during reading {nameof(TextSpan)} at {newOffset} offset; Message: {ex.Message}");
            }
        }
    }
}