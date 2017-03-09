using PT.PM.Common;
using System;
using PT.PM.Common.Ust;
using PT.PM.Common.Nodes;

namespace PT.PM.CSharpAstConversion
{
    public class AspxConverter: IParseTreeToUstConverter
    {
        public UstType AstType { get; set; }

        public Language MainLanguage => Language.Aspx;

        public LanguageFlags ConvertedLanguages { get; set; }

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public SemanticsInfo SemanticsInfo { get; set; }


        public AspxConverter()
        {
            ConvertedLanguages = MainLanguage.GetLanguageWithDependentLanguages();
        }

        public Ust Convert(ParseTree langAst)
        {
            Ust result = null;
            
            var aspxAst = (AspxParseTree)langAst;
            try
            {
                var converter = new AspxToCsConverter(aspxAst.FileName, aspxAst.FileData);
                result = new MostCommonUst((FileNode)aspxAst.Root.Accept(converter), ConvertedLanguages);
            }
            catch (Exception ex)
            {
                Logger.LogError(string.Format("Conversion error in \"{0}\"", aspxAst.FileName), ex);
                result = new MostCommonUst();
            }

            result.FileName = langAst.FileName;
            return result;
        }
    }
}
