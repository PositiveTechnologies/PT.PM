namespace PT.PM.Common
{
    public class LanguageInfo
    {
        public readonly Language Key;

        public readonly string Title;

        public readonly string[] Extensions;

        public readonly bool IsCaseInsensitive;

        public readonly Language[] Sublanguages;

        public readonly bool HasAntlrParser;

        public readonly bool IsPattern;

        public readonly bool IsSql;

        public LanguageInfo(Language key, string extension, bool caseInsensitive, string title = null,
            Language[] sublanguages = null, bool hasAntlrParser = true, bool isPattern = true, bool isSql = false)
            :this(key, new string[] { extension }, caseInsensitive, title, sublanguages, hasAntlrParser, isPattern, isSql)
        {
        }

        public LanguageInfo(Language key, string[] extensions, bool caseInsensitive, string title = null,
            Language[] sublanguages = null, bool hasAntlrParser = true, bool isPattern = true, bool isSql = false)
        {
            Key = key;
            Extensions = extensions;
            IsCaseInsensitive = caseInsensitive;
            Title = string.IsNullOrEmpty(title) ? Key.ToString() : title;
            Sublanguages = sublanguages ?? ArrayUtils<Language>.EmptyArray;
            HasAntlrParser = hasAntlrParser;
            IsPattern = isPattern;
            IsSql = isSql;
        }

        public override string ToString() => Title;
    }
}
