using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MessagePack;
using MessagePack.Formatters;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Files;
using PT.PM.Common.Utils;
using static MessagePack.MessagePackBinary;

namespace PT.PM.Common.MessagePack
{
    public class FileFormatter : IMessagePackFormatter<IFile>, IMessagePackFormatter<TextFile>, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public BinaryFile SerializedFile { get; private set; }

        public HashSet<IFile> SourceFiles { get; private set; }

        public Action<(IFile, TimeSpan)> ReadSourceFileAction { get; private set; }

        public static FileFormatter CreateWriter()
        {
            return new FileFormatter();
        }

        public static FileFormatter CreateReader(BinaryFile serializedFile, HashSet<IFile> sourceFiles, Action<(IFile, TimeSpan)> readSourceFileAction)
        {
            if (serializedFile == null)
            {
                throw new ArgumentNullException(nameof(serializedFile));
            }

            return new FileFormatter
            {
                SerializedFile = serializedFile,
                SourceFiles = sourceFiles,
                ReadSourceFileAction = readSourceFileAction
            };
        }

        private FileFormatter()
        {
        }
        
        public int Serialize(ref byte[] bytes, int offset, TextFile value, IFormatterResolver formatterResolver)
        {
            return Serialize(ref bytes, offset, (IFile)value, formatterResolver);
        }

        public int Serialize(ref byte[] bytes, int offset, IFile value, IFormatterResolver formatterResolver)
        {
            if (value is null)
            {
                return WriteNil(ref bytes, offset);
            }

            int newOffset = offset;
            newOffset += WriteByte(ref bytes, newOffset, (byte) value.Type);
            newOffset += WriteString(ref bytes, newOffset, value.RootPath);
            newOffset += WriteString(ref bytes, newOffset, value.RelativePath);
            newOffset += WriteString(ref bytes, newOffset, value.Name);
            newOffset += WriteString(ref bytes, newOffset, value.PatternKey);

            if (string.IsNullOrEmpty(value.FullName))
            {
                if (value is TextFile sourceFile)
                {
                    newOffset += WriteString(ref bytes, newOffset, sourceFile.Data);
                }
                else if (value is BinaryFile binaryFile)
                {
                    newOffset += WriteBytes(ref bytes, newOffset, binaryFile.Data);
                }
            }

            return newOffset - offset;
        }

        TextFile IMessagePackFormatter<TextFile>.Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return (TextFile)((IMessagePackFormatter<IFile>)this).Deserialize(bytes, offset, formatterResolver, out readSize);
        }

        IFile IMessagePackFormatter<IFile>.Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            int newOffset = offset;
            try
            {
                if (IsNil(bytes, offset))
                {
                    ReadNil(bytes, offset, out readSize);
                    return null;
                }

                FileType fileType = (FileType) ReadByte(bytes, offset, out int size);
                newOffset += size;
                
                string rootPath = ReadString(bytes, newOffset, out size) ?? "";
                rootPath = rootPath.NormalizeDirSeparator();
                newOffset += size;
                
                string relativePath = ReadString(bytes, newOffset, out size) ?? "";
                relativePath = relativePath.NormalizeDirSeparator();
                newOffset += size;
                
                string name = ReadString(bytes, newOffset, out size) ?? "";
                newOffset += size;
                
                string patternKey = ReadString(bytes, newOffset, out size);
                newOffset += size;

                string fullName = Path.Combine(rootPath, relativePath, name);
                IFile result;

                if (SourceFiles != null && !string.IsNullOrEmpty(fullName))
                {
                    lock (SourceFiles)
                    {
                        foreach (IFile sourceFile in SourceFiles)
                        {
                            if (sourceFile.RelativePath == relativePath && sourceFile.Name == name)
                            {
                                readSize = newOffset - offset;
                                return sourceFile;
                            }
                        }
                    }
                }

                var stopwatch = Stopwatch.StartNew();
                if (fileType == FileType.TextFile)
                {
                    string code;
                    if (string.IsNullOrEmpty(fullName))
                    {
                        code = ReadString(bytes, newOffset, out size);
                        newOffset += size;
                    }
                    else
                    {
                        try
                        {
                            code = File.ReadAllText(fullName);
                        }
                        catch (Exception ex)
                        {
                            code = "";
                            Logger.LogError(new ReadException(SerializedFile, ex,
                                $"Error during opening the file {fullName}"));
                        }
                    }

                    result = new TextFile(code);
                }
                else
                {
                    byte[] data;
                    if (string.IsNullOrEmpty(fullName))
                    {
                        data = ReadBytes(bytes, newOffset, out size);
                        newOffset += size;
                    }
                    else
                    {
                        try
                        {
                            data = File.ReadAllBytes(fullName);
                        }
                        catch (Exception ex)
                        {
                            data = new byte[0];
                            Logger.LogError(new ReadException(SerializedFile, ex,
                                $"Error during opening the file {fullName}"));
                        }
                    }

                    result = new BinaryFile(data);
                }

                stopwatch.Stop();

                result.RootPath = rootPath;
                result.RelativePath = relativePath;
                result.Name = name;
                result.PatternKey = patternKey;

                if (SourceFiles != null)
                {
                    lock (SourceFiles)
                    {
                        SourceFiles.Add(result);
                    }
                }

                ReadSourceFileAction?.Invoke((result, stopwatch.Elapsed));

                readSize = newOffset - offset;

                return result;
            }
            catch (InvalidOperationException ex) // Catch incorrect format exceptions
            {
                throw new ReadException(SerializedFile, ex, $"Error during reading {nameof(IFile)} or {nameof(TextFile)} at {newOffset} offset; Message: {ex.Message}");
            }
        }
    }
}