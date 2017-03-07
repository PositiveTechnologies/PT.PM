# Pattern language for a universal signature-based code analyzer

The process of signature-based code analysis in [PT Application
Inspector](https://www.ptsecurity.com/products/#ai) is divided into the
following stages:

1. Parsing into a language dependent representation (abstract syntax
    tree, AST).
2. Converting an AST to a language (agnostic) unified format.
3. A direct comparison with patterns described in the DSL.

The first two stages were discussed earlier in [Theory and Practice of
Source Code Parsing with ANTLR and Roslyn](http://blog.ptsecurity.com/2016/06/theory-and-practice-of-source-code.html) and [Tree Structures
Processing and Unified AST](http://blog.ptsecurity.com/2016/07/tree-structures-processing-and-unified.html) articles.
The present article focuses on the
third stage, namely: ways of describing patterns, development of a
custom DSL language, which allows to describe patterns, and patterns
written in this language.

Contents

* [Ways of describing patterns](#ways-of-describing-patterns)
    * [Hardcoded patterns](#Hardcoded-patterns)
    * [JSON, XML or some other markup language](#JSON--XML-or-some-other-markup-language)
* [Custom language for pattern description, DSL](#Custom-language-for-pattern-description--DSL)
    * [Practicability](#Practicability)
    * [Syntax](#Syntax)
    * [Examples of patterns](#Examples-of-patterns)
        * [Hardcoded password (all language)](#Hardcoded-password--all-language-)
        * [Weak random number generator (C#, Java)](#Weak-random-number-generator--C---Java-)
        * [Debug information leak (PHP)](#Debug-information-leak--PHP-)
        * [Insecure SSL connection (Java)](#Insecure-SSL-connection--Java-)
        * [Password in comments (all languages)](#Password-in-comments--all-languages-)
        * [SQL Injection (C#, Java, and PHP)](#SQL-Injection--C#--Java--and-PHP-)
        * [Cookie without the secure attribute](#Cookie-without-the-secure-attribute)
        * [Empty try catch block (all languages)](#Empty-try-catch-block--all-languages-)
        * [Insecure cookie (Java)](#Insecure-cookie--Java-)
        * [Dangling Cursor Snarfing (PL/SQL, T-SQL)](#Dangling-Cursor-Snarfing--PL-SQL--T-SQL-)
        * [Excessively granted privileges (PL/SQL, T-SQL)](#Excessively-granted-privileges--PL-SQL--T-SQL-)
* [Summary](#Summary)

## Ways of describing patterns

* Hardcoded patterns
* JSON, XML or some other markup language
* DSL, domain-specific language

### Hardcoded patterns

Patterns can be manually written directly inside the code. There is no
need to develop a parser. This approach is not suitable for
non-developers, though it can be used for writing unit tests. Addition
of new patterns requires recompilation of the whole program.

### JSON, XML or some other markup language

Parts of the compared AST can be stored and retrieved directly from JSON
or other data formats. This approach allows to load patterns from an
external source; however, syntax will be bulky and unsuitable for
editing by the user. Still, this method can be used for serialization of
tree structures. (The next article in the series will present methods
for serialization and bypassing of tree structures in .NET).

## Custom language for pattern description, DSL

The third approach is the development of a special domain-specific
language that will be easily editable, concise, but still having
sufficient expressive power to describe existing and future patterns. A
limitation to this approach is the need to develop the syntax and
parser.

### Practicability

As mentioned in the first article, we can not simply describe all the
patterns using regular expressions. The DSL is a mix of regular
expressions and frequently used structures of programming languages. In
addition, this language is designed for some particular domain knowledge
and it is not expected to be used as some kind of a standard.

### Syntax

The second article in the series discusses the fact that the basic
constructs in imperative programming languages are literals,
expressions, and statements. We used a similar approach for the
development of a DSL language. Examples of expressions:

* `expr(args);` (method call)
* `Id expr = expr;` (variable initialization)
* `expr + expr;` (concatenation)
* `new Id(args);` (object creation)
* `expr[expr];` (accessing an index or key).

Instructions are created by adding a semicolon at the end of the
expression.

Literals are the primitive types, such as:

* Id (an identifier)
* String (a string enclosed in double quotes)
* Int (an integer number)
* Bool (a boolean value)

These literals allow you to describe simple constructs, but you can not
describe a range of numbers or regular expressions using them. The
advanced constructs (PatternStatement, PatternExpression, and
PatternLiteral) were introduced to handle more complex cases. Such
constructs are enclosed in special `<[` and `]>` brackets. The
syntax was borrowed from [Nemerle language](https://en.wikipedia.org/wiki/Nemerle) 
(this language uses these special brackets for quasi-quotation, i.e. transforming the code in these
brackets to an AST tree).

Examples of the supported advanced structures are presented in the list
below. Syntactic sugar, that makes things easier to read or to express,
has been introduced for some structures:

* `<[]>`; an extended expression operator (e.g., `<[md5|sha1]>` or `<[0..2048]>`)
* `#` or `<[expr]>`; any Expression
* `...` or `<[args]>`; an arbitrary number of arguments of any kind
* `(expr.)?expr`; is equivalent to `expr.expr` or `expr`
* `<[~]>expr` — expression negation;
* `expr (<[||]> expr)*` — union of several expressions (logical OR)
* `Comment: "regex"` — search through the comments

### Examples of patterns

#### Hardcoded password (all languages)

`(#.)?<[(?i)password(?-i)]> = <["\w*"]>`

* `(#.)?`; any expression, potentially absent
* `<[(?i)password(?-i)]>`; a regular expression for *Id* types, case insensitive
* `<["\w*"]>`; a regular expression for *String* types

#### Weak random number generator (C#, Java)

`new Random(...)`

The lack is caused by using an insecure algorithm for
generating random numbers. Yet the standard `Random` class constructor is
used to monitor such cases.

#### Debug information leak (PHP)

`Configure.<[(?i)^write$]>("debug", <[1..9]>)`

* `<[(?i)^write$]>`;a regular expression for *Id* types, case insensitive and defines exact occurrences only
* `("debug", <[1..9]>)`; function arguments
* `<[1..9]>`; a range of integers from 1 to 9

#### Insecure SSL connection (Java)

`new AllowAllHostnameVerifier(...) <[||]> SSLSocketFactory.ALLOW_ALL_HOSTNAME_VERIFIER`

Using a "logical OR" with syntax structures. Matching both left
(constructor invocation) and right (using a constant) part of a
structure.

#### Password in comments (all languages)

`Comment: <[ "(?i)password(?-i)\s*\=" ]>`

Search for comments in the source code. Single-line comments begin with
a double slash `//` in C#, Java, and PHP; while a double hyphen `--` is used
in SQL-like languages.

#### SQL Injection (C#, Java, and PHP)

`<["(?i)select\s\w*"]> + <[~"\w*"]>`

A simple SQL injection is the concatenation of any string beginning with
SELECT and containing a non-string expression on the right side.

#### Cookie without the secure attribute

`session_set_cookie_params(#,#,#)`

A cookie has been set without the Secure flag, which is configured in
the fourth argument.

#### An empty try catch block (all languages)

`try {...} catch { }`

An empty exception handling block. If Pattern Matching module analyzes
C# source code, the following code will be matched:

```CSharp
try
{
}
catch
{
}
```

The matching result for T-SQL source code:

```SQL
BEGIN TRY
    SELECT 1/0 AS DivideByZero
END TRY
BEGIN CATCH
END CATCH
```

The matching result for PL/SQL source code:

```SQL
PROCEDURE empty_default_exception_handler IS
BEGIN
    INSERT INTO table1 VALUES(1, 2, 3, 4);
    COMMIT;
  EXCEPTION
    WHEN OTHERS THEN NULL;
END;
```

#### Insecure cookie (Java)

```
Cookie <[@cookie]> = new Cookie(...);
...
<[~]><[@cookie]>.setSecure(true);
...
response.addCookie(<[@cookie]>);
```

Adding a cookie without the Secure flag. Despite the fact that this
pattern is better to implement using a taint analysis, we also managed
to implement it using a more primitive matching algorithm. This
algorithm uses a pinned `@cookie` variable (by analogy with [back
references](http://www.regular-expressions.info/backref.html) in
regex), negation of an expression, and an arbitrary number of
statements.

#### Cursor Snarfing (PL/SQL, T-SQL)

##### PL/SQL

```
<[@cursor]> = DBMS_SQL.OPEN_CURSOR;
...
<[~]>DBMS_SQL.CLOSE_CURSOR(<[@cursor]>);
```

##### T-SQL
```
declare_cursor(<[@cursor]>);
...
<[~]>deallocate(<[@cursor]>);
```

A dangling cursor can be exploited by a less privileged user. Moreover,
most unreleased resource issues result in general software reliability
problems.

If Pattern Matching module analyzes T-SQL source code, the following
code will be matched:

```SQL
DECLARE Employee_Cursor CURSOR FOR
SELECT EmployeeID, Title FROM AdventureWorks2012.HumanResources.Employee;
OPEN Employee_Cursor;
FETCH NEXT FROM Employee_Cursor;
WHILE @@FETCH_STATUS = 0
   BEGIN
      FETCH NEXT FROM Employee_Cursor;
   END;
--DEALLOCATE Employee_Cursor; is missing
GO
```

### Excessively granted privileges (PL/SQL, T-SQL)

`grant_all(...)`

This flaw may result in inappropriate and excessive privileges assigned
to a user. Although the **grant all** phrase actually is an SQL query,
it is converted into a function call, as the pattern matching module
doesn't have a notion of a "query".

The following code will be matched:
`GRANT ALL ON employees TO john_doe;`

## Summary

We’ve prepared a video to demonstrate the functionality of our Pattern
Matching module in PT Application Inspector. This video explains the
process of matching against certain patterns of the source code written
in different programming languages (C#, Java, PHP). We also show you
the proper way to handle syntax errors, which was discussed in the
[first](http://blog.ptsecurity.com/2016/06/theory-and-practice-of-source-code.html) article in this series.

<video>https://youtu.be/06HSszFdAPk</video>

Next time we will tell you about:

* Matching, serialization and tree structures bypassing in .NET
* Building the CFG, DFG and taint analysis
