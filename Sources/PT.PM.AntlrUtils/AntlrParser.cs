using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using PT.PM.Common.Files;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrParser : AntlrBaseHandler, ILanguageParser
    {
        private static long processedFilesCount;
        private static long processedBytesCount;
        private static long checkNumber;
        private static volatile bool excessMemory;
        public Parser Parser { get; private set; }
        
        public AntlrLexer Lexer { get; protected set; }

        protected abstract string ParserSerializedATN { get; }
        
        protected abstract int CommentsChannel { get; }

        protected abstract Parser InitParser(ITokenStream inputStream);

        protected abstract ParserRuleContext Parse(Parser parser);

        protected abstract AntlrParseTree Create(ParserRuleContext syntaxTree);

        public int LineOffset { get; set; }

        public AntlrParser()
        {
            Parser = InitParser(null);
        }

        public ParseTree Parse(TextFile sourceFile)
        {
            if (sourceFile.Data == null)
            {
                return null;
            }

            AntlrParseTree result = null;
            var filePath = sourceFile.RelativeName;
            var errorListener = new AntlrMemoryErrorListener();
            errorListener.SourceFile = sourceFile;
            errorListener.Logger = Logger;
            errorListener.LineOffset = LineOffset;
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var tokens = Lexer?.GetTokens(sourceFile, errorListener);
                var lexerTimeSpan = stopwatch.Elapsed;
                
                var commentTokens = new List<IToken>();
                
                foreach (IToken token in tokens)
                {
                    if (token.Channel == CommentsChannel)
                    {
                        commentTokens.Add(token);
                    }
                }

                stopwatch.Restart();
                var codeTokenSource = new ListTokenSource(tokens);
                var codeTokenStream = new CommonTokenStream(codeTokenSource);
                ParserRuleContext syntaxTree = ParseTokens(sourceFile, errorListener, codeTokenStream);
                stopwatch.Stop();
                TimeSpan parserTimeSpan = stopwatch.Elapsed;

                result = Create(syntaxTree);
                result.LexerTimeSpan = lexerTimeSpan;
                result.ParserTimeSpan = parserTimeSpan;

                result.Tokens = tokens;
                result.Comments = commentTokens;
                result.SourceFile = sourceFile;
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ParsingException(sourceFile, ex));
            }
            finally
            {
                long localProcessedFilesCount = Interlocked.Increment(ref processedFilesCount);
                long localProcessedBytesCount = Interlocked.Add(ref processedBytesCount, sourceFile.Data.Length);

                long divideResult = localProcessedBytesCount / ClearCacheFilesBytes;
                bool exceededProcessedBytes = divideResult > Thread.VolatileRead(ref checkNumber);
                checkNumber = divideResult;

                if (Process.GetCurrentProcess().PrivateMemorySize64 > MemoryConsumptionBytes)
                {
                    bool prevExcessMemory = excessMemory;
                    excessMemory = true;

                    if (!prevExcessMemory ||
                        exceededProcessedBytes ||
                        localProcessedFilesCount % ClearCacheFilesCount == 0)
                    {
                        lock (lexerAtns)
                        {
                            lexerAtns.Remove(Language);
                        }
                        lock (parserAtns)
                        {
                            parserAtns.Remove(Language);
                        }

                        Logger.LogInfo($"Memory cleared due to big memory consumption during {sourceFile.RelativeName} parsing.");
                    }
                }
                else
                {
                    excessMemory = false;
                }
            }

            return result;
        }

        protected ParserRuleContext ParseTokens(TextFile sourceFile,
            AntlrMemoryErrorListener errorListener, BufferedTokenStream codeTokenStream,
            Func<ITokenStream, Parser> initParserFunc = null, Func<Parser, ParserRuleContext> parseFunc = null)
        {
            Parser parser = initParserFunc != null ? initParserFunc(codeTokenStream) : InitParser(codeTokenStream);
            parser.Interpreter = new ParserATNSimulator(parser, GetOrCreateAtn(false, ParserSerializedATN));
            parser.RemoveErrorListeners();
            Parser = parser;
            ParserRuleContext syntaxTree = null;

            if (UseFastParseStrategyAtFirst)
            {
                parser.Interpreter.PredictionMode = PredictionMode.Sll;
                parser.ErrorHandler = new BailErrorStrategy();
                parser.TrimParseTree = true;

                try
                {
                    syntaxTree = parseFunc != null ? parseFunc(parser) : Parse(parser);
                }
                catch (ParseCanceledException)
                {
                    parser.AddErrorListener(errorListener);
                    codeTokenStream.Reset();
                    parser.Reset();
                    parser.Interpreter.PredictionMode = PredictionMode.Ll;
                    parser.ErrorHandler = new DefaultErrorStrategy();

                    syntaxTree = parseFunc != null ? parseFunc(parser) : Parse(parser);
                }
            }
            else
            {
                parser.AddErrorListener(errorListener);
                syntaxTree = parseFunc != null ? parseFunc(parser) : Parse(parser);
            }

            return syntaxTree;
        }
    }
}
