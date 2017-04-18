# What is DSL?

A pattern matching DSL is a language designed for describing patterns.
Effective code analysis requires more than standard search methods, such
as word search in text or search of text that satisfies a regular
expression. To detect software flaws and vulnerabilities, PM patterns
should be adapted to the application business logic. DSL allows users to
create patterns for detecting code blocks that contain different but
logically similar types, expressions, and other elements.

## Rules for creating patterns

A pattern is a code fragment in which a part of the values can be
replaced by special objects. Supported programming languages: C\#, Java,
PHP, PL/SQL, T-SQL, and JavaScript. Patterns can be created for a single
or multiple languages.

### Special elements

1. `...` or `#*` is an arbitrary number of arguments of any kind
2. `#` is any expression
3. `<[patternValue]>`. This element is used to search for one of
   the six pattern values (patternValue). Combinations of values are
   also allowed. Values can be combined using the operator `||`.

### Types of primitive values

* `ID`: Identifier (should only contain Latin letters,
   numerals, `_` and `$`, must not start with a digit)
* `String`: String (Must be enclosed in double quotation marks (`"`).
   Special characters like single backslash (`\`) or double quote (`"`)
   need to be escaped by a backslash.)
* `Var`: Fixed variable (Should only contain Latin letters,
   numerals, `_` and `$`, must not start with a digit, the `@` character
   must be added before the variable name.)
* `Number`:
  * Integer, octal (starts with `0`) or hexadecimal number (starts with `0X`)
  * Expression: `number (operator number)*`, where operator is `+`, `-`, `*`, `/`
  * Range:
    * `number1..number2` - is consistent with the range in which only
        the left bound is included
    * `..` is an infinite range limited by a set of Int64 values
    * `number1..` - is the range limited by the bottom bound including
        the bound itself
    * `..number2` - is the range limited by the bottom bound excluding
        the bound
* `Bool`:  `bool`, `true` or `false`
* `Null`: `null` value

### Types of primitive structures

* `expression` is any of the structures **1-16** listed below
* `patternValue` is a regular expression, a range of numbers, or any
    primitive value
* `regexString` is a regular expression for strings (separated by
    double quotation marks `"`)
* `regexID` is a regular expression for identifiers

### Valid expressions in the pattern

1. `expression (<[||]> expression)+` is an enumeration (When
    being matched with the code, each expression is matched separately.)
2. `expression(args)` is a function call
3. `expression.Id` or `expression.<[regexID]>` is a reference to a
    member of object (field, method)
4. `(expression.)?Id` or `(expression.)?<[regexID]>` is similar to
    the previous one; the expression with a period may be absent
5. `Id expression = expression` or `<[regexID]> expression = expression` 
is a declaration of a variable
6. `expression = expression` is an assignment
7. `new Id(args)` or `new <[regexID]>(args)` is an object creation
8. value is a primitive value of `ID`, `Sting`, `Number`, `Bool` or `Null`
9. `<[patternValue (|| patternValue)*]>` is an enumeration (When
    being matched with the code, each pattern value is compared
    separately.)
10. `<[(~)? patternValue (|| (~)? patternValue)*]>` is a
    negative enumeration (If the enumeration is specified for the syntax
    structure, then the `<[~]>` syntax is used.)
11. `<[regexVar: (~)? patternValue (|| (~)? patternValue)*]>`
    After the colon, the valid values for the
    fixed variable are listed. For example, password (This means that
    the variable is a password identifier) or `3..10` (This means that
    the variable is a number from 3 to 10.)
12. `expression + expression` is an addition operation (concatenation for
    strings, addition for numbers)
13. `expression - expression` is a subtraction operation
14. `expression \* expression` is a multiplication operation
15. `expression / expression` is a division operation
16. `expression[expression]` is accessing by index or key
17. `Comment: <[regexString]>` is a search in comments
18. `try catch { }` is a try block with an empty exception handler
19. `<{ expression }>` This structure prescribes that `expression` can
    be at any depth of the AST.

### Arguments

`arg (, arg)*` is the list where arg is an
* `expression`;
* `<[args]>` or `...`, or `#*` is an arbitrary number of any
    arguments

## The particular aspects of creating patterns for PHP, PL/SQL, T-SQL languages

1.  The `$` character must be skipped in the names of variables.
2.  Function names in PHP and identifiers in T-SQL and PL/SQL are case
    insensitive; then, the modifiers `(?i)...(?-i)` may be skipped.

## Temporary solution

Valid constructions:

```
statement1
statement2
...
statementN
```

Where `statement` is:
1. `expression;` is an expression with a semicolon
2. `<[~]> expression;` is any structure that does not contain
    this expression
3. `...` is an arbitrary number of any statements (May be skipped.)
4. `if (expression) {statement1 ... statementN}` is a conditional
    operator with a single branch. In order to denote the same variable
    in different statements, use the `<[@var]>` structure
    where `<[@var]>` is a regexVar.

## Examples

1. `(#.)?<[(?i)password(?-i)]> = <["\w*" || null]>`,
  * `#` - any expression that may be absent
  * `<[(?i)password(?-i)]>` - regular expression ID
  * `<["\w*" || null]>` - regular expression String or Null
  * `<[(?i)password(?-i)]>` may be rewritten
        as `<[(?i)password]>`, because, if the modifier is
        effective till the end of a regular expression, then the
        disabling modifier may be skipped.
2. `Configure.<[(?i)^write$]>("debug", <[1..9]>)`
   * `<[(?i)^write$]>` - regular expression ID
   * `("debug", <[1..9]>)` - function arguments
   * `<[1..9]>` - a range of integers from 1 to 9
   * If a pattern is created for PHP only, it can be rewritten as
        follows: `Configure.write("debug", <[1..9]>)`.
3. `new AllowAllHostnameVerifier(...) <[||]> SSLSocketFactory.ALLOW_ALL_HOSTNAME_VERIFIER` search for the `new
    AllowAllHostnameVerifier` with any number of arguments, or
     `SSLSocketFactory.ALLOW_ALL_HOSTNAME_VERIFIER`. A short version of
    the "pattern or" `<[||]>` operator is also
    available - `<|>`.
4. `# = _GET["url"]` - the `$_GET` variable is specified in a
    pattern without the `$` character.
5. `<[@pwd:password]> = #.Text;` where the fixed variable is the
    identifier for password;
    ```
    ~<[@pwd]> = #
    #.Session[<[""]>] = <[@pwd]>;
    ```
6. `<{ document.<[^(URL|referrer|cookie)$]> }>`. An
    example of the code with which the following pattern will be
    matched: `document.getElementById('test').onclick=function(){ document.URL + "a" }`

Patterns with a temporary solution:

### Insecure Cookie (C\#)

```CSharp
HttpCookie <[@cookie]> = new HttpCookie(...);
... // may be skipped
<[~]><[@cookie]>.Secure = True;
... // may be skipped
Response.Cookies.Add(<[@cookie]>);
```

Response.Cookies.Add(&lt;\[@cookie\]&gt;);

### Insecure Cookie (Java)

```Java
Cookie <[@cookie]> = new Cookie(...);
...
<[~]><[@cookie]>.setSecure(true);
...
#.addCookie(<[@cookie]>);
```