namespace PT.PM.Common
{
    public class Language
    {
        public readonly string Key;

        public readonly string Title;

        public readonly string[] Extensions;

        public readonly bool IsCaseInsensitive;

        public readonly Language[] Sublanguages;

        public readonly bool HaveAntlrParser;

        public readonly bool IsPattern;

        public readonly bool IsSql;

        public Language(string language, string extension, bool caseInsensitive, string title = null,
            Language[] sublanguages = null, bool haveAntlrParser = true, bool isPattern = true, bool isSql = false)
            :this(language, new string[] { extension }, caseInsensitive, title, sublanguages, haveAntlrParser, isPattern, isSql)
        {
        }

        public Language(string language, string[] extensions, bool caseInsensitive, string title = null,
            Language[] sublanguages = null, bool haveAntlrParser = true, bool isPattern = true, bool isSql = false)
        {
            Key = language;
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
