using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MessagePack;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes;
using PT.PM.Common.Reflection;
using PT.PM.Common.Utils;

namespace PT.PM.Common.MessagePack
{
    public class RootUstMessagePackSerializer
    {
        [ThreadStatic]
        private static byte[] buffer;

        private BinaryFile serializedFile;
        private TextSpanSerializer textSpanSerializer;
        private FileSerializer fileSerializer;

        // One file can contain several RootUst nodes. For example, the file with PHP inside JavaScript inside PHP
        // Deserialization is being performed in top-down manner, that's why it's possible to fill roots and parents
        // right during deserialization.
        private readonly Stack<RootUst> rootAncestors = new Stack<RootUst>();
        private readonly Stack<Ust> ancestors = new Stack<Ust>();

        public RootUst CurrentRoot { get; internal set; }

        public static byte[] Serialize(RootUst rootUst, bool isLineColumn, ILogger logger)
        {
            byte[] result = Serialize(rootUst, isLineColumn, logger, out int writeSize);
            return MessagePackBinary.FastCloneWithResize(result, writeSize);
        }

        public static byte[] Serialize(RootUst rootUst, bool isLineColumn, ILogger logger, out int writeSize)
        {
            var textSpanSerializer = TextSpanSerializer.CreateWriter();
            textSpanSerializer.IsLineColumn = isLineColumn;
            textSpanSerializer.Logger = logger;

            var sourceFileSerializer = FileSerializer.CreateWriter();
            sourceFileSerializer.Logger = logger;

            var rootUstSerializer = new RootUstMessagePackSerializer
            {
                textSpanSerializer = textSpanSerializer,
                fileSerializer = sourceFileSerializer
            };

            if (buffer == null)
            {
                buffer = new byte[ushort.MaxValue + 1];
            }

            writeSize = rootUstSerializer.SerializeRootUst(ref buffer, 0, rootUst);
            return buffer;
        }

        public static RootUst Deserialize(BinaryFile serializedFile,
            HashSet<IFile> sourceFiles, Action<(IFile, TimeSpan)> readSourceFileAction,
            ILogger logger, out int readSize)
        {
            var textSpanSerializer = TextSpanSerializer.CreateReader(serializedFile);
            textSpanSerializer.Logger = logger;

            var sourceFileSerializer = FileSerializer.CreateReader(serializedFile, sourceFiles, readSourceFileAction);
            sourceFileSerializer.Logger = logger;

            var rootUstSerializer =  new RootUstMessagePackSerializer
            {
                serializedFile = serializedFile,
                textSpanSerializer = textSpanSerializer,
                fileSerializer = sourceFileSerializer
            };

            RootUst result = rootUstSerializer.DeserializeRootUst(serializedFile.Data, sourceFiles, 0, out readSize, logger);

            return result;
        }

        private int SerializeRootUst(ref byte[] bytes, int offset, RootUst rootUst)
        {
            int newOffset = offset;

            CurrentRoot = rootUst;

            var localSourceFiles = new List<TextFile> {CurrentRoot.SourceFile};
            rootUst.ApplyActionToDescendantsAndSelf(ust =>
            {
                TextSpan textSpan = ust.TextSpan;
                if (textSpan.File != null && !localSourceFiles.Contains(textSpan.File))
                {
                    localSourceFiles.Add(textSpan.File);
                }
            });

            textSpanSerializer.LocalSourceFiles = localSourceFiles.ToArray();

            //int sizeOffset = newOffset;
            //newOffset += 5;

            newOffset += MessagePackBinary.WriteBoolean(ref bytes, newOffset, textSpanSerializer.IsLineColumn);

            newOffset += SerializeObject(ref bytes, newOffset, typeof(List<TextFile>), localSourceFiles);

            newOffset += SerializeUst(ref bytes, newOffset, rootUst);

            int writeSize = newOffset - offset;
            //MessagePackBinary.WriteInt32ForceInt32Block(ref bytes, sizeOffset, writeSize - 5); TODO implement later

            return writeSize;
        }

        private RootUst DeserializeRootUst(byte[] bytes, HashSet<IFile> sourceFiles, int offset, out int readSize, ILogger logger)
        {
            int newOffset = offset;

            textSpanSerializer.IsLineColumn = MessagePackBinary.ReadBoolean(bytes, newOffset, out int size);
            newOffset += size;

            var localSourceFiles = (TextFile[])DeserializeObject(bytes, newOffset, typeof(TextFile[]), out size, logger);
            newOffset += size;
            textSpanSerializer.LocalSourceFiles = localSourceFiles;

            lock (sourceFiles)
            {
                foreach (TextFile localSourceFile in localSourceFiles)
                {
                    sourceFiles.Add(localSourceFile);
                }
            }

            RootUst rootUst = (RootUst)DeserializeUst(bytes, newOffset, out size, logger);
            rootUst.SourceFile = localSourceFiles[0];
            newOffset += size;

            readSize = newOffset - offset;

            return rootUst;
        }

        private int SerializeUst(ref byte[] bytes, int offset, Ust value)
        {
            if (value is null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }

            PropertyInfo[] serializableProperties = value.GetType().GetSerializableProperties();

            int newOffset = offset;
            newOffset += MessagePackBinary.WriteByte(ref bytes, newOffset, (byte)value.UstType);

            foreach (PropertyInfo property in serializableProperties)
            {
                object serializeObject = property.GetValue(value);

                newOffset += SerializeObject(ref bytes, newOffset, property.PropertyType, serializeObject);
            }

            return newOffset - offset;
        }

        private int SerializeObject(ref byte[] bytes, int offset, Type type, object value)
        {
            if (value is null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }

            if (type == typeof(Ust) || type.IsSubclassOf(typeof(Ust)))
            {
                return SerializeUst(ref bytes, offset, (Ust) value);
            }
            if (type == typeof(string))
            {
                return MessagePackBinary.WriteString(ref bytes, offset, (string) value);
            }

            if (type == typeof(int))
            {
                return MessagePackBinary.WriteInt32(ref bytes, offset, (int) value);
            }

            if (type == typeof(long))
            {
                return MessagePackBinary.WriteInt64(ref bytes, offset, (long) value);
            }

            if (type == typeof(bool))
            {
                return MessagePackBinary.WriteBoolean(ref bytes, offset, (bool) value);
            }

            if (type == typeof(TextSpan))
            {
                return textSpanSerializer.Serialize(ref bytes, offset, (TextSpan) value);
            }

            if (type == typeof(double))
            {
                return MessagePackBinary.WriteDouble(ref bytes, offset, (double) value);
            }

            if (type.IsEnum)
            {
                return MessagePackBinary.WriteInt32(ref bytes, offset, (int) value);
            }

            Type[] typeInterfaces = type.GetInterfaces();

            if (typeInterfaces.Contains(typeof(IFile)))
            {
                return fileSerializer.Serialize(ref bytes, offset, (IFile) value);
            }

            if (typeInterfaces.Contains(typeof(IList)))
            {
                int newOffset = offset;
                IList collection = (IList) value;
                newOffset += MessagePackBinary.WriteArrayHeader(ref bytes, newOffset, collection.Count);
                foreach (object item in collection)
                {
                    newOffset += SerializeObject(ref bytes, newOffset, item?.GetType(), item);
                }

                return newOffset - offset;
            }

            if (type == typeof(char))
            {
                return MessagePackBinary.WriteChar(ref bytes, offset, (char) value);
            }

            if (type == typeof(byte))
            {
                return MessagePackBinary.WriteByte(ref bytes, offset, (byte) value);
            }

            if (type == typeof(sbyte))
            {
                return MessagePackBinary.WriteSByte(ref bytes, offset, (sbyte) value);
            }

            if (type == typeof(short))
            {
                return MessagePackBinary.WriteInt16(ref bytes, offset, (short) value);
            }

            if (type == typeof(ushort))
            {
                return MessagePackBinary.WriteUInt16(ref bytes, offset, (ushort) value);
            }

            if (type == typeof(uint))
            {
                return MessagePackBinary.WriteUInt32(ref bytes, offset, (uint) value);
            }

            if (type == typeof(ulong))
            {
                return MessagePackBinary.WriteUInt64(ref bytes, offset, (ulong) value);
            }

            if (type == typeof(float))
            {
                return MessagePackBinary.WriteSingle(ref bytes, offset, (float) value);
            }

            if (type == typeof(DateTime))
            {
                return MessagePackBinary.WriteDateTime(ref bytes, offset, (DateTime) value);
            }

            throw new NotImplementedException($"Serialization of {type.Name} is not supported");
        }

        private Ust DeserializeUst(byte[] bytes, int offset, out int readSize, ILogger logger)
        {
            try
            {
                if (MessagePackBinary.IsNil(bytes, offset))
                {
                    MessagePackBinary.ReadNil(bytes, offset, out readSize);
                    return null;
                }

                int newOffset = offset;

                byte nodeType = MessagePackBinary.ReadByte(bytes, newOffset, out int size);
                newOffset += size;

                Ust ust;
                try
                {
                    ust = ReflectionCache.CreateUst(nodeType);
                }
                catch (Exception ex)
                {
                    throw new ReadException(serializedFile, ex, $"Invalid ust type {nodeType} at {offset} offset");
                }

                if (rootAncestors.Count > 0)
                {
                    ust.Root = rootAncestors.Peek();
                }

                if (ancestors.Count > 0)
                {
                    ust.Parent = ancestors.Peek();
                }

                var rootUst = ust as RootUst;

                if (rootUst != null)
                {
                    rootAncestors.Push(rootUst);
                }
                ancestors.Push(ust);

                PropertyInfo[] serializableProperties = ust.GetType().GetSerializableProperties();
                foreach (PropertyInfo property in serializableProperties)
                {
                    if (MessagePackBinary.IsNil(bytes, newOffset))
                    {
                        // Optimization: do not fill the property if null is read
                        MessagePackBinary.ReadNil(bytes, newOffset, out size);
                    }
                    else
                    {
                        object obj = DeserializeObject(bytes, newOffset, property.PropertyType, out size, logger);

                        try
                        {
                            property.SetValue(ust, obj);
                        }
                        catch (Exception ex)
                        {
                             logger?.LogError(new ReadException(serializedFile, ex, $"Deserialization error at offset {newOffset}: {ex.Message}"));
                        }
                    }
                    newOffset += size;
                }

                if (rootUst != null)
                {
                    rootAncestors.Pop();
                }
                ancestors.Pop();

                readSize = newOffset - offset;

                return ust;
            }
            catch (InvalidOperationException ex)
            {
                throw new ReadException(serializedFile, ex, $"Error during reading Ust at {offset} offset; Message: {ex.Message}");
            }
        }

        private object DeserializeObject(byte[] bytes, int offset, Type type, out int readSize, ILogger logger)
        {
            try
            {
                if (MessagePackBinary.IsNil(bytes, offset))
                {
                    MessagePackBinary.ReadNil(bytes, offset, out readSize);
                    return type.GetDefaultValue();
                }

                if (type == typeof(Ust) || type.IsSubclassOf(typeof(Ust)))
                {
                    return DeserializeUst(bytes, offset, out readSize, logger);
                }

                if (type == typeof(TextSpan))
                {
                    return textSpanSerializer.Deserialize(bytes, offset, out readSize);
                }

                if (type == typeof(string))
                {
                    return MessagePackBinary.ReadString(bytes, offset, out readSize);
                }

                if (type == typeof(int))
                {
                    return MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                }

                if (type == typeof(long))
                {
                    return MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                }

                if (type == typeof(bool))
                {
                    return MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                }

                if (type == typeof(double))
                {
                    return MessagePackBinary.ReadDouble(bytes, offset, out readSize);
                }

                if (type.IsEnum)
                {
                    return MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                }

                Type[] typeInterfaces = type.GetInterfaces();

                if (typeInterfaces.Contains(typeof(IFile)))
                {
                    return fileSerializer.Deserialize(bytes, offset, out readSize);
                }

                if (type.GetInterfaces().Contains(typeof(IList)))
                {
                    int newOffset = offset;

                    int arrayLength = MessagePackBinary.ReadArrayHeader(bytes, newOffset, out int size);
                    newOffset += size;

                    Type itemType;
                    IList result = (IList) Activator.CreateInstance(type, arrayLength);

                    if (type.IsArray)
                    {
                        itemType = type.GetElementType();

                        for (int i = 0; i < arrayLength; i++)
                        {
                            result[i] = DeserializeObject(bytes, newOffset, itemType, out size, logger);
                            newOffset += size;
                        }
                    }
                    else
                    {
                        itemType = type.GetGenericArguments()[0];

                        for (int i = 0; i < arrayLength; i++)
                        {
                            result.Add(DeserializeObject(bytes, newOffset, itemType, out size, logger));
                            newOffset += size;
                        }
                    }

                    readSize = newOffset - offset;
                    return result;
                }

                if (type == typeof(char))
                {
                    return MessagePackBinary.ReadChar(bytes, offset, out readSize);
                }

                if (type == typeof(byte))
                {
                    return MessagePackBinary.ReadByte(bytes, offset, out readSize);
                }

                if (type == typeof(sbyte))
                {
                    return MessagePackBinary.ReadSByte(bytes, offset, out readSize);
                }

                if (type == typeof(short))
                {
                    return MessagePackBinary.ReadInt16(bytes, offset, out readSize);
                }

                if (type == typeof(ushort))
                {
                    return MessagePackBinary.ReadUInt16(bytes, offset, out readSize);
                }

                if (type == typeof(uint))
                {
                    return MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
                }

                if (type == typeof(ulong))
                {
                    return MessagePackBinary.ReadUInt64(bytes, offset, out readSize);
                }

                if (type == typeof(float))
                {
                    return MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                }

                if (type == typeof(DateTime))
                {
                    return MessagePackBinary.ReadDateTime(bytes, offset, out readSize);
                }

                throw new NotImplementedException($"Deserialization of {type.Name} is not supported");
            }
            catch (InvalidOperationException ex)
            {
                throw new ReadException(serializedFile, ex, $"Error during reading {type.Name} at {offset} offset; Message: {ex.Message}");
            }
        }
    }
}
