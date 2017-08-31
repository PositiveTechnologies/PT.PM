namespace PT.PM.Common
{
    public class LanguageInfo
    {
        public readonly Language Language;

        public readonly string Title;

        public readonly string[] Extensions;

        public readonly bool CaseInsensitive;

        public readonly Language[] Sublanguages;

        public readonly bool HaveAntlrParser;

        public LanguageInfo(Language language, string extension, bool caseInsensitive, string title = null,
            Language[] sublanguages = null, bool haveAntlrParser = true)
            :this(language, new string[] { extension }, caseInsensitive, title, sublanguages, haveAntlrParser)
        {
        }

        public LanguageInfo(Language language, string[] extensions, bool caseInsensitive, string title = null,
            Language[] sublanguages = null, bool haveAntlrParser = true)
        {
            Language = language;
            Extensions = extensions;
            CaseInsensitive = caseInsensitive;
            Title = string.IsNullOrEmpty(title) ? Language.ToString() : title;
            Sublanguages = sublanguages ?? ArrayUtils<Language>.EmptyArray;
            HaveAntlrParser = haveAntlrParser;
        }

        public override string ToString() => Title;
    }
}
