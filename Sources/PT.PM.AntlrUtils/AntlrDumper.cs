using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PT.PM.Common;
using System.Collections.Generic;
using System.Text;
using PT.PM.Common.Files;

namespace PT.PM.AntlrUtils
{
    public class AntlrDumper : ParseTreeDumper
    {
        public override string ParseTreeSuffix => "parseTree.lisp";

        public void DumpTokens(IList<IToken> tokens, Language language, TextFile sourceFile)
        {
            IVocabulary vocabulary = language.CreateAntlrLexer().Vocabulary;

            var resultString = new StringBuilder();
            foreach (IToken token in tokens)
            {
                if (!OnlyCommonTokens || token.Channel == 0)
                {
                    resultString.Append(RenderToken(token, false, vocabulary));
                    if (EachTokenOnNewLine)
                        resultString.AppendLine();
                    else
                        resultString.Append(" ");
                }
            }
            resultString.Append("EOF");

            Dump(resultString.ToString(), sourceFile, true);
        }

        public override void DumpTree(ParseTree parseTree)
        {
            var result = new StringBuilder();
            string[] ruleNames = ((AntlrParser)parseTree.SourceLanguage.CreateParser()).RuleNames;
            DumpTree(((AntlrParseTree)parseTree).SyntaxTree, ruleNames, result, 0);

            Dump(result.ToString(), parseTree.SourceFile, false);
        }

        private void DumpTree(IParseTree parseTree, string[] ruleNames, StringBuilder builder, int level)
        {
            int currentLevelStringLength = level * IndentSize;
            builder.PadLeft(currentLevelStringLength);
            if (parseTree is RuleContext ruleContext)
            {
                builder.Append(ruleNames[ruleContext.RuleIndex]);
                builder.AppendLine(" (");

                for (int i = 0; i < ruleContext.ChildCount; i++)
                {
                    DumpTree(ruleContext.GetChild(i), ruleNames, builder, level + 1);
                    builder.AppendLine();
                }

                builder.PadLeft(currentLevelStringLength);
                builder.Append(")");
            }
            else
            {
                string text = parseTree.GetText().Replace(@"\", @"\\").Replace(@"'", @"\'")
                    .Replace("\n", "\\n").Replace("\r", "\\r");
                builder.Append('\'');
                builder.Append(text);
                builder.Append('\'');
            }
        }

        private string RenderToken(IToken token, bool showChannel, IVocabulary vocabulary)
        {
            string symbolicName = vocabulary?.GetSymbolicName(token.Type) ?? token.Type.ToString();
            string value = TokenValueDisplayMode == TokenValueDisplayMode.Trim ? token.Text?.Trim() : token.Text;

            string tokenValue = string.Empty;
            if (TokenValueDisplayMode != TokenValueDisplayMode.Ignore && value != null && symbolicName != value)
            {
                tokenValue = value.Length <= MaxTokenValueLength
                    ? value
                    : value.Substring(0, MaxTokenValueLength) + "...";
            }

            string channelValue = string.Empty;
            if (showChannel)
            {
                channelValue = "c: " + token.Channel; // TODO: channel name instead of identifier.
            }

            string textSpan = token is LightToken lightToken
                ? lightToken.LineColumnTextSpan.ToString()
                : token.GetTextSpan().ToString();
            string result =  $"{token.TokenIndex}; {textSpan}; {symbolicName}";
            if (!string.IsNullOrEmpty(tokenValue) || !string.IsNullOrEmpty(channelValue))
            {
                var strings = new List<string>();
                if (!string.IsNullOrEmpty(tokenValue))
                {
                    strings.Add(tokenValue.Replace("\r", "\\r").Replace("\n", "\\n"));
                }

                if (!string.IsNullOrEmpty(channelValue))
                    strings.Add(channelValue);
                result = $"{result} ({string.Join(", ", strings)})";
            }

            return result;
        }
    }
}
