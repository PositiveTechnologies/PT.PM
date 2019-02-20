using System;
using System.Collections.Generic;
using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using PT.PM.Common;
using PT.PM.Common.Files;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrLexer : AntlrBaseHandler
    {
        public Lexer Lexer { get; protected set; }

        public virtual CaseInsensitiveType CaseInsensitiveType { get; } = CaseInsensitiveType.None;

        protected abstract string LexerSerializedATN { get; }

        protected abstract IVocabulary Vocabulary { get; }

        public int LineOffset { get; set; }

        public abstract Lexer InitLexer(ICharStream inputStream);

        public IList<IToken> GetTokens(TextFile sourceFile, IAntlrErrorListener<int> errorListener)
        {
            var preprocessedText = PreprocessText(sourceFile);
            AntlrInputStream inputStream;
            if (Language.IsCaseInsensitive())
            {
                inputStream = new AntlrCaseInsensitiveInputStream(preprocessedText, CaseInsensitiveType);
            }
            else
            {
                inputStream = new AntlrInputStream(preprocessedText);
            }

            inputStream.name = sourceFile.RelativeName;

            Lexer lexer = InitLexer(inputStream);
            lexer.Interpreter = new LexerATNSimulator(lexer, GetOrCreateAtn(true, LexerSerializedATN));
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(errorListener);
            var tokens = lexer.GetAllTokens();

            return tokens;
        }

        public AntlrLexer()
        {
            Lexer = InitLexer(null);
        }
    }
}