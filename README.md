# PT.PM (PT Pattern Matching Engine)

**PT Pattern Matching Engine**, or **PT.PM,** is an engine for searching patterns in the source code.
PT.PM is based on Unified Abstract Syntax Tree ([UAST](https://en.wikipedia.org/wiki/Abstract_syntax_tree#Unified_AST),
UST). At present time C\#, Java, PHP, PL/SQL, T-SQL, and JavaScript
are supported. Patterns can be described within the code or using a
domain-specific language (DSL).

| Windows Build Status | Linux/Mac Build Status |
|---|---|
| [![Windows Build Status](https://ci.appveyor.com/api/projects/status/70hsp9pj8is24s4i?svg=true)](https://ci.appveyor.com/project/KvanTTT/pt-pm-xi6ya) | [![Linux/Mac Build Status](https://api.travis-ci.org/PositiveTechnologies/PT.PM.svg?branch=master)](https://travis-ci.org/PositiveTechnologies/PT.PM) |

## NuGet and Artifacts

The last nightly packages can be found here: [https://ci.appveyor.com/nuget/pt-pm-8eejhkgvrrha](https://ci.appveyor.com/nuget/pt-pm-8eejhkgvrrha). See an article [Using nightly build feed](https://github.com/AvaloniaUI/Avalonia/wiki/Using-nightly-build-feed) for custom package source adding.

Artifacts also available: [https://ci.appveyor.com/project/KvanTTT/pt-pm-xi6ya/build/artifacts](https://ci.appveyor.com/project/KvanTTT/pt-pm-xi6ya/build/artifacts).

## Documentation

* Use Cases and API (TODO)
* Console API (TODO)
* DSL Description and Examples of Patterns (TODO)

## Projects Description

* Creating an AST from the source code and converting this parse tree to UST 
(C\#, Java, PHP, PL/SQL, T-SQL, and JavaScript are implemented).
* PT.PM.CSharpParseTreeUst for C\#- and ASP.NET files (.aspx, .ascx, etc.).
	* PT.PM.JavaParseTreeUst
	* PT.PM.PhpParseTreeUst
	* PT.PM.SqlParseTreeUst (for PL/SQL and T-SQL)
	* PT.PM.JavaScriptParseTreeUst
* DSL processing:
	* PT.PM.Patterns
	* PT.PM.Dsl
* UST preprocessing:
	* PT.PM.UstPreprocessing
* Patterns and UST matching:
	* PT.PM.Patterns
	* PT.PM.Matching

Other projects:

* PT.PM.Common is an helper shared library.
* PT.PM.AntlrUtils are the helper methods for working with ANTLR
    grammars.
* PT.PM is a common workflow (combining work steps, their timing).
* PT.PM.Cli is used for interaction with a console. Also it contains
   an implementation of loggers.
* PT.PM.Prebuild is a utility for creating parsers from ANTLR grammars
    that takes into consideration the changes in the grammars files.
* PT.PM.PatternEditor is an editor for creating and checking the user
    patterns. This GUI can be used to check the correctness of grammars
    and parsing algorithms, convert and match an UST.

Unit tests for projects are located in the projects with similar names
containing the **.Tests** suffix:

* PT.PM.CSharpParseTreeUst.Tests
* PT.PM.CSharpAntlrParseTreeUst.Tests
* PT.PM.JavaParseTreeUst.Tests
* PT.PM.PhpParseTreeUst.Tests
* PT.PM.SqlParseTreeUst.Tests
* PT.PM.JavaScriptParseTreeUst.Tests
* PT.PM.Dsl.Tests
* PT.PM.UstPreprocessing.Tests
* PT.PM.Matching.Tests
* PT.PM.Tests
* PT.PM.Cli.Tests

Parsing and converting test scenarios are written for projects
downloaded from GitHub and cached in a local folder.

## Dependencies

* [FluentCommandLineParser](https://github.com/fclp/fluent-command-line-parser)
    is used for parsing command-line arguments.
* [Microsoft.CodeAnalysis
    (Roslyn)](https://github.com/dotnet/roslyn)is used for analyzing and
    parsing .NET-based languages (C\#).
* [ANTLR](http://www.antlr.org/) is used for parsing formal languages
    (Java, PHP, PL/SQL, T-SQL, JavaScript, and DSL).
* [Newtonsoft.Json](http://www.newtonsoft.com/json) is used for
    serialization/deserialization of JSON.
* [NLog](http://nlog-project.org/) is a logging system.
* [NUnit](http://www.nunit.org/) is a framework for working with unit
    tests.
* [Moq](https://github.com/Moq/moq4) is a framework for mocking
    objects in .NET.
* [Avalonia](https://github.com/AvaloniaUI/Avalonia) is a
    cross-platform .XAML-based NET UI framework.
* [Graphviz](http://www.graphviz.org/) is used for drawing tree
    structures and graphs.

## License

PT.PM is authorized under the [Attribution-NonCommercial 4.0
International](https://creativecommons.org/licenses/by-nc/4.0/legalcode)
(BY-NC) license. The source code and compiled files can be used for
non-commercial projects.
