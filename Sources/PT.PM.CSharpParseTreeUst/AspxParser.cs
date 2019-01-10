using AspxParser;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using System;
using System.Threading;
using PT.PM.Common.Files;

namespace PT.PM.CSharpParseTreeUst
{
    public class AspxParser : ILanguageParser
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Language Language => Aspx.Language;

        public ParseTree Parse(CodeFile sourceCodeFile)
        {
            if (sourceCodeFile.Data == null)
            {
                return null;
            }

            try
            {
                var parser = new global::AspxParser.AspxParser(sourceCodeFile.RelativePath);
                var source = new AspxSource(sourceCodeFile.FullName, sourceCodeFile.Data);
                AspxParseResult aspxTree = parser.Parse(source);
                foreach (var error in aspxTree.ParseErrors)
                {
                    Logger.LogError(new ParsingException(sourceCodeFile, message: error.Message)
                    {
                        TextSpan = error.Location.GetTextSpan()
                    });
                }
                var result = new AspxParseTree(aspxTree.RootNode);
                result.SourceCodeFile = sourceCodeFile;

                return result;
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ParsingException(sourceCodeFile, ex));
                return null;
            }
        }
    }
}
