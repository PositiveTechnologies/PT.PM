using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Files;
using PT.PM.LanguageDetectors;

namespace PT.PM
{
    public class SqlDialectsAntlrLexer : AntlrLexer
    {
        public static int MaxCharsCount { get; set; } = 50000;

        public override Language Language => Language.Sql;

        public override CaseInsensitiveType CaseInsensitiveType => CaseInsensitiveType.UPPER;

        protected override string LexerSerializedATN => SqlDialectsLexer._serializedATN;

        public override IVocabulary Vocabulary => SqlDialectsLexer.DefaultVocabulary;

        public override Lexer InitLexer(ICharStream inputStream) => new SqlDialectsLexer(inputStream);

        protected override string PreprocessText(TextFile file)
        {
            string data = file.Data;
            string shortenedData;

            if (data.Length > MaxCharsCount)
            {
                // Remove trailing not spaces
                int lastInd = MaxCharsCount - 1;
                while (lastInd > 0 && !char.IsWhiteSpace(data[lastInd]))
                {
                    lastInd--;
                }

                shortenedData = data.Remove(lastInd + 1);
            }
            else
            {
                shortenedData = data;
            }

            return base.PreprocessText(new TextFile(shortenedData));
        }
    }
}
