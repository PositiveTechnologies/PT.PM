using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Files;

namespace PT.PM.Common.MessagePack
{
    public class LanguageFormatter : IMessagePackFormatter<Language>
    {
        public BinaryFile SerializedFile { get; private set; }

        public static LanguageFormatter CreateWriter()
        {
            return new LanguageFormatter();
        }

        public static LanguageFormatter CreateReader(BinaryFile serializedFile)
        {
            if (serializedFile == null)
            {
                throw new ArgumentNullException(nameof(serializedFile));
            }

            return new LanguageFormatter
            {
                SerializedFile = serializedFile
            };
        }

        private LanguageFormatter() {}

        public int Serialize(ref byte[] bytes, int offset, Language value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteString(ref bytes, offset, value.Key);
        }

        public Language Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            try
            {
                string langKey = MessagePackBinary.ReadString(bytes, offset, out readSize);
                HashSet<Language> langs = langKey.ParseLanguages();
    
                if (langs.Count == 0)
                {
                    return Uncertain.Language;
                }
    
                using (var enumerator = langs.GetEnumerator())
                {
                    enumerator.MoveNext();
                    return enumerator.Current;
                }
            }
            catch (InvalidOperationException ex) // Catch incorrect format exceptions
            {
                throw new ReadException(SerializedFile, ex, $"Error during reading {nameof(Language)} at {offset} offset; Message: {ex.Message}");
            }
        }
    }
}