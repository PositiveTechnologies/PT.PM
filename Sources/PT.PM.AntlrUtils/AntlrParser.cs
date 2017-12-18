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

        public virtual CaseInsensitiveType CaseInsensitiveType { get; } = CaseInsensitiveType.None;

        public bool UseFastParseStrategyAtFirst { get; set; } = true;

        public int ClearCacheLexerFilesCount { get; set; } = 100;

        public int ClearCacheParserFilesCount { get; set; } = 50;

        public long MemoryConsumptionMb { get; set; } = 300;

        public abstract Lexer InitLexer(ICharStream inputStream);

        public abstract Parser InitParser(ITokenStream inputStream);

        protected abstract ParserRuleContext Parse(Parser parser);

        protected abstract AntlrParseTree Create(ParserRuleContext syntaxTree);

        protected abstract IVocabulary Vocabulary { get; }

        protected abstract int CommentsChannel { get; }

        public int LineOffset { get; set; }

        public AntlrParser()
        {
            Lexer = InitLexer(null);
            Parser = InitParser(null);
        }

        public ParseTree Parse(CodeFile sourceCodeFile)
        {
            AntlrParseTree result = null;

            var filePath = sourceCodeFile.RelativeName;
            if (sourceCodeFile.Code != null)
            {
                var errorListener = new AntlrMemoryErrorListener();
                errorListener.CodeFile = sourceCodeFile;
                errorListener.Logger = Logger;
                errorListener.LineOffset = LineOffset;
                try
                {
                    var preprocessedText = PreprocessText(sourceCodeFile);
                    AntlrInputStream inputStream;
                    if (Language.IsCaseInsensitive)
                    {
                        inputStream = new AntlrCaseInsensitiveInputStream(preprocessedText, CaseInsensitiveType);
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

                    ClearCacheIfRequired(lexer.Interpreter, lexerLock, ClearCacheLexerFilesCount);

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
                    Logger.LogError(new ParsingException(sourceCodeFile, ex));

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
            result.SourceCodeFile = sourceCodeFile;

            return result;
        }

        public void ClearCache()
        {
            ClearCacheIfRequired(InitLexer(null).Interpreter, lexerLock, 1, false);
            ClearCacheIfRequired(InitParser(null).Interpreter, parserLock, 1);
        }

        protected ParserRuleContext ParseTokens(CodeFile sourceCodeFile,
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
            ClearCacheIfRequired(parser.Interpreter, parserLock, ClearCacheParserFilesCount);

            return syntaxTree;
        }

        /// <summary>
        /// Converts \r to \r\n.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected virtual string PreprocessText(CodeFile file)
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

        protected void ClearCacheIfRequired(ATNSimulator interpreter, ReaderWriterLockSlim interpreterLock,
            int interpreterFilesCount, bool startGC = true)
        {
            if (processedFilesCount % interpreterFilesCount == 0)
            {
                long memory = Process.GetCurrentProcess().PrivateMemorySize64 / 1000000;
                if (memory > MemoryConsumptionMb)
                {
                    interpreterLock.EnterWriteLock();
                    try
                    {
                        interpreter.ClearDFA();
                        if (startGC)
                        {
                            GC.Collect();
                        }
                    }
                    finally
                    {
                        interpreterLock.ExitWriteLock();
                    }
                }
            }
        }

        protected void IncrementProcessedFilesCount()
        {
            int newValue = Interlocked.Increment(ref processedFilesCount);
            if (newValue == int.MaxValue)
            {
                processedFilesCount = 1;
            }
        }
    }
}
