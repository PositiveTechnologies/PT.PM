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
    public static class AntlrUtils
    {
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
                if (parent is ParserRuleContext parentParserRuleContext)
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

        public static void LogConversionError(this ILogger logger, Exception ex,
            ParserRuleContext context, SourceCodeFile currentFileData)
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
            LineColumnTextSpan lineColumnTextSpan = currentFileData.GetLineColumnTextSpan(textSpan);
            if (fileName != null)
            {
                exceptionText = $"{ex.Message} at method \"{methodName}\" {line}:{column} at position {lineColumnTextSpan.BeginLine}:{lineColumnTextSpan.BeginColumn} in source file";
            }
            else
            {
                exceptionText = $"{ex.Message} at position {lineColumnTextSpan.BeginLine}:{lineColumnTextSpan.BeginColumn} in source file";
            }
            logger.LogError(new ConversionException(currentFileData.RelativeName, message: exceptionText) { TextSpan = textSpan });
        }
    }
}
