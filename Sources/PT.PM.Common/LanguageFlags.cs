using System;

namespace PT.PM.Common
{
    [Flags]
    public enum LanguageFlags
    {
        None = 0,
        CSharp = 1 << Language.CSharp,
        Aspx = 1 << Language.Aspx,
        Java = 1 << Language.Java,
        Php = 1 << Language.Php,
        PlSql = 1 << Language.PlSql,
        TSql = 1 << Language.TSql,
        JavaScript = 1 << Language.JavaScript,
        Html = 1 << Language.Html
    }
}
