using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MessagePack;
using MessagePack.Formatters;
using PT.PM.Common.Files;
using PT.PM.Common.Utils;

namespace PT.PM.Common.MessagePack
{
    public class FileFormatter : IMessagePackFormatter<IFile>, IMessagePackFormatter<CodeFile>, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public HashSet<IFile> SourceFiles { get; private set; }

        public Action<(IFile, TimeSpan)> ReadCodeFileAction { get; private set; }

        public static FileFormatter CreateWriter()
        {
            return new FileFormatter();
        }

        public static FileFormatter CreateReader(HashSet<IFile> sourceFiles, Action<(IFile, TimeSpan)> readCodeFileAction)
        {
            return new FileFormatter
            {
                SourceFiles = sourceFiles,
                ReadCodeFileAction = readCodeFileAction
            };
        }

        private FileFormatter()
        {
        }
        
        public int Serialize(ref byte[] bytes, int offset, CodeFile value, IFormatterResolver formatterResolver)
        {
            return Serialize(ref bytes, offset, (IFile)value, formatterResolver);
        }

        public int Serialize(ref byte[] bytes, int offset, IFile value, IFormatterResolver formatterResolver)
        {
            int writeSize = 0;

            if (value is null)
            {
                writeSize += MessagePackBinary.WriteNil(ref bytes, offset);
            }
            else
            {
                writeSize += MessagePackBinary.WriteByte(ref bytes, offset, (byte) value.Type);
                writeSize += MessagePackBinary.WriteString(ref bytes, offset + writeSize, value.RootPath);
                writeSize += MessagePackBinary.WriteString(ref bytes, offset + writeSize, value.RelativePath);
                writeSize += MessagePackBinary.WriteString(ref bytes, offset + writeSize, value.Name);
                writeSize += MessagePackBinary.WriteString(ref bytes, offset + writeSize, value.PatternKey);

                if (string.IsNullOrEmpty(value.FullName))
                {
                    if (value is CodeFile codeFile)
                    {
                        writeSize += MessagePackBinary.WriteString(ref bytes, offset + writeSize, codeFile.Data);
                    }
                    else if (value is BinaryFile binaryFile)
                    {
                        writeSize += MessagePackBinary.WriteBytes(ref bytes, offset + writeSize, binaryFile.Data);
                    }
                }
            }

            return writeSize;
        }

        CodeFile IMessagePackFormatter<CodeFile>.Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return (CodeFile)((IMessagePackFormatter<IFile>)this).Deserialize(bytes, offset, formatterResolver, out readSize);
        }

        IFile IMessagePackFormatter<IFile>.Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            readSize = 0;

            if (MessagePackBinary.IsNil(bytes, offset))
            {
                MessagePackBinary.ReadNil(bytes, offset, out readSize);
                return null;
            }

            FileType fileType = (FileType)MessagePackBinary.ReadByte(bytes, offset, out int size);
            readSize += size;
            string rootPath = MessagePackBinary.ReadString(bytes, offset + readSize, out size) ?? "";
            rootPath = rootPath.NormalizeDirSeparator();
            readSize += size;
            string relativePath = MessagePackBinary.ReadString(bytes, offset + readSize, out size) ?? "";
            relativePath = relativePath.NormalizeDirSeparator();
            readSize += size;
            string name = MessagePackBinary.ReadString(bytes, offset + readSize, out size) ?? "";
            readSize += size;
            string patternKey = MessagePackBinary.ReadString(bytes, offset + readSize, out size);
            readSize += size;

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
                            return sourceFile;
                        }
                    }
                }
            }
            
            var stopwatch = Stopwatch.StartNew();
            if (fileType == FileType.CodeFile)
            {
                string code;
                if (string.IsNullOrEmpty(fullName))
                {
                    code = MessagePackBinary.ReadString(bytes, offset + readSize, out size);
                    readSize += size;
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
                        Logger.LogError(new FileLoadException($"Error during {fullName} file reading", ex));
                    }
                }
                result = new CodeFile(code);
            }
            else
            {
                byte[] data;
                if (string.IsNullOrEmpty(fullName))
                {
                    data = MessagePackBinary.ReadBytes(bytes, offset + readSize, out size);
                    readSize += size;
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
                        Logger.LogError(new FileLoadException($"Error during {fullName} file reading", ex));
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

            ReadCodeFileAction?.Invoke((result, stopwatch.Elapsed));

            return result;
        }
    }
}