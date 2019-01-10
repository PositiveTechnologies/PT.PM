using MessagePack;
using PT.PM.Common.MessagePack;

namespace PT.PM.Common
{
    [MessagePackObject]
    [MessagePackFormatter(typeof(LanguageFormatter))]
    public class Language
    {
        [Key(0)]
        public readonly string Key;

        [IgnoreMember]
        public readonly string Title;

        [IgnoreMember]
        public readonly string[] Extensions;

        [IgnoreMember]
        public readonly bool IsCaseInsensitive;

        [IgnoreMember]
        public readonly Language[] Sublanguages;

        [IgnoreMember]
        public readonly bool HaveAntlrParser;

        [IgnoreMember]
        public readonly bool IsPattern;

        [IgnoreMember]
        public readonly bool IsSql;

        public Language(string key, string extension, bool caseInsensitive, string title = null,
            Language[] sublanguages = null, bool haveAntlrParser = true, bool isPattern = true, bool isSql = false)
            :this(key, new string[] { extension }, caseInsensitive, title, sublanguages, haveAntlrParser, isPattern, isSql)
        {
        }

        public Language(string key, string[] extensions, bool caseInsensitive, string title = null,
            Language[] sublanguages = null, bool haveAntlrParser = true, bool isPattern = true, bool isSql = false)
        {
            Key = key;
            Extensions = extensions;
            IsCaseInsensitive = caseInsensitive;
            Title = string.IsNullOrEmpty(title) ? Key.ToString() : title;
            Sublanguages = sublanguages ?? ArrayUtils<Language>.EmptyArray;
            HaveAntlrParser = haveAntlrParser;
            IsPattern = isPattern;
            IsSql = isSql;
        }

        public override string ToString() => Title;
    }
}
