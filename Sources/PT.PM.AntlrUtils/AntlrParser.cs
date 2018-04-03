using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrParser : ILanguageParser
    {
        private static long processedFilesCount = 0;
        private static long processedBytesCount = 0;
        private static long lexerCheckNumber = 0;
        private static long parserCheckNumber = 0;

        private static ConcurrentDictionary<Language, ATN> lexerAtns = new ConcurrentDictionary<Language, ATN>();

        private static ConcurrentDictionary<Language, ATN> parserAtns = new ConcurrentDictionary<Language, ATN>();

        public static ILogger StaticLogger { get; set; } = DummyLogger.Instance;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Lexer Lexer { get; private set; }

        public Parser Parser { get; private set; }

        public abstract Language Language { get; }

        public virtual CaseInsensitiveType CaseInsensitiveType { get; } = CaseInsensitiveType.None;

        public bool UseFastParseStrategyAtFirst { get; set; } = true;

        public static int ClearCacheLexerFilesBytes { get; set; } = 40 * 1000 * 1000;

        public static int ClearCacheParserFilesBytes { get; set; } = 20 * 1000 * 1000;

        public static int ClearCacheLexerFilesCount { get; set; } = 80;

        public static int ClearCacheParserFilesCount { get; set; } = 40;

        public static long MemoryConsumptionBytes { get; set; } = 500 * 1000 * 1000;

        protected abstract Lexer InitLexer(ICharStream inputStream);

        protected abstract Parser InitParser(ITokenStream inputStream);

        protected abstract ParserRuleContext Parse(Parser parser);

        protected abstract AntlrParseTree Create(ParserRuleContext syntaxTree);

        protected abstract IVocabulary Vocabulary { get; }

        protected abstract int CommentsChannel { get; }

        protected abstract string LexerSerializedATN { get; }

        protected abstract string ParserSerializedATN { get; }

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
                    lexer.Interpreter = new LexerATNSimulator(lexer, GetOrCreateAtn(true));
                    lexer.RemoveErrorListeners();
                    lexer.AddErrorListener(errorListener);
                    var commentTokens = new List<IToken>();

                    var stopwatch = Stopwatch.StartNew();
                    IList<IToken> tokens = lexer.GetAllTokens();
                    stopwatch.Stop();
                    long lexerTimeSpanTicks = stopwatch.ElapsedTicks;

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
                    ParserRuleContext syntaxTree = ParseTokens(sourceCodeFile, errorListener, codeTokenStream);
                    stopwatch.Stop();
                    long parserTimeSpanTicks = stopwatch.ElapsedTicks;

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
                finally
                {
                    Interlocked.Increment(ref processedFilesCount);
                    Interlocked.Add(ref processedBytesCount, sourceCodeFile.Code.Length);
                }
            }
            else
            {
                result = Create(null);
            }
            result.SourceCodeFile = sourceCodeFile;

            return result;
        }

        public static void ClearCacheIfRequired()
        {
            ClearCacheIfRequired(false);
            ClearCacheIfRequired(true);
        }

        protected ParserRuleContext ParseTokens(CodeFile sourceCodeFile,
            AntlrMemoryErrorListener errorListener, BufferedTokenStream codeTokenStream,
            Func<ITokenStream, Parser> initParserFunc = null, Func<Parser, ParserRuleContext> parseFunc = null)
        {
            Parser parser = initParserFunc != null ? initParserFunc(codeTokenStream) : InitParser(codeTokenStream);
            parser.Interpreter = new ParserATNSimulator(parser, GetOrCreateAtn(false));
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

        protected static void ClearCacheIfRequired(bool lexer)
        {
            long bytesCount = lexer ? ClearCacheLexerFilesBytes : ClearCacheParserFilesBytes;
            long processedCheckNumber = lexer ? lexerCheckNumber : parserCheckNumber;
            long filesCount = lexer ? ClearCacheLexerFilesCount : ClearCacheParserFilesCount;

            long checkNumber = bytesCount != 0
                ? processedBytesCount / bytesCount
                : processedCheckNumber + 1;

            if (bytesCount == 0 ||
                checkNumber > processedCheckNumber ||
                processedFilesCount % filesCount == 0)
            {
                if (lexer)
                {
                    lexerCheckNumber = checkNumber;
                }
                else
                {
                    parserCheckNumber = checkNumber;
                }

                long memory = Process.GetCurrentProcess().PrivateMemorySize64;
                if (memory > MemoryConsumptionBytes)
                {
                    //GetOrCreateAtn(lexer, true);
                    ConcurrentDictionary<Language, ATN> atns = lexer ? lexerAtns : parserAtns;
                    if (atns.Count > 0)
                    {
                        StaticLogger.LogDebug("Garbarage collection started...");
                        atns.Clear();
                        GC.Collect();
                        StaticLogger.LogDebug("Garbarage collection completed.");
                    }
                }
            }
        }

        private ATN GetOrCreateAtn(bool lexer, bool clear = false)
        {
            ATN atn;
            ConcurrentDictionary<Language, ATN> atns = lexer ? lexerAtns : parserAtns;

            if (!atns.TryGetValue(Language, out atn) || clear)
            {
                string stringAtn = lexer ? LexerSerializedATN : ParserSerializedATN;
                atn = new ATNDeserializer().Deserialize(stringAtn.ToCharArray());
                atns[Language] = atn;
            }

            return atn;
        }
    }
}
