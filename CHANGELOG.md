#### PT.PM 0.9 (07.03.2017)

* **AI.PM** renamed to **PT.PM** (PT Pattern Matching Engine).
* PT.PM became an Open Source project hosted on github: [PT.PM](https://github.com/PositiveTechnologies/PT.PM).
* `UAst` -> `Ust` (Unified Syntax Tree) and other renamings.
* Fixed deserialization of patterns without `Languages` and `DataFormat`.
* Added LICENSE.
* Unused and private code and documents removed.
* Various code improvements and refactorings.
* Dependencies updated.

#### AI.PM 1.4.8 (30.01.2017)

* Improved JavaScript grammar for more performant deep-recursive expressions parsing.
* JavaScript converter:
  * Implemented try-catch, anonymous functions, binary expressions and other stuff handling.
  * Improved terminals processing in `AntlrDefaultVisitor`.
* Improved DSL (for JavaScript):
  * Added Arbitrary depth expression syntax: `<{ expression }>`.
  * Added anonymous function declaration: `function { statements }`.
  * Unused files removed.
* Improved Pattern Editor GUI:
  * Indented ANTLR parse tree.
  * IsExpanded option for Expander now stored in settings.
  * Bugfixes and better UX.
* NUnit upgraded to 3.6.0.
* Removed not used or experimental projects: *CSharpAntlrAstConversion*, *ObjectiveCAstConversion*.
* Improved incorrect languages processing in `StringPatternsRepository`.

#### AI.PM 1.4.7 (16.01.2017)

* New architecture of all UAst ANTLR converters: `BaseVisitor` replaced with own `AntlrDefaultVisitor` which implements  `Visit`, `VisitChildren`, `VisitTerminal` and other methods.
* `UAst` and `IParseTreeToUAstConverter` now have several languages support (`LanguageFlags` instead of single `Language`).
* New JavaScript converter with some unit-tests.
* `$` sign now supported as an identifier name in DSL (used in JavaScript patterns).
* Correct multiple languages parsing and converting inside one file (JavaScript in PHP).
* Renaming: `IdLiteral` -> `IdToken` and other.
* Much code (including PHP grammars) has been refactored and cleared.
* Fixed default log path.

#### AI.PM 1.4.6 (29.12.2016)

* ANTLR grammars synchonization with grammars from an official repository: [grammars-v4](https://github.com/antlr/grammars-v4/pull/529).
* Support of multiple languages for directories processing.
* Language detection included in Workflow.
* Better parallel tests running for pipes.
* Added `--log-debugs` command line parameter for debug messages logging in RELEASE mode.
* Some code removed, refactored and simplified.

#### AI.PM 1.4.5 (25.11.2016)

* Async mode for pipes.
* Aspx files processing fixed.
* NUnit upgrade:  2.6.4 -> 3.5.0.
* Renaming: Ast -> ParseTree. 
* Enum micro-optimizations.

#### AI.PM 1.4.4 (18.11.2016)

* Integration with Aspx parser:
  * Added [AspxParser](https://github.com/PositiveTechnologies/AspxParser) submodule.
  * Added `Aspx` Language, implemented Aspx Pattern Matching.
* Aspx parser submodule:
  * Linear TextSpan instead of LineColumn.
  * System.Collection.Immutable: 1.1.36.0 -> 1.1.37.0 (fix for CI).
* Pipe:
  * Single thread for pipe mode (`GetMatchings` command).
  * Added `DetectLanguage` command.
* Common:
  * AI.PM.Prebuild moved to a common AI.PM.sln solution: "One-click" build.
  * AI.PM.Prebuild.sln removed.
  * Updated docs with respect to pipe commands.

#### AI.PM 1.4.3 (31.10.2016)

* Linear TextSpan everywhere.
* Bugfix (`instanceof` handling in Java).
* First merge with ai.taint branch (proof of concept).
* Refactoring & renaming (Ast -> UAst, AstNode -> UAstNode).
* Added `--pipe-mode` for working with AI.PM without process restart:
  * CheckPattern command.
  * GetMatchings command.
* Added `--log-errors` parameter.
* Fixed CI pipe testing.
* Updated ai.pm_protocol.md.
* Improved test names.

#### AI.PM 1.4.2 (10.10.2016)

* Initial version of Pattern Editor for Approof.
* Added `--log-path` parameter to AI.PM.Console.
* Improved repository structure and build process.
* Small fixes & improvements.
* Added translated to English publications.
* Added webinars stuff (presentation and text).
* Updated texts, fixed errors & typos.

#### AI.PM 1.4.1 (29.07.2016)

* Improved CaseInsensitive handling.
* Removed useless comments.
* Languages for patterns restored in PatternEditor.
* Fixed errors.

#### AI.PM 1.4.0 (23.07.2016)

* Fixed incorrect text position for comments.
* Fixed pattern matching with negation (`new XMLUtil().parse(<[~"\w*"]>)`).
* Fixed errors.
* Removed DEBUG messages from log.

#### AI.PM 1.3.9 (24.06.2016)

* Fixed ANTLR parsers huge memory consumption (Java, PHP, T-SQL, PL/SQL).
* Improved PHP real numbers handling.
* Fixed base64 escaping ('@' instead of '/').
* Added english publication about theory and practice of source code parsing.

#### AI.PM 1.3.8 (09.06.2016)

* Added string interpolation support to PHP parser.
* Improved PHP performance for deep expressions.
* Improved PHP error handling.
* PHP grammar simplified.
* Added AntlrCaseInsensitiveInputStream for case insensitive languages.

#### AI.PM 1.3.7 (26.05.2016)

* Fixed CREATE PROCEDURE T-SQL block.
* Improved TextSpan detection for multiline pattern.
* Added PM icons. 

#### AI.PM 1.3.6 (20.05.2016)

* AI.PM.Prebuild is able to generate Listener instead of Visitor.
* PHP parser error correction improved.
* Code refactored.
* Removed useless files.
* Added topics "Обработка древовидных структур и унифицированное AST" and "Язык шаблонов для универсального сигнатурного анализатора кода".

#### AI.PM 1.3.5 (20.04.2016)

* DSL syntax improved. 
* Added SQL patterns with unclosed cursor, file and other.
* Fixed tsql pattern matching.
* New format for Pattern with AI.UI interaction: Language -> Languages.

#### AI.PM 1.3.4 (11.04.2016)

* Implemented PL/SQL Converter for some nodes (for matching with tests samples).
* Added PL/SQL & T-SQL sample code from "HP Enterprise Security" for unit tests.
* Improved Language Autodetection (especially important for PL/SQL & T-SQL  because of same file extension).
* Improved PatternEditor GUI with PL/SQL & T-SQL support and detail info about parsing.
* Fixed preprocessing for handling 1/0 and integer overflow exceptions.
* Bug fixing & refactoring.

#### AI.PM 1.3.3 (28.03.2016)

* Initial version of T-SQL converter.

#### AI.PM 1.3.2 (17.03.2016)

* Added topic "Теория и практика парсинга исходников с помощью ANTLR и Roslyn.md".

#### AI.PM 1.3.1 (03.03.2016)

* ANTLR lexing performance improved.
* Roslyn errors detection improved.
* Roslyn & ANTLR updated.
* `ZipAtUrlCachedCodeRepository` now scans all urls for source grabbing.
* Improved statistics output (antlr lexing & parsing time, number of files, lines and chars).
* C# ANTLR grammar with tests for topic "Теория и практика парсинга исходников с помощью ANTLR и Roslyn.md".
* PLSQL & TSQL grammars with tests added.
* Code refactored & cleared.

#### AI.PM 1.3.0 (24.02.2016)

* Mopas Tests Passing automation (from xlsx table).
* New GUI (Perspex-based) for Pattern Editing (obfuscation and compilation to only one .exe).
* Old GUI (WindowsForms-based) for Pattern Edititng has been removed.
* Implemented automatic language detection for part of source code.
* DSL IfStatement support.
* Improved node comparison methods.

#### AI.PM 1.2.9 (27.01.2016)

* Constant folding for integer types and concatenation for strings.
* Added `AstVisitor` for AST preprocessing (based on reflection and dynamic type).
* Added `PatternExpressionInsideStatement` for matching cases described in PatternMatchingTests (expression inside expressions):
```
<[@pwd:password2]> = #;
~<[@pwd]>.Length;
#(#*, <[@pwd]>, #*);
```
will be match such code:
```
$password2 = "1234"
if ($password2->Length > 0) { }
test_call_5(1, $password2, 2);
```
* Several values for pinned vars (They can be defined by such way: `<[@var: "" || null]>`).
* Fixed numbers in different scales parsing and other issues in PHP.
* Json serialization output cleared.
* Improved `CompareTo` methods for node comparisons.
* Fixed line numbering for PHP files.

#### AI.PM 1.2.8, DSL Syntax Improvements & Extending (18.01.2016)

* Octal & Hex digits support.
* New syntax for named vars (Now they start with "at" char: @var).
* Upper bound excluding for ranges (0..2048 is not include 2048).
* Added #* as multiply expressions in function calls. Now it is possible to use the following patterns:
  * `set_cookie_params(<[3600..]>, #*)`
  * `#(#*, <[@pwd]>, #*)`
* Improved `.ToString()` methods for common and pattern AST nodes for better debug.

#### AI.PM 1.2.7 (03.12.2015)

* Fixed Obfuscation.
* Fixed bugs with incorrect position in source code for matched patterns.
* Improved and fixed multiline pattern support:
```
Cookie <[cookie]> = new Cookie(...);
...
~<[cookie]>.setSecure(true);
...
#.addCookie(<[cookie]>);
```

#### AI.PM 1.2.6 (26.11.2015)

* Major version number incrementation.
* Extension-based content detection (.cs, .java, .php).
* Improved logging for AI (log files removing before scan and info appending during scan).
* Added tests ignore messages.

#### AI.PM 0.2.5 (19.11.2015)

Various fixes from merge requests:
* Implemented Pattern Editor Gui.
* Fixed dsl: added concatenetion, indexer, try catch.
* Added php aspTags support.
* Refactored tests data (moved from TestsData to projects).
* Added Sql parser project (ANTLR).
* Fixed CI utils (build-on-server.cmd, tests-on-server.cmd).
* Fixed tests running.
* Fixed php bugs converting.
* CI checking (unit tests running).

#### AI.PM 0.0.0 (17.06.2015)

* Project started.