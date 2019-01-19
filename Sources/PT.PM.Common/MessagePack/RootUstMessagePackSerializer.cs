using MessagePack.Formatters;
using MessagePack;
using System;
using System.Collections.Generic;
using MessagePack.Resolvers;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes;

namespace PT.PM.Common.MessagePack
{
    public class RootUstMessagePackSerializer : IFormatterResolver
    {
        [ThreadStatic]
        private static byte[] buffer;

        private TextSpanFormatter textSpanFormatter;
        private FileFormatter fileFormatter;
        private RootUstFormatter rootUstFormatter;

        public static byte[] Serialize(RootUst rootUst, bool isLineColumn, ILogger logger)
        {
            byte[] result = Serialize(rootUst, isLineColumn, logger, out int writeSize);
            return MessagePackBinary.FastCloneWithResize(result, writeSize);
        }

        protected static byte[] Serialize(RootUst rootUst, bool isLineColumn, ILogger logger, out int writeSize)
        {
            var textSpanFormatter = TextSpanFormatter.CreateWriter();
            textSpanFormatter.IsLineColumn = isLineColumn;
            textSpanFormatter.Logger = logger;

            var sourceFileFormatter = FileFormatter.CreateWriter();
            sourceFileFormatter.Logger = logger;

            var rootUstFormatter = RootUstFormatter.CreateWriter();

            var writeResolver = new RootUstMessagePackSerializer
            {
                textSpanFormatter = textSpanFormatter,
                fileFormatter = sourceFileFormatter,
                rootUstFormatter = rootUstFormatter
            };

            if (buffer == null)
            {
                buffer = new byte[ushort.MaxValue + 1];
            }

            writeSize = rootUstFormatter.Serialize(ref buffer, 0, rootUst, writeResolver);

            return buffer;
        }

        public static RootUst Deserialize(BinaryFile serializedFile, bool isLineColumn,
            HashSet<IFile> sourceFiles, Action<(IFile, TimeSpan)> readSourceFileAction, ILogger logger,
            out int readSize)
        {
            var textSpanFormatter = TextSpanFormatter.CreateReader(serializedFile);
            textSpanFormatter.IsLineColumn = isLineColumn;
            textSpanFormatter.Logger = logger;

            var sourceFileFormatter = FileFormatter.CreateReader(serializedFile, sourceFiles, readSourceFileAction);
            sourceFileFormatter.Logger = logger;

            var rootUstFormatter = RootUstFormatter.CreateReader(serializedFile, sourceFiles);

            var readerResolver =  new RootUstMessagePackSerializer
            {
                textSpanFormatter = textSpanFormatter,
                fileFormatter = sourceFileFormatter,
                rootUstFormatter = rootUstFormatter
            };

            RootUst result = readerResolver.GetFormatterWithVerify<RootUst>()
                .Deserialize(serializedFile.Data, 0, readerResolver, out readSize);
            result.FillAscendants();

            return result;
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            Type type = typeof(T);

            if (type == typeof(TextSpan))
            {
                return (IMessagePackFormatter<T>) textSpanFormatter;
            }

            if (type == typeof(TextFile))
            {
                return (IMessagePackFormatter<T>) fileFormatter;
            }

            if (type == typeof(RootUst))
            {
                return (IMessagePackFormatter<T>) rootUstFormatter;
            }

            return StandardResolver.Instance.GetFormatter<T>();
        }

        protected RootUstMessagePackSerializer() {}
    }
}