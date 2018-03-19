dotnet AntlrGen/AntlrGen.dll --lexer Sources/PT.PM.Dsl/DslLexer.g4 --parser Sources/PT.PM.Dsl/DslParser.g4 --package PT.PM.Dsl --output Sources/PT.PM.Dsl/Generated

dotnet AntlrGen/AntlrGen.dll --lexer Sources/antlr-grammars-v4/java/JavaLexer.g4 --parser Sources/antlr-grammars-v4/java/JavaParser.g4 --package PT.PM.JavaParseTreeUst --output Sources/PT.PM.JavaParseTreeUst/Generated

dotnet AntlrGen/AntlrGen.dll --lexer Sources/antlr-grammars-v4/javascript/JavaScriptLexer.g4 --parser Sources/antlr-grammars-v4/javascript/JavaScriptParser.g4 --package PT.PM.JavaScriptParseTreeUst.Parser --output Sources/PT.PM.JavaScriptParseTreeUst/Generated

dotnet AntlrGen/AntlrGen.dll --lexer Sources/antlr-grammars-v4/php/CSharpSharwell/PhpLexer.g4 --parser Sources/antlr-grammars-v4/php/PhpParser.g4 --package PT.PM.PhpParseTreeUst --output Sources/PT.PM.PhpParseTreeUst/Generated

dotnet AntlrGen/AntlrGen.dll --lexer Sources/antlr-grammars-v4/plsql/PlSqlLexer.g4 --parser Sources/antlr-grammars-v4/plsql/PlSqlParser.g4 --package PT.PM.PlSqlParseTreeUst --output Sources/PT.PM.SqlParseTreeUst/Generated

dotnet AntlrGen/AntlrGen.dll --lexer Sources/antlr-grammars-v4/tsql/TSqlLexer.g4 --parser Sources/antlr-grammars-v4/tsql/TSqlParser.g4 --package PT.PM.TSqlParseTreeUst --output Sources/PT.PM.SqlParseTreeUst/Generated
