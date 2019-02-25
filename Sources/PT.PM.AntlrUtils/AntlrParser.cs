using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using PT.PM.Common.Files;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrParser : AntlrBaseHandler, ILanguageParser<IEnumerable<IToken>>
    {
        public static Dictionary<Language, ATN> Atns = new Dictionary<Language, ATN>();
        
        public Parser Parser { get; private set; }

        public AntlrLexer Lexer { get; set; }

        protected abstract string ParserSerializedATN { get; }

        protected abstract int CommentsChannel { get; }

        protected abstract Parser InitParser(ITokenStream inputStream);

        public abstract AntlrLexer InitAntlrLexer();

        protected abstract ParserRuleContext Parse(Parser parser);

        protected abstract AntlrParseTree Create(ParserRuleContext syntaxTree);

        public int LineOffset { get; set; }

        public AntlrParser()
        {
            Parser = InitParser(null);
            Lexer = InitAntlrLexer();
        }

        public ParseTree Parse(IEnumerable<IToken> tokens)
        {
            var tokensList = tokens.ToList();
            if (!tokensList.Any())
            {
                return null;
            }
            
            if (SourceFile == null)
            {
                throw new ArgumentNullException(nameof(SourceFile));   
            }
            
            if (ErrorListener == null)
            {
                ErrorListener = new AntlrMemoryErrorListener();
                ErrorListener.Logger = Logger;
                ErrorListener.LineOffset = LineOffset;
            }
            
            ErrorListener.SourceFile = SourceFile;
            
            AntlrParseTree result = null;
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var lexerTimeSpan = stopwatch.Elapsed;

                var commentTokens = new List<IToken>();

                foreach (IToken token in tokensList)
                {
                    if (token.Channel == CommentsChannel)
                    {
                        commentTokens.Add(token);
                    }
                }

                stopwatch.Restart();
                var codeTokenSource = new ListTokenSource(tokensList);
                var codeTokenStream = new CommonTokenStream(codeTokenSource);
                ParserRuleContext syntaxTree = ParseTokens(SourceFile, ErrorListener, codeTokenStream);
                stopwatch.Stop();
                TimeSpan parserTimeSpan = stopwatch.Elapsed;

                result = Create(syntaxTree);
                result.LexerTimeSpan = lexerTimeSpan;
                result.ParserTimeSpan = parserTimeSpan;

                result.Tokens = tokensList;
                result.Comments = commentTokens;
                result.SourceFile = SourceFile;
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ParsingException(SourceFile, ex));
            }
            finally
            {
                HandleMemoryConsumption(Atns);
            }

            return result;
        }

        protected ParserRuleContext ParseTokens(TextFile sourceFile,
            AntlrMemoryErrorListener errorListener, BufferedTokenStream codeTokenStream,
            Func<ITokenStream, Parser> initParserFunc = null, Func<Parser, ParserRuleContext> parseFunc = null)
        {
            Parser parser = initParserFunc != null ? initParserFunc(codeTokenStream) : InitParser(codeTokenStream);
            parser.Interpreter = new ParserATNSimulator(parser, this.GetOrCreateAtn(ParserSerializedATN));
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