using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using System.IO;
using PT.PM.Common.Files;

namespace PT.PM.PhpParseTreeUst
{
    public class PhpAntlrParser : AntlrParser
    {
        public override Language Language => Language.Php;

        public override CaseInsensitiveType CaseInsensitiveType => CaseInsensitiveType.lower;

        public static PhpAntlrParser Create() => new PhpAntlrParser();

        public PhpAntlrParser()
        {
        }

        protected override int CommentsChannel => PhpLexer.PhpComments;

        protected override IVocabulary Vocabulary => PhpLexer.DefaultVocabulary;

        protected override Lexer InitLexer(ICharStream inputStream) => new PhpLexer(inputStream);

        protected override Parser InitParser(ITokenStream inputStream) => new PhpParser(inputStream);

        protected override ParserRuleContext Parse(Parser parser) => ((PhpParser)parser).htmlDocument();

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree) =>
            new PhpAntlrParseTree((PhpParser.HtmlDocumentContext)syntaxTree);

        protected override string PreprocessText(TextFile file)
        {
            var result = base.PreprocessText(file);

            bool trimmed = false;
            var vtIndex = result.IndexOf('\v');
            if (vtIndex != -1 && vtIndex > 0 && result[vtIndex - 1] == '\n' &&
                vtIndex + 1 < result.Length && char.IsDigit(result[vtIndex + 1]))
            {
                result = result.Remove(vtIndex);
                trimmed = true;
            }
            // TODO: Fix Hardcode!
            int lastPhpInd = result.LastIndexOf("?>");
            if (lastPhpInd != -1)
            {
                if (lastPhpInd + "?>".Length + 12 <= result.Length &&
                    result.Substring(lastPhpInd + "?>".Length, 12) == "\r\nChangelog:")
                {
                    result = result.Remove(lastPhpInd + "?>".Length);
                    trimmed = true;
                }
            }

            if (trimmed)
            {
                Logger.LogDebug($"File {Path.Combine(file.RelativePath, file.Name)} trimmed.");
            }

            return result;
        }

        protected override string LexerSerializedATN => PhpLexer._serializedATN;

        protected override string ParserSerializedATN => PhpParser._serializedATN;
    }
}
