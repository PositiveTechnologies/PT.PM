using PT.PM.Common;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using PT.PM.Common.Exceptions;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrParser : ILanguageParser
    {
        private int processedFilesCount = 1;
        private static ReaderWriterLockSlim lexerLock = new ReaderWriterLockSlim();
        private static ReaderWriterLockSlim parserLock = new ReaderWriterLockSlim();

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Lexer Lexer { get; private set; }

        public Parser Parser { get; private set; }

        public abstract Language Language { get; }

        public int MaxStackSize { get; set; } = 0;

        public int MaxTimespan { get; set; } = 0;

        public bool UseFastParseStrategyAtFirst { get; set; } = true;

        public int ClearCacheLexerFilesCount { get; set; } = 300;

        public int ClearCacheParserFilesCount { get; set; } = 150;

        public long MemoryConsumptionMb { get; set; } = 300;

        protected abstract Lexer InitLexer(ICharStream inputStream);

        protected abstract Parser InitParser(ITokenStream inputStream);

        protected abstract ParserRuleContext Parse(Parser parser);

        protected abstract AntlrParseTree Create(ParserRuleContext syntaxTree);

        protected abstract IVocabulary Vocabulary { get; }

        protected abstract int CommentsChannel { get; }

        public AntlrParser()
        {
            Lexer = InitLexer(null);
            Parser = InitParser(null);
        }

        public ParseTree Parse(SourceCodeFile sourceCodeFile)
        {
            if (MaxStackSize == 0 && MaxTimespan == 0)
            {
                return TokenizeAndParse(sourceCodeFile);
            }
            else
            {
                ParseTree result = null;

                Thread thread = new Thread(delegate ()
                {
                    result = TokenizeAndParse(sourceCodeFile);
                },
                MaxStackSize);
                thread.Start();
                bool finished = thread.Join(MaxTimespan == 0 ? int.MaxValue : MaxTimespan);
                if (!finished)
                {
                    thread.Interrupt();
                    thread.Abort();
                    Logger.LogError($"Parsing error in \"{sourceCodeFile.Name}\": Timeout ({MaxTimespan}) expired");
                }

                return result;
            }
        }

        public void ClearCache()
        {
            Lexer lexer = InitLexer(new AntlrInputStream());
            lexer.Interpreter.ClearDFA();
            Parser parser = InitParser(new CommonTokenStream(new ListTokenSource(new IToken[0])));
            parser.Interpreter.ClearDFA();
            processedFilesCount = 1;
        }

        protected virtual ParseTree TokenizeAndParse(SourceCodeFile sourceCodeFile)
        {
            AntlrParseTree result = null;

            var filePath = Path.Combine(sourceCodeFile.RelativePath, sourceCodeFile.Name);
            if (sourceCodeFile.Code != null)
            {
                var errorListener = new AntlrMemoryErrorListener();
                errorListener.FileName = filePath;
                errorListener.FileData = sourceCodeFile.Code;
                errorListener.Logger = Logger;
                try
                {
                    var preprocessedText = PreprocessText(sourceCodeFile);
                    AntlrInputStream inputStream;
                    if (Language.IsCaseInsensitive())
                    {
                        inputStream = new AntlrCaseInsensitiveInputStream(preprocessedText);
                    }
                    else
                    {
                        inputStream = new AntlrInputStream(preprocessedText);
                    }
                    inputStream.name = filePath;

                    Lexer lexer = InitLexer(inputStream);
                    Lexer = lexer;
                    lexer.RemoveErrorListeners();
                    lexer.AddErrorListener(errorListener);
                    var commentTokens = new List<IToken>();

                    var stopwatch = Stopwatch.StartNew();
                    IList<IToken> tokens = GetAllTokens(lexer);
                    stopwatch.Stop();
                    long lexerTimeSpanTicks = stopwatch.ElapsedTicks;

#if DEBUG
                    var codeTokensStr = AntlrHelper.GetTokensString(tokens, Vocabulary, onlyDefaultChannel: false);
#endif

                    ClearLexerCacheIfRequired(lexer);

                    foreach (var token in tokens)
                    {
                        if (token.Channel == CommentsChannel)
                        {
                            commentTokens.Add(token);
                        }
                    }

                    stopwatch.Restart();
                    var codeTokenSource = new ListTokenSource(tokens);
                    var codeTokenStream = new CommonTokenStream(codeTokenSource);
                    ParserRuleContext syntaxTree = ParseTokens(sourceCodeFile, errorListener, codeTokenStream);
                    stopwatch.Stop();
                    long parserTimeSpanTicks = stopwatch.ElapsedTicks;

                    IncrementProcessedFilesCount();

                    result = Create(syntaxTree);
                    result.LexerTimeSpan = new TimeSpan(lexerTimeSpanTicks);
                    result.ParserTimeSpan = new TimeSpan(parserTimeSpanTicks);
                    result.Tokens = tokens;
                    result.Comments = commentTokens;
                }
                catch (Exception ex)
                {
                    Logger.LogError(new ParsingException(filePath, ex));

                    if (result == null)
                    {
                        result = Create(null);
                    }
                }
            }
            else
            {
                result = Create(null);
            }
            result.FileName = filePath;
            result.FileData = sourceCodeFile.Code;

            return result;
        }

        protected ParserRuleContext ParseTokens(SourceCodeFile sourceCodeFile,
            AntlrMemoryErrorListener errorListener, BufferedTokenStream codeTokenStream,
            Func<ITokenStream, Parser> initParserFunc = null, Func<Parser, ParserRuleContext> parseFunc = null)
        {
            Parser parser = initParserFunc != null ? initParserFunc(codeTokenStream) : InitParser(codeTokenStream);
            parser.RemoveErrorListeners();
            Parser = parser;
            ParserRuleContext syntaxTree;

            if (UseFastParseStrategyAtFirst)
            {
                parser.Interpreter.PredictionMode = PredictionMode.Sll;
                parser.ErrorHandler = new BailErrorStrategy();
                parser.TrimParseTree = true;

                parserLock.EnterReadLock();
                try
                {
                    syntaxTree = parseFunc != null ? parseFunc(parser) : Parse(parser);
                }
                catch (ParseCanceledException)
                {
                    parserLock.ExitReadLock();
                    parser.AddErrorListener(errorListener);
                    codeTokenStream.Reset();
                    parser.Reset();
                    parser.Interpreter.PredictionMode = PredictionMode.Ll;
                    parser.ErrorHandler = new DefaultErrorStrategy();

                    parserLock.EnterReadLock();
                    syntaxTree = parseFunc != null ? parseFunc(parser) : Parse(parser);
                }
                finally
                {
                    parserLock.ExitReadLock();
                }
            }
            else
            {
                parser.AddErrorListener(errorListener);
                parserLock.EnterReadLock();
                try
                {
                    syntaxTree = parseFunc != null ? parseFunc(parser) : Parse(parser);
                }
                finally
                {
                    parserLock.ExitReadLock();
                }
            }
            ClearParserCacheIfRequired(parser);

#if DEBUG
            var tree = syntaxTree.ToStringTree(parser);
#endif

            return syntaxTree;
        }

        /// <summary>
        /// Converts \r to \r\n.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected virtual string PreprocessText(SourceCodeFile file)
        {
            var text = file.Code;
            var result = new StringBuilder(text.Length);
            int i = 0;
            while (i < text.Length)
            {
                if (text[i] == '\r')
                {
                    if (i + 1 >= text.Length)
                    {
                        result.Append('\n');
                    }
                    else if (text[i + 1] != '\n')
                    {
                        result.Append('\n');
                    }
                    else
                    {
                        result.Append(text[i]);
                    }
                }
                else
                {
                    result.Append(text[i]);
                }
                i++;
            }
            return result.ToString();
        }

        protected static IList<IToken> GetAllTokens(Lexer lexer)
        {
            IList<IToken> tokens = null;
            lexerLock.EnterReadLock();
            try
            {
                tokens = lexer.GetAllTokens();
            }
            finally
            {
                lexerLock.ExitReadLock();
            }
            return tokens;
        }

        protected void ClearLexerCacheIfRequired(Lexer lexer)
        {
            if (processedFilesCount % ClearCacheLexerFilesCount == 0)
            {
                lexerLock.EnterWriteLock();
                try
                {
                    lexer.Interpreter.ClearDFA();
                }
                finally
                {
                    lexerLock.ExitWriteLock();
                }
            }
        }

        protected void ClearParserCacheIfRequired(Parser parser)
        {
            if (processedFilesCount % ClearCacheParserFilesCount == 0 &&
                GC.GetTotalMemory(true) / 1000000 > MemoryConsumptionMb)
            {
                parserLock.EnterWriteLock();
                try
                {
                    parser.Interpreter.ClearDFA();
                }
                finally
                {
                    parserLock.ExitWriteLock();
                }
            }
        }

        protected void IncrementProcessedFilesCount()
        {
            Interlocked.Increment(ref processedFilesCount);
            Interlocked.CompareExchange(ref processedFilesCount, 0, int.MaxValue);
        }
    }
}
