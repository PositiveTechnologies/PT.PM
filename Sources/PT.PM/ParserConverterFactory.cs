using PT.PM.Common;
using PT.PM.CSharpParseTreeUst;
using PT.PM.CSharpParseTreeUst.RoslynUstVisitor;
using PT.PM.JavaParseTreeUst;
using PT.PM.JavaParseTreeUst.Converter;
using PT.PM.JavaScriptParseTreeUst;
using PT.PM.PhpParseTreeUst;
using PT.PM.SqlParseTreeUst;
using System;

namespace PT.PM
{
    public static class ParserConverterFactory
    {
        public static ILanguageParser CreateParser(Language language)
        {
            switch (language)
            {
                case Language.CSharp:
                    return new CSharpRoslynParser();
                case Language.Java:
                    return new JavaAntlrParser();
                case Language.Php:
                    return new PhpAntlrParser();
                case Language.PlSql:
                    return new PlSqlAntlrParser();
                case Language.TSql:
                    return new TSqlAntlrParser();
                case Language.Aspx:
                    return new CSharpParseTreeUst.AspxParser();
                case Language.JavaScript:
                    return new JavaScriptAntlrParser();
                case Language.Html:
                    return new PhpAntlrParser();
                default:
                    throw new NotImplementedException($"Language {language} is not supported");
            }
        }

        public static IParseTreeToUstConverter CreateConverter(Language language)
        {
            switch (language)
            {
                case Language.CSharp:
                    return new CSharpRoslynParseTreeConverter();
                case Language.Java:
                    return new JavaAntlrParseTreeConverter();
                case Language.Php:
                    return new PhpAntlrParseTreeConverter();
                case Language.PlSql:
                    return new PlSqlAntlrConverter();
                case Language.TSql:
                    return new TSqlAntlrConverter();
                case Language.Aspx:
                    return new AspxConverter();
                case Language.JavaScript:
                    return new JavaScriptParseTreeConverter();
                case Language.Html:
                    return new PhpAntlrParseTreeConverter();
                default:
                    throw new NotImplementedException($"Language {language} is not supported");
            }
        }
    }
}
