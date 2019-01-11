using System;
using System.Collections.Generic;
using MessagePack;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes;

namespace PT.PM.Common.MessagePack
{
    public class RootUstMessagePackSerializer : ISerializer
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool LineColumnTextSpans { get; set; }

        public byte[] Serialize(RootUst rootUst)
        {
            var writerResolver = UstMessagePackResolver.CreateWriter(rootUst.SourceFile, LineColumnTextSpans, Logger);
            return MessagePackSerializer.Serialize(rootUst, writerResolver);
        }

        public RootUst Deserialize(BinaryFile serializedFile, HashSet<IFile> sourceFiles, Action<(IFile, TimeSpan)> readSourceFileAction)
        {
            var readerResolver =
                UstMessagePackResolver.CreateReader(serializedFile, LineColumnTextSpans, sourceFiles, readSourceFileAction, Logger);
            var result = MessagePackSerializer.Deserialize<RootUst>(serializedFile.Data, readerResolver);
            result.FillAscendants();
            return result;
        }
    }
}