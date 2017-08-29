using PT.PM.Common;
using System;
using PT.PM.Common.Ust;
using PT.PM.Common.Nodes;
using PT.PM.Common.Exceptions;

namespace PT.PM.CSharpParseTreeUst
{
    public class AspxConverter: IParseTreeToUstConverter
    {
        public Language MainLanguage => Language.Aspx;

        public LanguageFlags ConvertedLanguages { get; set; }

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public AspxConverter()
        {
            ConvertedLanguages = MainLanguage.GetLanguageWithDependentLanguages();
        }

        public Ust Convert(ParseTree langParseTree)
        {
            Ust result = null;
            
            var aspxParseTree = (AspxParseTree)langParseTree;
            try
            {
                var converter = new AspxToCsConverter(aspxParseTree.FileName, aspxParseTree.FileData);
                result = new Ust((FileNode)aspxParseTree.Root.Accept(converter), ConvertedLanguages);
            }
            catch (Exception ex)
            {
                Logger.LogError(new ConversionException(aspxParseTree.FileName, ex));
                result = new Ust();
            }

            result.FileName = langParseTree.FileName;
            return result;
        }
    }
}
