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

namespace PT.PM.Common.MessagePack
{
    public class RootUstMessagePackSerializer
    {
        [ThreadStatic]
        private static byte[] buffer;

        private BinaryFile serializedFile;
        private TextSpanSerializer textSpanSerializer;
        private FileSerializer fileSerializer;

        private readonly Stack<RootUst> rootAncestors = new Stack<RootUst>();
        private readonly Stack<Ust> ancestors = new Stack<Ust>();

        public RootUst CurrentRoot { get; internal set; }

        public static byte[] Serialize(RootUst rootUst, bool isLineColumn, bool compress, ILogger logger)
        {
            byte[] result = Serialize(rootUst, isLineColumn, compress, logger, out int writeSize);

            if (compress)
            {
                return result;
            }

            return MessagePackBinary.FastCloneWithResize(result, writeSize);
        }

        public static byte[] Serialize(RootUst rootUst, bool isLineColumn, bool compress, ILogger logger,
            out int writeSize)
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

            if (compress)
            {
                var result = LZ4MessagePackSerializer.ToLZ4Binary(new ArraySegment<byte>(buffer, 0, writeSize));
                return result;
            }

            return buffer;
        }

        public static RootUst Deserialize(BinaryFile serializedFile,
            HashSet<IFile> sourceFiles, Action<(IFile, TimeSpan)> readSourceFileAction,
            ILogger logger, out int readSize, byte[] data = null)
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

            byte[] data2 = data ?? MessagePackUtils.UnpackDataIfRequired(serializedFile.Data);

            RootUst result = rootUstSerializer.DeserializeRootUst(data2, sourceFiles, 0, out readSize);

            return result;
        }

        private int SerializeRootUst(ref byte[] bytes, int offset, RootUst rootUst)
        {
            int newOffset = offset;

            CurrentRoot = rootUst;

            var localSourceFiles = new List<TextFile> {CurrentRoot.SourceFile};
            rootUst.ApplyActionToDescendantsAndSelf(ust =>
            {
                foreach (TextSpan textSpan in ust.TextSpans)
                {
                    if (textSpan.File != null && !localSourceFiles.Contains(textSpan.File))
                    {
                        localSourceFiles.Add(textSpan.File);
                    }
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

        private RootUst DeserializeRootUst(byte[] bytes, HashSet<IFile> sourceFiles, int offset, out int readSize)
        {
            int newOffset = offset;

            textSpanSerializer.IsLineColumn = MessagePackBinary.ReadBoolean(bytes, newOffset, out int size);
            newOffset += size;

            var localSourceFiles = (TextFile[])DeserializeObject(bytes, newOffset, typeof(TextFile[]), out size);
            newOffset += size;
            textSpanSerializer.LocalSourceFiles = localSourceFiles;

            lock (sourceFiles)
            {
                foreach (TextFile localSourceFile in localSourceFiles)
                {
                    sourceFiles.Add(localSourceFile);
                }
            }

            RootUst rootUst = (RootUst)DeserializeUst(bytes, newOffset, out size);
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

            PropertyInfo[] serializableProperties = value.GetType().GetSerializableProperties(out byte type);

            int newOffset = offset;
            newOffset += MessagePackBinary.WriteByte(ref bytes, newOffset, type);

            foreach (PropertyInfo property in serializableProperties)
            {
                newOffset += SerializeObject(ref bytes, newOffset, property.PropertyType, property.GetValue(value));
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

            if (type.IsEnum)
            {
                return MessagePackBinary.WriteInt32(ref bytes, offset, (int) value);
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

            throw new NotImplementedException($"Serialization of {type.Name} is not supported");
        }

        private Ust DeserializeUst(byte[] bytes, int offset, out int readSize)
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

                PropertyInfo[] serializableProperties = ust.GetType().GetSerializableProperties(out _);
                foreach (PropertyInfo property in serializableProperties)
                {
                    object obj = DeserializeObject(bytes, newOffset, property.PropertyType, out size);
                    newOffset += size;
                    property.SetValue(ust, obj);
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

        private object DeserializeObject(byte[] bytes, int offset, Type type, out int readSize)
        {
            try
            {
                if (MessagePackBinary.IsNil(bytes, offset))
                {
                    MessagePackBinary.ReadNil(bytes, offset, out readSize);
                    return null;
                }

                if (type == typeof(Ust) || type.IsSubclassOf(typeof(Ust)))
                {
                    return DeserializeUst(bytes, offset, out readSize);
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
                            result[i] = DeserializeObject(bytes, newOffset, itemType, out size);
                            newOffset += size;
                        }
                    }
                    else
                    {
                        itemType = type.GetGenericArguments()[0];

                        for (int i = 0; i < arrayLength; i++)
                        {
                            result.Add(DeserializeObject(bytes, newOffset, itemType, out size));
                            newOffset += size;
                        }
                    }

                    readSize = newOffset - offset;
                    return result;
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