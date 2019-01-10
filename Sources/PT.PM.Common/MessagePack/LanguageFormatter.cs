using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace PT.PM.Common.MessagePack
{
    public class LanguageFormatter : IMessagePackFormatter<Language>
    {
        public int Serialize(ref byte[] bytes, int offset, Language value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteString(ref bytes, offset, value.Key);
        }

        public Language Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
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
    }
}