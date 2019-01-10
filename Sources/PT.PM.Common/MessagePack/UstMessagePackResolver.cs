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
        
        public static UstMessagePackResolver CreateWriter(CodeFile codeFile, bool isLineColumn, ILogger logger)
        {
            var textSpanFormatter = TextSpanFormatter.CreateWriter(codeFile);
            textSpanFormatter.IsLineColumn = isLineColumn;
            textSpanFormatter.Logger = logger;

            var codeFileFormatter = FileFormatter.CreateWriter();
            codeFileFormatter.Logger = logger;

            return new UstMessagePackResolver
            {
                TextSpanFormatter = textSpanFormatter,
                FileFormatter = codeFileFormatter
            };
        }

        public static UstMessagePackResolver CreateReader(BinaryFile serializedFile, bool isLineColumn,
            HashSet<IFile> sourceFiles, Action<(IFile, TimeSpan)> readCodeFileAction, ILogger logger)
        {
            var textSpanFormatter = TextSpanFormatter.CreateReader(serializedFile);
            textSpanFormatter.IsLineColumn = isLineColumn;
            textSpanFormatter.Logger = logger;

            var codeFileFormatter = FileFormatter.CreateReader(sourceFiles, readCodeFileAction);
            codeFileFormatter.Logger = logger;

            return new UstMessagePackResolver
            {
                TextSpanFormatter = textSpanFormatter,
                FileFormatter = codeFileFormatter
            };
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            Type type = typeof(T);

            if (type == typeof(TextSpan))
            {
                return (IMessagePackFormatter<T>) TextSpanFormatter;
            }

            if (type == typeof(CodeFile))
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