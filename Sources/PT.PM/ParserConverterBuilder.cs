using System;
using PT.PM.AstConversion;
using PT.PM.AstParsing;
using PT.PM.Common;
using PT.PM.CSharpAstConversion;
using PT.PM.JavaAstConversion;
using PT.PM.SqlAstConversion;
using System.Collections.Generic;
using PT.PM.JavaScriptAstConversion;

namespace PT.PM
{
    public class ParserConverterBuilder
    {
        public static Dictionary<Language, ParserConverterSet> GetParserConverterSets(LanguageFlags languageFlags)
        {
            var languages = new List<Language>();
            languages.AddRange(languageFlags.GetLanguages());
            languages.AddRange(languageFlags.GetImpactLanguages());
            Dictionary<Language, ParserConverterSet> result = new Dictionary<Language, ParserConverterSet>();
            foreach (var language in languages)
            {
                result[language] = GetParserConverterSet(language);
                result[language].Converter.ConvertedLanguages &= languageFlags;
            }
            return result;
        }

        public static ParserConverterSet GetParserConverterSet(Language language)
        {
            var result = new ParserConverterSet();
            switch (language)
            {
                case Language.CSharp:
                    result.Parser = new CSharpRoslynParser();
                    result.Converter = new CSharpRoslynParseTreeConverter();
                    result.SemanticsCollector = new CSharpRoslynSemanticsCollector();
                    break;
                case Language.Java:
                    result.Parser = new JavaAntlrParser();
                    result.Converter = new JavaAntlrParseTreeConverter();
                    break;
                case Language.Php:
                    result.Parser = new PhpAntlrParser();
                    result.Converter = new PhpAntlrParseTreeConverter();
                    break;
                case Language.PlSql:
                    result.Parser = new PlSqlAntlrParser();
                    result.Converter = new PlSqlAntlrConverter();
                    break;
                case Language.TSql:
                    result.Parser = new TSqlAntlrParser();
                    result.Converter = new TSqlAntlrConverter();
                    break;
                case Language.Aspx:
                    result.Parser = new AspxPmParser();
                    result.Converter = new AspxConverter();
                    break;
                case Language.JavaScript:
                    result.Parser = new JavaScriptAntlrParser();
                    result.Converter = new JavaScriptParseTreeConverter();
                    break;
                default:
                    throw new NotImplementedException($"Language {language} is not supported");
            }
            return result;
        }
    }
}