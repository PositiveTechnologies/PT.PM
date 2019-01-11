using MessagePack.Formatters;
using MessagePack;
using System;
using System.Collections.Generic;
using MessagePack.Resolvers;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes;

namespace PT.PM.Common.MessagePack
{
    internal class UstMessagePackResolver : IFormatterResolver
    {
        public TextSpanFormatter TextSpanFormatter { get; private set; }

        public FileFormatter FileFormatter { get; private set; }

        public LanguageFormatter LanguageFormatter { get; } = new LanguageFormatter();

        public RootUstFormatter RootUstFormatter { get; } = new RootUstFormatter();
        
        public static UstMessagePackResolver CreateWriter(TextFile sourceFile, bool isLineColumn, ILogger logger)
        {
            var textSpanFormatter = TextSpanFormatter.CreateWriter(sourceFile);
            textSpanFormatter.IsLineColumn = isLineColumn;
            textSpanFormatter.Logger = logger;

            var sourceFileFormatter = FileFormatter.CreateWriter();
            sourceFileFormatter.Logger = logger;

            return new UstMessagePackResolver
            {
                TextSpanFormatter = textSpanFormatter,
                FileFormatter = sourceFileFormatter
            };
        }

        public static UstMessagePackResolver CreateReader(BinaryFile serializedFile, bool isLineColumn,
            HashSet<IFile> sourceFiles, Action<(IFile, TimeSpan)> readSourceFileAction, ILogger logger)
        {
            var textSpanFormatter = TextSpanFormatter.CreateReader(serializedFile);
            textSpanFormatter.IsLineColumn = isLineColumn;
            textSpanFormatter.Logger = logger;

            var sourceFileFormatter = FileFormatter.CreateReader(sourceFiles, readSourceFileAction);
            sourceFileFormatter.Logger = logger;

            return new UstMessagePackResolver
            {
                TextSpanFormatter = textSpanFormatter,
                FileFormatter = sourceFileFormatter
            };
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            Type type = typeof(T);

            if (type == typeof(TextSpan))
            {
                return (IMessagePackFormatter<T>) TextSpanFormatter;
            }

            if (type == typeof(TextFile))
            {
                return (IMessagePackFormatter<T>) FileFormatter;
            }

            if (type == typeof(Language))
            {
                return (IMessagePackFormatter<T>) LanguageFormatter;
            }

            if (type == typeof(RootUst))
            {
                return (IMessagePackFormatter<T>) RootUstFormatter;
            }

            return StandardResolver.Instance.GetFormatter<T>();
        }
    }
}