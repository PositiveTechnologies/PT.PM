using Esprima;
using Esprima.Ast;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using System;
using System.Diagnostics;
using System.Threading;
using PT.PM.Common.Files;
using Collections = System.Collections.Generic;

namespace PT.PM.JavaScriptParseTreeUst
{
    public class JavaScriptEsprimaParser : ILanguageParser<TextFile>
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public JavaScriptType JavaScriptType { get; set; } = JavaScriptType.Undefined;

        public Language Language => Language.JavaScript;

        public int Offset { get; set; }

        public TextFile OriginFile { get; set; }

        public static JavaScriptEsprimaParser Create() => new JavaScriptEsprimaParser();

        public ParseTree Parse(TextFile sourceFile, out TimeSpan timeSpan)
        {
            if (sourceFile.Data == null)
            {
                return null;
            }

            var errorHandler = new JavaScriptEsprimaErrorHandler(sourceFile)
            {
                Logger = Logger,
                Offset = Offset,
                OriginFile = OriginFile
            };
            try
            {
                var parserOptions = new ParserOptions(sourceFile.FullName)
                {
                    Range = true,
                    Comment = true,
                    Tolerant = true,
                    ErrorHandler = errorHandler
                };
                var parser = new JavaScriptParser(sourceFile.Data, parserOptions);

                var stopwatch = Stopwatch.StartNew();
                Program ast = parser.ParseProgram(JavaScriptType == JavaScriptType.Strict);
                stopwatch.Stop();
                timeSpan = stopwatch.Elapsed;

                var scanner = new Scanner(sourceFile.Data, parserOptions);
                errorHandler.Scanner = scanner;
                errorHandler.Logger = DummyLogger.Instance; // Ignore errors on tokenization because of the first stage
                var comments = new Collections.List<Comment>();

                stopwatch.Restart();
                Token token;
                do
                {
                    comments.AddRange(scanner.ScanComments());
                    try
                    {
                        token = scanner.Lex();
                    }
                    catch (Exception ex) when (!(ex is ThreadAbortException))
                    {
                        // TODO: handle the end of the stream without exception
                        var parsingException = ex is ParserException parserException &&
                                               parserException.InnerException is ParsingException innerException
                                               ? innerException
                                               : new ParsingException(sourceFile, ex);
                        Logger.LogError(parsingException);
                        break;
                    }
                }
                while (token.Type != TokenType.EOF);
                stopwatch.Stop();
                TimeSpan lexerTimeSpan = stopwatch.Elapsed; // TODO: store lexer time

                // TODO: comments handling without scanner, https://github.com/sebastienros/esprima-dotnet/issues/39
                var result = new JavaScriptEsprimaParseTree(ast, comments);
                result.SourceFile = sourceFile;

                return result;
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                ParsingException exception = ex is ParserException parserException
                    ? errorHandler.CreateException(parserException.Index, parserException.Message)
                    : ex is ParsingException parsingException
                    ? parsingException
                    : new ParsingException(sourceFile, ex);
                Logger.LogError(exception);
                return null;
            }
        }
    }
}
