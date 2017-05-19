using PT.PM.Common;
using PT.PM.Common.Exceptions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Antlr4.Runtime.Misc;

namespace PT.PM.AntlrUtils
{
    public static class AntlrHelper
    {
        private const int IndentSize = 2;
        private const int MaxTokenValueLength = 16;

        public static string GetText(this ParserRuleContext ruleContext, IList<IToken> tokens)
        {
            if (tokens == null)
                return ruleContext.GetText();

            var result = new StringBuilder();
            Interval interval = ruleContext.SourceInterval;
            for (int i = interval.a; i <= interval.b; i++)
            {
                result.Append(tokens[i].Text);
            }
            return result.ToString();
        }

        public static TextSpan GetTextSpan(this ParserRuleContext ruleContext)
        {
            var start = ruleContext.Start;
            if (start.Text == "<EOF>")
                return default(TextSpan);

            IToken stop = ruleContext.Stop;
            RuleContext parent = ruleContext.Parent;
            while (stop == null && parent != null)
            {
                var parentParserRuleContext = parent as ParserRuleContext;
                if (parentParserRuleContext != null)
                {
                    stop = parentParserRuleContext.Stop;
                }
                parent = parent.Parent;
            }

            TextSpan result;
            if (stop != null && stop.StopIndex >= start.StartIndex)
            {
                result = new TextSpan(start.StartIndex, stop.StopIndex - start.StartIndex + 1);
            }
            else
            {
                result = default(TextSpan);
            }
            return result;
        }

        public static TextSpan GetTextSpan(this ITerminalNode node)
        {
            return GetTextSpan(node.Symbol);
        }

        public static TextSpan GetTextSpan(this IToken token)
        {
            var result = new TextSpan(token.StartIndex, token.StopIndex - token.StartIndex + 1);
            return result;
        }

        public static string GetTokensString(this IList<IToken> tokens, IVocabulary vocabulary,
            TokenValueDisplayMode tokenValueDisplayMode = TokenValueDisplayMode.Show, bool onlyDefaultChannel = false,
            bool eachTokenOnNewLine = true)
        {
            var resultString = new StringBuilder();
            foreach (var token in tokens)
            {
                if (!onlyDefaultChannel || token.Channel == 0)
                {
                    resultString.Append(RenderToken(token, TokenValueDisplayMode.Show, false, vocabulary));
                    if (eachTokenOnNewLine)
                        resultString.AppendLine();
                    else
                        resultString.Append(" ");
                }
            }
            resultString.Append("EOF");

            return resultString.ToString();
        }

        public static string ToStringTreeIndented(this IParseTree parseTree, Parser parser, bool eachTokenOnNewLine = true)
        {
            var result = new StringBuilder();
            parseTree.ToStringTreeIndented(parser, result, 0);
            return result.ToString();
        }

        private static void ToStringTreeIndented(this IParseTree parseTree, Parser parser, StringBuilder builder, int level)
        {
            int currentLevelStringLength = level * IndentSize;
            builder.PadLeft(currentLevelStringLength);
            var ruleContext = parseTree as RuleContext;
            if (ruleContext != null)
            {
                builder.AppendLine("(");
                builder.PadLeft(currentLevelStringLength);
                builder.AppendLine(parser.RuleNames[ruleContext.RuleIndex]);

                for (int i = 0; i < ruleContext.ChildCount; i++)
                {
                    ruleContext.GetChild(i).ToStringTreeIndented(parser, builder, level + 1);
                    builder.AppendLine();
                }
                
                builder.PadLeft(currentLevelStringLength);
                builder.Append(")");
            }
            else
            {
                builder.Append('\'' + parseTree.GetText() + '\''); // TODO: replace with RenderToken.
            }
        }

        public static void LogConversionError(Exception ex, ParserRuleContext context,
            string currentFileName, string currentFileData, ILogger logger)
        {
            StackTrace stackTrace = new StackTrace(ex, true);
            int frameNumber = 0;
            string fileName = null;
            string methodName = null;
            int line = 0;
            int column = 0;
            do
            {
                StackFrame frame = stackTrace.GetFrame(frameNumber);
                fileName = frame.GetFileName();
                methodName = frame.GetMethod().Name;
                line = frame.GetFileLineNumber();
                column = frame.GetFileColumnNumber();
                frameNumber++;
            }
            while (frameNumber < stackTrace.FrameCount && (fileName == null || methodName == "Visit"));

            var textSpan = context.GetTextSpan();
            string exceptionText;
            int sourceCodeLine, sourceCodeColumn;
            TextHelper.LinearToLineColumn(textSpan.Start, currentFileData, out sourceCodeLine, out sourceCodeColumn);
            if (fileName != null)
            {
                exceptionText = $"{ex.Message} at method \"{methodName}\" {line}:{column} at position {sourceCodeLine}:{sourceCodeColumn} in source file";
            }
            else
            {
                exceptionText = $"{ex.Message} at position {sourceCodeLine}:{sourceCodeColumn} in source file";
            }
            logger.LogError(new ConversionException(currentFileName, message: exceptionText) { TextSpan = textSpan });
        }

        private static string RenderToken(IToken token, TokenValueDisplayMode tokenValueDisplayMode = TokenValueDisplayMode.Show, bool showChannel = false, IVocabulary vocabulary = null)
        {
            string symbolicName = vocabulary?.GetSymbolicName(token.Type) ?? token.Type.ToString();
            string value = tokenValueDisplayMode == TokenValueDisplayMode.Trim ? token.Text?.Trim() : token.Text;

            string tokenValue = string.Empty;
            if (tokenValueDisplayMode != TokenValueDisplayMode.Ignore && value != null && symbolicName != value)
            {
                tokenValue = value.Length <= MaxTokenValueLength
                    ? value
                    : value.Substring(0, MaxTokenValueLength) + "...";
            }

            string channelValue = string.Empty;
            if (showChannel)
            {
                channelValue = "c: " + token.Channel.ToString(); // TODO: channel name instead of identifier.
            }

            string result = symbolicName;
            if (!string.IsNullOrEmpty(tokenValue) || !string.IsNullOrEmpty(channelValue))
            {
                var strings = new List<string>();
                if (!string.IsNullOrEmpty(tokenValue))
                    strings.Add(tokenValue);
                if (!string.IsNullOrEmpty(channelValue))
                    strings.Add(channelValue);
                result = $"{result} ({(string.Join(", ", strings))})";
            }

            return result;
        }

        private static void PadLeft(this StringBuilder builder, int totalWidth)
        {
            builder.Append(' ', totalWidth);
        }
    }
}
