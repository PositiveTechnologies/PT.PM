using PT.PM.AntlrUtils;
using PT.PM.Common;
using Antlr4.Runtime;
using System;
using PT.PM.Common.Exceptions;
using System.Threading;
using PT.PM.Common.Files;

namespace PT.PM.Dsl
{
    public class DslAntlrParser : ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public DslParser.PatternContext Parse(string patternKey, string data)
        {
            DslParser.PatternContext pattern;
            var sourceFile = new TextFile(data) { PatternKey = patternKey ?? data };
            var errorListener = new AntlrMemoryErrorListener(sourceFile)
            {
                IsPattern = true,
                Logger = Logger
            };
            try
            {
                var inputStream = new LightInputStream(DslLexer.DefaultVocabulary, sourceFile, data);
                DslLexer lexer = new DslLexer(inputStream);

                lexer.TokenFactory = new LightTokenFactory();
                lexer.RemoveErrorListeners();
                lexer.AddErrorListener(errorListener);

                var tokenStream = new CommonTokenStream(lexer);
                DslParser parser = new DslParser(tokenStream);
                parser.RemoveErrorListeners();
                parser.AddErrorListener(errorListener);
                pattern = parser.pattern();
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ParsingException(sourceFile, ex));
                throw;
            }

            return pattern;
        }
    }
}
