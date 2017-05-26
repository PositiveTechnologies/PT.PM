using PT.PM.Common;
using AspxParser;
using System;
using System.IO;
using PT.PM.Common.Exceptions;

namespace PT.PM.CSharpParseTreeUst
{
    public class AspxPmParser : ILanguageParser
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Language Language => Language.Aspx;

        public ParseTree Parse(SourceCodeFile sourceCodeFile)
        {
            ParseTree result = null;

            var filePath = Path.Combine(sourceCodeFile.RelativePath, sourceCodeFile.Name);
            if (sourceCodeFile.Code != null)
            {
                try
                {
                    var parser = new AspxParser.AspxParser(sourceCodeFile.RelativePath);
                    var source = new AspxSource(sourceCodeFile.FullPath, sourceCodeFile.Code);
                    AspxParseResult aspxTree = parser.Parse(source);
                    foreach (var error in aspxTree.ParseErrors)
                    {
                        Logger.LogError(new ParsingException(filePath, message: error.Message) { TextSpan = error.Location.GetTextSpan() });
                    }
                    result = new AspxParseTree(aspxTree.RootNode);
                }
                catch (Exception ex)
                {
                    Logger.LogError(new ParsingException(filePath, ex));
                    result = new CSharpRoslynParseTree();
                }
            }
            else
            {
                result = new CSharpRoslynParseTree();
            }
            result.FileName = filePath;
            result.FileData = sourceCodeFile.Code;

            return result;
        }

        public void ClearCache()
        {
        }
    }
}
