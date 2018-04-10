using PT.PM.Common;
using AspxParser;
using System;
using System.IO;
using PT.PM.Common.Exceptions;
using System.Threading;

namespace PT.PM.CSharpParseTreeUst
{
    public class AspxParser : ILanguageParser
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Language Language => Aspx.Language;

        public ParseTree Parse(CodeFile sourceCodeFile)
        {
            ParseTree result = null;

            var filePath = Path.Combine(sourceCodeFile.RelativePath, sourceCodeFile.Name);
            if (sourceCodeFile.Code != null)
            {
                try
                {
                    var parser = new global::AspxParser.AspxParser(sourceCodeFile.RelativePath);
                    var source = new AspxSource(sourceCodeFile.FullName, sourceCodeFile.Code);
                    AspxParseResult aspxTree = parser.Parse(source);
                    foreach (var error in aspxTree.ParseErrors)
                    {
                        Logger.LogError(new ParsingException(sourceCodeFile, message: error.Message)
                        {
                            TextSpan = error.Location.GetTextSpan()
                        });
                    }
                    result = new AspxParseTree(aspxTree.RootNode);
                }
                catch (Exception ex) when (!(ex is ThreadAbortException))
                {
                    Logger.LogError(new ParsingException(sourceCodeFile, ex));
                    result = new CSharpRoslynParseTree();
                }
            }
            else
            {
                result = new CSharpRoslynParseTree();
            }
            result.SourceCodeFile = sourceCodeFile;

            return result;
        }

        public void ClearCache()
        {
        }
    }
}
