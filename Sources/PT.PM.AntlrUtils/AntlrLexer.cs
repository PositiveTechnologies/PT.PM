using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Files;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrLexer : AntlrBaseHandler, ILanguageLexer
    {
        public static Dictionary<Language, ATN> Atns = new Dictionary<Language, ATN>();

        public virtual CaseInsensitiveType CaseInsensitiveType { get; } = CaseInsensitiveType.None;

        protected abstract string LexerSerializedATN { get; }

        public abstract IVocabulary Vocabulary { get; }

        public int LineOffset { get; set; }

        public abstract Lexer InitLexer(ICharStream inputStream);

        public IList<IToken> GetTokens(TextFile sourceFile, out TimeSpan lexerTimeSpan)
        {
            SourceFile = sourceFile;
            if (ErrorListener == null)
            {
                ErrorListener = new AntlrMemoryErrorListener();
                ErrorListener.Logger = Logger;
                ErrorListener.LineOffset = LineOffset;
            }

            ErrorListener.SourceFile = sourceFile;
            var preprocessedText = PreprocessText(sourceFile);
            var inputStream = new LightInputStream(Vocabulary, sourceFile, preprocessedText, CaseInsensitiveType);

            IList<IToken> tokens;
            try
            {
                var stopwatch = Stopwatch.StartNew();
                Lexer lexer = InitLexer(inputStream);
                lexer.Interpreter = new LexerATNSimulator(lexer, GetOrCreateAtn(LexerSerializedATN));
                lexer.RemoveErrorListeners();
                lexer.AddErrorListener(ErrorListener);
                lexer.TokenFactory = new LightTokenFactory();

                tokens = lexer.GetAllTokens();

                stopwatch.Stop();
                lexerTimeSpan = stopwatch.Elapsed;
            }
            catch (Exception ex)
            {
                Logger.LogError(new LexingException(SourceFile, ex));
                tokens = new List<IToken>();
            }
            finally
            {
                HandleMemoryConsumption();
            }

            return tokens;
        }

        protected virtual string PreprocessText(TextFile file) => file.Data;
    }
}