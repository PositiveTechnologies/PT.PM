using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrParser : AntlrBaseHandler, ILanguageParser<IList<IToken>>
    {
        protected const int ZeroTokenType = -2;

        public static readonly Dictionary<Language, ATN> Atns = new Dictionary<Language, ATN>();

        public abstract IVocabulary LexerVocabulary { get; }

        public abstract string[] RuleNames { get; }

        public abstract int[] IdentifierTokenTypes { get; }

        public abstract Type[] IdentifierRuleType { get; }

        public abstract int[] StringTokenTypes { get; }

        public abstract int NullTokenType { get; }

        public abstract int TrueTokenType { get; }

        public abstract int FalseTokenType { get; }

        public abstract int ThisTokenType { get; }

        public abstract int DecNumberTokenType { get; }

        public abstract int HexNumberTokenType { get; }

        public abstract int OctNumberTokenType { get; }

        public abstract int BinNumberTokenType { get; }

        public abstract int FloatNumberTokenType { get; }

        public abstract int[] CommentTokenTypes { get; }

        protected abstract string ParserSerializedATN { get; }

        protected abstract Parser InitParser(ITokenStream inputStream);

        protected abstract ParserRuleContext Parse(Parser parser);

        protected abstract AntlrParseTree CreateParseTree(ParserRuleContext parserRuleContext);

        public bool UseFastParseStrategyAtFirst { get; set; } = true;

        public int LineOffset { get; set; }

        public AntlrDumper ParseTreeDumper { get; set; }

        public ParseTree Parse(IList<IToken> tokens, out TimeSpan parserTimeSpan)
        {
            ProcessTokens(tokens, null, out AntlrParseTree parseTree, out _, out parserTimeSpan);
            return parseTree;
        }

        protected void ProcessTokens(IList<IToken> tokens, AntlrListenerConverter antlrListenerConverter,
            out AntlrParseTree parseTree, out RootUst rootUst, out TimeSpan timeSpan)
        {
            parseTree = null;
            rootUst = null;

            if (tokens.Count == 0)
            {
                timeSpan = TimeSpan.Zero;
                return;
            }

            var sourceFile = ((LightToken) tokens.First(token => token is LightToken)).TextFile;

            var errorListener = new AntlrMemoryErrorListener(sourceFile)
            {
                Logger = Logger,
                LineOffset = LineOffset
            };

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var codeTokenSource = new ListTokenSource(tokens)
                {
                    TokenFactory = new LightTokenFactory()
                };
                var codeTokenStream = new CommonTokenStream(codeTokenSource);
                ProcessTokens(errorListener, codeTokenStream, antlrListenerConverter,
                                       out ParserRuleContext parserRuleContext, out rootUst);

                if (antlrListenerConverter == null)
                {
                    var commentTokens = new List<IToken>();

                    foreach (IToken token in tokens)
                    {
                        if (CommentTokenTypes.Contains(token.Type))
                        {
                            commentTokens.Add(token);
                        }
                    }

                    commentTokens.TrimExcess();

                    parseTree = CreateParseTree(parserRuleContext);
                    parseTree.Tokens = tokens;
                    parseTree.Comments = commentTokens;
                    parseTree.SourceFile = ((LightToken)tokens.First(token => token is LightToken)).TextFile;
                }
                else
                {
                    var comments = new List<Comment>();

                    foreach (IToken token in tokens)
                    {
                        if (CommentTokenTypes.Contains(token.Type))
                        {
                            comments.Add(new Comment(token.GetTextSpan(), rootUst));
                        }
                    }

                    rootUst.Comments = comments.ToArray();
                }

                stopwatch.Stop();
                timeSpan = stopwatch.Elapsed;
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ParsingException(sourceFile, ex));
            }
        }

        private void ProcessTokens(
            AntlrMemoryErrorListener errorListener, CommonTokenStream codeTokenStream,
            AntlrListenerConverter antlrListenerConverter,
            out ParserRuleContext parseTree, out RootUst rootUst)
        {
            Parser parser = InitParser(codeTokenStream);
            parser.Interpreter = new ParserATNSimulator(parser, GetOrCreateAtn(ParserSerializedATN));
            parser.TrimParseTree = true;

            if (antlrListenerConverter != null)
            {
                parser.BuildParseTree = ParseTreeDumper != null;
                parser.AddParseListener(antlrListenerConverter);
            }

            parser.RemoveErrorListeners();

            if (UseFastParseStrategyAtFirst)
            {
                parser.Interpreter.PredictionMode = PredictionMode.Sll;
                parser.ErrorHandler = new BailErrorStrategy();

                try
                {
                    parseTree = Parse(parser);
                }
                catch (ParseCanceledException)
                {
                    antlrListenerConverter?.Clear();

                    codeTokenStream.Reset();
                    parser.AddErrorListener(errorListener);
                    parser.Reset();
                    parser.Interpreter.PredictionMode = PredictionMode.Ll;
                    parser.ErrorHandler = new DefaultErrorStrategy();

                    parseTree = Parse(parser);
                }
            }
            else
            {
                parser.AddErrorListener(errorListener);
                parseTree = Parse(parser);
            }

            if (ParseTreeDumper != null)
            {
                TextFile sourceFile = ((LightToken) parseTree.Start).TextFile;
                ParseTreeDumper.DumpTree(parseTree, RuleNames, sourceFile);
            }

            rootUst = antlrListenerConverter?.Complete();
        }
    }
}
