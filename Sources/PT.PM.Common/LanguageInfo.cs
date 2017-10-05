namespace PT.PM.Common
{
    public class LanguageInfo
    {
        public readonly string Key;

        public readonly string Title;

        public readonly string[] Extensions;

        public readonly bool IsCaseInsensitive;

        public readonly LanguageInfo[] Sublanguages;

        public readonly bool HaveAntlrParser;

        public readonly bool IsPattern;

        public readonly bool IsSql;

        public LanguageInfo(string language, string extension, bool caseInsensitive, string title = null,
            LanguageInfo[] sublanguages = null, bool haveAntlrParser = true, bool isPattern = true, bool isSql = false)
            :this(language, new string[] { extension }, caseInsensitive, title, sublanguages, haveAntlrParser, isPattern, isSql)
        {
        }

        public LanguageInfo(string language, string[] extensions, bool caseInsensitive, string title = null,
            LanguageInfo[] sublanguages = null, bool haveAntlrParser = true, bool isPattern = true, bool isSql = false)
        {
            Key = language;
            Extensions = extensions;
            IsCaseInsensitive = caseInsensitive;
            Title = string.IsNullOrEmpty(title) ? Key.ToString() : title;
            Sublanguages = sublanguages ?? ArrayUtils<LanguageInfo>.EmptyArray;
            HaveAntlrParser = haveAntlrParser;
            IsPattern = isPattern;
            IsSql = isSql;
        }

        public override string ToString() => Title;
    }
}
