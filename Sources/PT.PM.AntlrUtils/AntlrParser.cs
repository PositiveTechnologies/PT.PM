using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrParser : AntlrBaseHandler, ILanguageParser<IList<IToken>>
    {
        public static readonly Dictionary<Language, ATN> Atns = new Dictionary<Language, ATN>();

        public abstract string[] RuleNames { get; }

        protected abstract string ParserSerializedATN { get; }

        protected abstract int CommentsChannel { get; }

        protected abstract Parser InitParser(ITokenStream inputStream);

        protected abstract ParserRuleContext Parse(Parser parser);

        protected abstract AntlrParseTree Create(ParserRuleContext syntaxTree);

        public int LineOffset { get; set; }

        public ParseTree Parse(IList<IToken> tokens, out TimeSpan parserTimeSpan)
        {
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
                var commentTokens = new List<IToken>();

                foreach (IToken token in tokens)
                {
                    if (token.Channel == CommentsChannel)
                    {
                        commentTokens.Add(token);
                    }
                }

                var stopwatch = Stopwatch.StartNew();
                var codeTokenSource = new ListTokenSource(tokens);
                var codeTokenStream = new CommonTokenStream(codeTokenSource);
                ParserRuleContext syntaxTree = ParseTokens(ErrorListener, codeTokenStream);
                stopwatch.Stop();
                parserTimeSpan = stopwatch.Elapsed;

                result = Create(syntaxTree);

                result.Tokens = tokens;
                result.Comments = commentTokens;
                result.SourceFile = SourceFile;
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ParsingException(SourceFile, ex));
            }
            finally
            {
                HandleMemoryConsumption();
            }

            return result;
        }

        private ParserRuleContext ParseTokens(
            AntlrMemoryErrorListener errorListener, BufferedTokenStream codeTokenStream,
            Func<ITokenStream, Parser> initParserFunc = null, Func<Parser, ParserRuleContext> parseFunc = null)
        {
            Parser parser = initParserFunc != null ? initParserFunc(codeTokenStream) : InitParser(codeTokenStream);
            parser.Interpreter = new ParserATNSimulator(parser, GetOrCreateAtn(ParserSerializedATN));
            parser.RemoveErrorListeners();
            ParserRuleContext syntaxTree;

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