using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrParserBase : AntlrBaseHandler
    {
        protected const int ZeroTokenType = -2;

        public static readonly Dictionary<Language, ATN> Atns = new Dictionary<Language, ATN>();

        protected abstract bool IsParseTreeFree { get; }

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

        public bool UseFastParseStrategyAtFirst { get; set; } = true;

        public int LineOffset { get; set; }

        public AntlrDumper ParseTreeDumper { get; set; }

        protected object ProcessTokens(IList<IToken> tokens, AntlrListenerConverter antlrListenerConverter,
            out List<IToken> commentTokens, out TimeSpan timeSpan)
        {
            commentTokens = new List<IToken>();

            if (tokens.Count == 0)
            {
                timeSpan = TimeSpan.Zero;
                return null;
            }

            var sourceFile = ((LightToken) tokens[0]).TextFile;

            var errorListener = new AntlrMemoryErrorListener(sourceFile)
            {
                Logger = Logger,
                LineOffset = LineOffset
            };

            object result = null;
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var codeTokenSource = new ListTokenSource(tokens)
                {
                    TokenFactory = new LightTokenFactory()
                };
                var codeTokenStream = new CommonTokenStream(codeTokenSource);
                result = ProcessTokens(errorListener, codeTokenStream, antlrListenerConverter);

                if (!IsParseTreeFree)
                {
                    foreach (IToken token in tokens)
                    {
                        if (CommentTokenTypes.Contains(token.Type))
                        {
                            commentTokens.Add(token);
                        }
                    }
                }
                else
                {
                    var rootUst = (RootUst) result;
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

            return result;
        }

        private object ProcessTokens(
            AntlrMemoryErrorListener errorListener, CommonTokenStream codeTokenStream,
            AntlrListenerConverter antlrListenerConverter)
        {
            Parser parser = InitParser(codeTokenStream);
            parser.Interpreter = new ParserATNSimulator(parser, GetOrCreateAtn(ParserSerializedATN));

            if (IsParseTreeFree)
            {
                parser.BuildParseTree = ParseTreeDumper != null;
                if (antlrListenerConverter != null)
                {
                    parser.AddParseListener(antlrListenerConverter);
                }
            }
            else
            {
                parser.TrimParseTree = true;
            }

            parser.RemoveErrorListeners();
            ParserRuleContext parseTree;

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
                ParseTreeDumper?.DumpTree(parseTree, RuleNames, sourceFile);
            }

            return IsParseTreeFree ? (object)antlrListenerConverter?.Complete() : parseTree;
        }
    }
}
