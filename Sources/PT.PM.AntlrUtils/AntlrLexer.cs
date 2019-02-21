using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using PT.PM.Common;
using PT.PM.Common.Files;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrLexer : AntlrBaseHandler
    {
        public static Dictionary<Language, ATN> Atns = new Dictionary<Language, ATN>();

        public Lexer Lexer { get; protected set; }

        public virtual CaseInsensitiveType CaseInsensitiveType { get; } = CaseInsensitiveType.None;

        protected abstract string LexerSerializedATN { get; }

        protected abstract IVocabulary Vocabulary { get; }

        public int LineOffset { get; set; }

        public abstract Lexer InitLexer(ICharStream inputStream);

        public AntlrLexer()
        {
            Lexer = InitLexer(null);
        }

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
            lexer.Interpreter = new LexerATNSimulator(lexer, this.GetOrCreateAtn(LexerSerializedATN));
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(errorListener);
            var tokens = lexer.GetAllTokens();

            return tokens;
        }

        /// <summary>
        /// Converts \r to \r\n.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected virtual string PreprocessText(TextFile file)
        {
            var text = file.Data;
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
    }
}