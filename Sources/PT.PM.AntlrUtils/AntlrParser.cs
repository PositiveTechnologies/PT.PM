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

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrParser : ILanguageParser
    {
        private static long processedFilesCount = 0;
        private static long processedBytesCount = 0;
        private static long checkNumber = 0;
        private static volatile bool excessMemory = false;

        private static Dictionary<Language, ATN> lexerAtns = new Dictionary<Language, ATN>();

        private static Dictionary<Language, ATN> parserAtns = new Dictionary<Language, ATN>();

        public static ILogger StaticLogger { get; set; } = DummyLogger.Instance;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Lexer Lexer { get; private set; }

        public Parser Parser { get; private set; }

        public abstract Language Language { get; }

        public virtual bool IsActive => true;

        public virtual CaseInsensitiveType CaseInsensitiveType { get; } = CaseInsensitiveType.None;

        public bool UseFastParseStrategyAtFirst { get; set; } = true;

        public static long MemoryConsumptionBytes { get; set; } = 3 * 1024 * 1024 * 1024L;

        public static long ClearCacheFilesBytes { get; set; } = 5 * 1024 * 1024L;

        public static int ClearCacheFilesCount { get; set; } = 50;

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
            if (sourceCodeFile.Code == null)
            {
                return null;
            }

            AntlrParseTree result = null;
            var filePath = sourceCodeFile.RelativeName;
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
                TimeSpan lexerTimeSpan = stopwatch.Elapsed;

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
                TimeSpan parserTimeSpan = stopwatch.Elapsed;

                result = Create(syntaxTree);
                result.LexerTimeSpan = lexerTimeSpan;
                result.ParserTimeSpan = parserTimeSpan;

                result.Tokens = tokens;
                result.Comments = commentTokens;
                result.SourceCodeFile = sourceCodeFile;
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ParsingException(sourceCodeFile, ex));
            }
            finally
            {
                long localProcessedFilesCount = Interlocked.Increment(ref processedFilesCount);
                long localProcessedBytesCount = Interlocked.Add(ref processedBytesCount, sourceCodeFile.Code.Length);

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

                        Logger.LogInfo($"Memory cleared due to big memory consumption during {sourceCodeFile.RelativeName} parsing.");
                    }
                }
                else
                {
                    excessMemory = false;
                }
            }

            return result;
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

        private ATN GetOrCreateAtn(bool lexer)
        {
            ATN atn;
            Dictionary<Language, ATN> atns = lexer ? lexerAtns : parserAtns;

            lock (atns)
            {
                if (!atns.TryGetValue(Language, out atn))
                {
                    string stringAtn = lexer ? LexerSerializedATN : ParserSerializedATN;
                    atn = new ATNDeserializer().Deserialize(stringAtn.ToCharArray());
                    atns.Add(Language, atn);
                    Logger.LogDebug($"New ATN initialized for {Language.Key} {(lexer ? "lexer" : "parser")}.");
                }
            }

            return atn;
        }
    }
}
