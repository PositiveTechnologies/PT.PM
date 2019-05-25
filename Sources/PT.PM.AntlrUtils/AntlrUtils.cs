using PT.PM.Common;
using PT.PM.Common.Exceptions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Diagnostics;
using System.Threading;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.AntlrUtils
{
    public static class AntlrUtils
    {
        private static long processedFilesCount;
        private static long processedBytesCount;
        private static long checkNumber;
        private static volatile bool excessMemory;

        public static long CheckCacheMemoryBytes { get; set; } = 2 * 1024 * 1024 * 1024L;

        public static long ClearCacheFilesBytesCount { get; set; } = 5 * 1024 * 1024L;

        public static int ClearCacheFilesCount { get; set; } = 50;

        public static void HandleMemoryConsumption(TextFile antlrTextFile, ILogger logger)
        {
            long localProcessedFilesCount = Interlocked.Increment(ref processedFilesCount);
            long localProcessedBytesCount = Interlocked.Add(ref processedBytesCount, antlrTextFile.Data.Length);

            if (Process.GetCurrentProcess().PrivateMemorySize64 > CheckCacheMemoryBytes)
            {
                long divideResult = localProcessedBytesCount / ClearCacheFilesBytesCount;
                bool exceededProcessedBytes = divideResult > Thread.VolatileRead(ref checkNumber);
                Thread.VolatileWrite(ref checkNumber, divideResult);

                bool prevExcessMemory = excessMemory;
                excessMemory = true;

                if (!prevExcessMemory ||
                    exceededProcessedBytes ||
                    localProcessedFilesCount % ClearCacheFilesCount == 0)
                {
                    lock (AntlrLexer.Atns)
                    {
                        AntlrLexer.Atns.Clear();
                    }
                    lock (AntlrParser.Atns)
                    {
                        AntlrParser.Atns.Clear();
                    }

                    logger?.LogInfo(
                        $"ANTLR cache cleared due to big memory consumption after parsing of {antlrTextFile.RelativeName}.");
                }
            }
            else
            {
                excessMemory = false;
            }
        }

        public static AntlrLexer CreateAntlrLexer(this Language language)
        {
            if (language.CreateLexer() is AntlrLexer antlrLexer)
            {
                return antlrLexer;
            }
            throw new NotImplementedException($"{nameof(AntlrLexer)} for language {language} is not supported");
        }

        public static TextSpan GetTextSpan(this ParserRuleContext ruleContext)
        {
            var start = ruleContext.Start;

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
                result = new TextSpan(start.StartIndex, stop.StopIndex - start.StartIndex);
            }
            else
            {
                result = default;
            }

            return result;
        }

        public static TextSpan GetTextSpan(this ITerminalNode node)
        {
            return GetTextSpan(node.Symbol);
        }

        public static TextSpan GetTextSpan(this IToken token)
        {
            return token is LightToken lightToken
                ? lightToken.TextSpan
                : new TextSpan(token.StartIndex, token.StopIndex - token.StartIndex);
        }

        public static void LogConversionError(this ILogger logger, Exception ex,
            ParserRuleContext context, TextFile currentFileData)
        {
            StackTrace stackTrace = new StackTrace(ex, true);
            int frameNumber = 0;
            string fileName;
            string methodName;
            int line;
            int column;
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

            logger.LogError(new ConversionException(currentFileData, message: exceptionText) { TextSpan = textSpan });
        }


        public static ArgumentExpression ConvertToInOutArgument(this ParserRuleContext context)
        {
            var argModifier = new InOutModifierLiteral(InOutModifier.InOut, TextSpan.Zero);
            TextSpan contextTextSpan = context.GetTextSpan();
            var arg = new IdToken(context.GetText(), contextTextSpan);
            return new ArgumentExpression(argModifier, arg, contextTextSpan);
        }
    }
}
