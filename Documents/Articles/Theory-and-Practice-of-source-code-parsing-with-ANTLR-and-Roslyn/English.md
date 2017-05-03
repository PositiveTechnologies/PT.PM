# Theory and Practice of Source Code Parsing with ANTLR and Roslyn

[PT Application Inspector](https://www.ptsecurity.com/ww-en/products/ai/) provides several approaches 
to analysis of the source code written in different programming languages:

* Search by signatures.
* Exploring the properties of mathematical models derived from the static abstract interpretation of code.
* Dynamic analysis of the deployed application and verification of the static analysis results.

This series of articles focuses on the structure and operation
principles of the signature analysis module (PM, pattern matching). The
key benefits of such an analyzer include high performance, simplicity of
pattern description, and scalability across various languages. The
disadvantage of this approach is that the module is not able to analyze
complex vulnerabilities, which require developing high-level models of
code execution.

<img align="right" src="https://habrastorage.org/files/879/56c/123/87956c123b5248f58393586831153cd4.png" alt="Unified Pattern Matching Workflow"/>

The following requirements have been defined for the module under
development:

* Capability of working with multiple programming languages and the option to add new ones easily.
* Functionality that allows analysis of the code containing syntactic and semantic errors.
* Capability of describing patterns using a common programming language (DSL, domain specific language).

In this case, all the patterns describe flaws or vulnerabilities in the
source code.

The process of code analysis could be divided into the following stages:

1. Parsing into a language dependent representation (abstract syntax tree, AST).
2. Converting AST to a language independent unified format (UAST).
3. A direct comparison with the patterns described in the DSL.

This article focuses on the first stage that includes parsing, comparing
functionalities and features of various parsers, as well as applying
theoretical principles to practice using Java, PHP, PLSQL, TSQL and even
C\# grammars. Other stages will be discussed in future publications.

## Contents

* [Theory of parsing](#theory-of-parsing)
    * [Terminology](#terminology)
    * [Types of formal languages](#types-of-formal-languages)
    * [ANTLR](#antlr)
    * [Roslyn](#roslyn)
* [Parsing problems](#parsing-problems)
    * [Using keywords as identifiers](#using-keywords-as-identifiers)
    * [Ambiguity](#ambiguity)
    * [Handling whitespaces, comments](#handling-whitespaces-comments)
    * [Handling parse errors](#handling-parse-errors)
        * [ANTLR errors](#antlr-errors)
        * [Roslyn errors](#roslyn-errors)
* [From theory to practice](#from-theory-to-practice)
    * [Java- and Java8 grammars](#java--and-java8-grammars)
    * [PHP grammar](#php-grammar)
        * [Case insensitive keywords](#case-insensitive-keywords)
        * [Lexical modes for PHP, HTML, CSS, and JavaScript](#lexical-modes-for-php-html-css-and-javascript)
        * [Complex context sensitive statements](#complex-context-sensitive-statements)
    * [T-SQL grammar](#t-sql-grammar)
    * [PL/SQL grammar](#plsql-grammar)
    * [C# grammar](#c-grammar)
        * [Preprocessor directives](#preprocessor-directives)
        * [String interpolation](#string-interpolation)
    * [Testing](#testing)
        * [Correctness of ANTLR parsers](#correctness-of-antlr-parsers)
        * [Performance of ANTLR and Roslyn parsers](#performance-of-antlr-and-roslyn-parsers)
        * [Error handling in ANTLR and Roslyn parsers](#error-handling-in-antlr-and-roslyn-parsers)
        * [Memory consumption of ANTLR runtime](#memory-consumption-of-antlr-runtime)
* [Conclusion](#conclusion)
* [References](#references)

## Theory of parsing

At the outset, the following question may arise: why do we need to build
a unified AST or develop algorithms for graph comparison instead of
using regular expressions? The point here is that not all patterns can
be simply described using regular expressions. It should be noted that 
regular expressions in C# are as concise as context-free grammars due to named
groups and reverse links. There is also an
[article](http://www.viva64.com/en/b/0087/) from the
PVS-Studio developers covering this subject. Moreover, the coherence in
a unified AST allows using it to build the more complex representations
of code execution, such as [code property
graph](https://user.informatik.uni-goettingen.de/~krieck/docs/2014-ieeesp.pdf).

### Terminology

Those already familiar with the theory of parsing may skip this section.

Parsing is a process of creating a structured representation of the
source code. Typically, parsing is broken into two parts, lexing and
parsing. The lexer groups the source code characters into notional
sequences called lexemes. It will then identify the type of the lexeme (an
identifier, a number, a string, etc.). The set of values of the lexeme and its type 
is called a token. If you have a look at the figure below,
*sp*, *=*, *100* is the token. The parser converts a stream of
tokens into the tree structure, which is called a parse tree. In this
case, *assign* is one of the nodes of the tree. The abstract syntax tree
(AST) is a high-level parse tree with "unimportant" tokens such as
brackets or commas removed. However, some parsers combine parsing and
lexing.

![Lexer & Parser](https://habrastorage.org/files/6c4/385/fbe/6c4385fbe3d8471982c9b2a030106d38.png)

There are some rules, which define AST nodes. The rules together are
called the grammar of the language. Tools that generate code for a
particular platform (runtime) to perform lexical analysis of
grammar-based languages are called parser generators. Such as *ANTLR*,
*Bison*, *Coco/R*. However, for some reason, the parser is often written
manually. *Roslyn* can be an example of such tool. The advantages of
a manual approach are that parsers tend to be more efficient and
readable.

We have decided to develop this project using .NET technologies, then we
have chosen Roslyn to perform the analysis of the C\# code and ANTLR for
other languages as it supports the .NET runtime and has more features compared to 
other alternatives.

### Types of formal languages

There are 4 types of formal languages according to [the Chomsky
hierarchy](https://en.wikipedia.org/wiki/Chomsky_hierarchy):

* Regular grammars: **a<sup>n</sup>**
* Context-free grammars: **a<sup>n</sup>b<sup>n</sup>**
* Context-sensitive grammars: **a<sup>n</sup>b<sup>n</sup>c<sup>n</sup>**
* Turing complete.

Regular expressions describe only elementary statements for matching,
which, however, cover the majority of tasks in everyday practice.
Another advantage of regular expressions is that most programming
languages support them. Complexity in both writing and parsing makes
Turing complete languages unsuitable for computation in practice (for
example, an esoteric programming language called
[Thue](https://en.wikipedia.org/wiki/Thue_(programming_language)) comes
to mind).

Thus far, in fact all the syntax of modern programming languages can
be defined by context-free grammars. If we compare context-free grammars
and regular expressions in lay terms, the latter do not have a memory
(are not able to perform calculations). And if we compare
context-sensitive and context-free grammars, the latter do not remember
the already applicable rules (are able to calculate only two things).

Moreover, the language may be context-free in one case and
context-sensitive in the other. Given the semantics (i.e., consistency
with the definitions used in the language and consistency of types in
particular) the language may be considered as context-sensitive. For
example, **<font color="red">T</font>** a = new **<font color="red">T</font>**().
The type in the constructor on the right must be the same as the one on the left.
It is usually advisable to check the
semantics after parsing. Still there are such syntactic constructions
that cannot be parsed using context-free grammars, for example, **Heredoc**
in PHP: $x = <<<**<font color="red">EOL</font>** Hello world **<font color="red">EOL</font>**; the
**EOL** token (or line
break) is a special character signifying the end of a line of text and
the start of a new line, therefore it requires memorizing the value of
the token being visite. This article focuses on the analysis of such
context-free and context-sensitive languages.

### ANTLR

<img align="left" src="https://habrastorage.org/files/3ce/bab/ae6/3cebabae6be0455587bc3a379dc7a4f9.png" alt="ANTLR" />

This parser generator is an
[LL(\*)](https://en.wikipedia.org/wiki/LL_parser), it has existed for
over 20 years and the 4th version was released in 2013. Now it’s under
development on GitHub. This module allows users to create parsers in
Java, C\#, Python2, Python3, and JavaScript. C++, Swift, and Go are coming
soon (hopefully). Well, I need to say, that it is simple enough to develop and debug
grammars using this tool. Despite the fact that LL grammars do not allow
for left-recursive grammar rules, ANTLR since version 4 will let you
write the above mentioned rules (except for the rules with hidden or
indirect left recursion). These rules are translated into ordinary rules
during the parser generation. This reduces the time when writing
expressions, for example, arithmetic ones:

```ANTLR
expr
    : expr '*' expr
    | expr '+' expr
    | <assoc=right> expr '^' expr
    | id
    ;
```

In addition, parsing performance is significantly improved by using the
Adaptive LL algorithm (\*). This algorithm combines the advantages of
relatively slow GLL and [GLR](https://en.wikipedia.org/wiki/GLR_parser)
algorithms, which, however, are able to resolve cases of ambiguity (used
in the analysis of natural languages) compared to fast LL recursive
descent algorithms, which in turn are not able to resolve all problems
with ambiguity. The idea of the used algorithm is based on
pseudo-parallel running LL parsers on each rule, caching and choosing
the right prediction (as opposed to the GLR where a number of
alternatives is allowed). Thus, the algorithm is dynamic. Although the
theoretical worst-case behavior of this algorithm parsing is O(n<sup>4</sup>),
the parsing rate for existing programming languages appears to be linear
in practice. The 4th version also has an improved error recovery algorithm.
Read more about the ANTLR 4 algorithms and differences with the other parsing 
algorithms in the following article: [Adaptive LL(\*) Parsing: The Power of Dynamic Analysis](http://www.antlr.org/papers/allstar-techreport.pdf).

### Roslyn

<img align="left" src="https://habrastorage.org/files/f73/8f6/1df/f738f61dfbc34181bb430ac48c0965f3.png" alt="Roslyn" />

Roslyn is not just a parser; it is a fully-featured tool for parsing,
analyzing and compiling C\# code. It is also developed on GitHub, but it
is more advanced than ANTLR. This article deals only with its parsing
features, regardless of the semantics. Roslyn parses the code to
fidelity, immutable, and thread safe tree. Fidelity is that such tree
can be converted back into the same character-for-character code,
including spaces, comments and preprocessor directives, even if there
are some syntax errors. Immutability makes it easy to handle
multiple-tree processing, as a "smart" copy of a tree (which is used only
for storing the changes) is created in each separate stream. The above
mentioned tree may consist of:

* **Syntax Node** — a non-terminal node of the tree containing a few 
other nodes and displaying a certain structure. It may also contain 
an optional node (e.g., ElseClauseSyntax for if).

* **Syntax Token** — a terminal node that represents a keyword, an
identifier, a literal, or a punctuation mark.

* **Syntax Trivia** — a terminal node that represents a space, a comment or a
preprocessor directive (it can be easily removed without losing
code information). Trivia does not have a parent. These nodes are
critical when converting a tree back to code (e.g.,
during refactoring).

## Parsing problems

The development of grammars and parsers introduces some challenges that
should be considered.

### Using keywords as identifiers

It often happens that some keywords may appear as identifiers during the
parsing. For example, in the C \# the `async` keyword placed before the 
method signature indicates that this method is asynchronous. But if this
word will be used as the identifier of the `var async = 42;` variable, the
code will be also valid. In ANTLR, this problem can be solved in two
ways:

1. Using a semantic predicate for the syntactic rule:
`async: {_input.LT(1).GetText() == "async"}? ID ;` while the async
token itself will not exist. This approach is bad because the
grammar becomes dependent on runtime and looks ugly.

2. Inserting the token into the id rule:
```ANTLR
ASYNC: 'async';
...
id
    : ID
    ...
    | ASYNC;
```

### Ambiguity

Natural language contains ambiguous expressions (like, "Flying planes
can be dangerous"). Such constructions may also occur in a formal
language. For example:

```ANTLR
stat: expr ';' // expression statement
    | ID '(' ')' ';' // function call statement;
    ;
expr: ID '(' ')'
    | INT
    ;
```

However, contrary to natural languages, they are likely to be the result
of improper grammars. ANTLR is not able to detect such ambiguity in the
process of generating a parser, but if we set the `LL_EXACT_AMBIG_DETECTION` 
mode (as ALL is a dynamic algorithm),
ambiguity can be defined during the process of parsing. Ambiguity may
arise in both lexer and parser. The lexer generates a token for each of
two identical tokens (see the example with identifiers). Yet, in
languages where the ambiguity is acceptable (for example, C ++), you can
use semantic predicates (code insertions) to resolve it, for example:

```ANTLR
expr: { isfunc(ID) }? ID '(' expr ')' // func call with 1 arg
    | { istype(ID) }? ID '(' expr ')' // ctor-style type cast of expr
    | INT
    | void
    ;
```

Sometimes the ambiguity can be fixed after a little reinvention of
grammar. For example, there is a right shift bit operator `RIGHT_SHIFT: '>>'` 
in C\#: two angle brackets can also be used to
describe a generics class: `List<List<int>>`. If we define the
`>>` as a token, the construction of two lists would never be parsed
because the parser will assume that there is a `>>` operator instead
of two closing brackets. To resolve this you only need to put the
`RIGHT_SHIFT` token aside. At the same time, we can leave the
`LEFT_SHIFT: '<<'` token as-is, because such a sequence of
characters would not take place during the parsing of a valid code.

Yet, we have not performed a detailed analysis of whether there is any
ambiguity in grammars developed using our module.

### **Handling whitespaces, comments**

Another parsing problem is handling comments. The disadvantage here is
that the comments being included into the grammar make it
overcomplicated; in fact, each node will contain comments. However,
we cannot simply eliminate the comments, because they may contain some
relevant information. ANTLR uses the so-called channels to handle the
comments, these channels isolate a lot of comments from other
tokens: `Comment: ~[\r\n?]+ -> channel(PhpComments);`

In Roslyn the comments are included into the tree nodes, but they belong
to a special type called Syntax Trivia. You can get a list of trivial
tokens associated with a certain ordinary token in both ANTLR and
Roslyn. ANTLR has a method 
`getHiddenTokensToRight(int tokenIndex, int channel)` which collect 
all tokens on specified channel to the right of the current token 
up until we see a token on `DEFAULT_TOKEN_CHANNEL` or `EOF`. If `channel`
 is -1, it finds any non default channel token. Roslyn adds such
tokens to the terminal Syntax Token.

In order to retrieve all the comments in ANTLR you can get all tokens on
a specified channel: `lexer.GetAllTokens().Where(t => t.Channel == PhpComments)`, 
in Roslyn you can get all DescendantTrivia for the root
node with the following SyntaxKind: *SingleLineCommentTrivia*,
*MultiLineCommentTrivia*,
*SingleLineDocumentationCommentTrivia*,*MultiLineDocumentationCommentTrivia*,
*DocumentationCommentExteriorTrivia*, *XmlComment*.

Handling white spaces and comments is one of the reasons for which the
code, for example, LLVM, cannot be used for the analysis: they will be
just omitted. Apart from handling comments, even handling of whitespace
is a very important part. For example, detecting errors in a single *if
statement* (this example was taken from an article entitled [PVS-Studio
delved into the FreeBSD
kernel](http://www.viva64.com/en/b/0377/#ID0EHEAE)):

```C
case MOD_UNLOAD:
    if (via_feature_rng & VIA_HAS_RNG)
        random_nehemiah_deinit();
        random_source_deregister(&random_nehemiah);
```

### Handling parse errors

<img align="right" src="https://habrastorage.org/files/7eb/ac2/95e/7ebac295e2544306ad18a43b363c41fa.png" alt="Parsing Error" />

An important capability of each parser is error handling. The reasons
are as follows:

* The parsing process should not be interrupted only because of a 
single mistake, it must recover properly and continue to parse the
code (for instance, after missing a semicolon).
* Search for relevant error and its location, instead of searching
multiple irrelevant errors.

#### ANTLR errors

The following parsing errors are present in ANTLR:

* **Token recognition error** (Lexer no viable alt). Is the only
lexical error, indicating the absence of the rule used to create
the token from an existing lexeme: <br /><br />
class **<font color="red">#</font>** { int i; } — **<font color="red">#</font>** is the above mentioned lexeme.

* **Missing token.** In this case, ANTLR inserts the missing
token to a stream of tokens, marks it as missing, and continues
parsing as if this token exists. <br /><br />
class T { int f(x) { a = 3 4 5; } **<font color="green">}</font>** — 
**<font color="green">}</font>** is the above mentioned token.

* **Extraneous token.** ANTLR marks a token as incorrect and
continues parsing as if this token doesn’t exist: The example of
such a token will be the first **<font color="red">;</font>** <br /><br />
class T **<font color="red">;</font>** { int i; }

* **Mismatched input.** In this case "panic mode" will be
initiated, a set of input tokens will be ignored, and the parser
will wait for a token from the synchronizing set. The 4th and 5th
tokens of the following example are ignored and **;** is the
synchronizing token<br /><br />
class T { int f(x) { a = 3 **<font color="red">4 5</font>**; } }

* **No viable alternative input.** This error describes all other
possible parsing errors.<br /><br />
class T { **<font color="red">int ;</font>** }

Furthermore, errors can be handled manually by adding an error alternative to
the rule:

```ANTLR
function_call
    : ID '(' expr ')'
    | ID '(' expr ')' ')' {notifyErrorListeners("Too many parentheses");}
    | ID '(' expr {notifyErrorListeners("Missing closing ')'");}
    ;
```

Moreover, ANTLR 4 allows you to use your own error handling mechanism.
This option may be used to increase the performance of the parser:
first, code is parsed using a fast SLL algorithm, which, however, may
parse the ambiguous code in an improper way. If this algorithm reveals
at least a single error (this may be an error in the code or ambiguity),
the code is parsed using the complete, but less rapid ALL-algorithm. Of
course, an actual error (e.g., the missed semicolon) will always be parsed
using LL, but the number of such files is less compared to ones without
any errors.

Maximizing performance when parsing in ANTLR:
```Java
// try with simpler/faster SLL(*)
parser.getInterpreter().setPredictionMode(PredictionMode.SLL);
// we don't want error messages or recovery during first try
parser.removeErrorListeners();
parser.setErrorHandler(new BailErrorStrategy());
try {
    parser.startRule();
    // if we get here, there was no syntax error and SLL(*) was enough;
    // there is no need to try full LL(*)
}
catch (ParseCancellationException ex) { // thrown by BailErrorStrategy
    tokens.reset(); // rewind input stream
    parser.reset();
    // back to standard listeners/handlers
    parser.addErrorListener(ConsoleErrorListener.INSTANCE);
    parser.setErrorHandler(new DefaultErrorStrategy());
    // full now with full LL(*)
    parser.getInterpreter().setPredictionMode(PredictionMode.LL);
    parser.startRule();
}
```

#### Roslyn errors

The following parsing errors are present in Roslyn:

* **Missing syntax;** Roslyn completes the corresponding node with the
`IsMissing = true` property value (a common example — Statement
without a semicolon).
* **Incomplete member;** a separate node `IncompleteMember` is created.
* **Incorrect value** of the numeric, string, or character literal
(e.g., a too long value, an empty char): A separate node with Kind
equal to `NumericLiteralToken`, `StringLiteralToken`, or
`CharacterLiteralToken` is created.
* **Excessive syntax** (e.g., an accidentally typed character): A separate
node with `Kind = SkippedTokensTrivia` is created.

The following code fragment demonstrates these errors (You can explore the features of Roslyn using the Visual Studio plugin
[Syntax
Visualizer](https://visualstudiogallery.msdn.microsoft.com/2ddb7240-5249-4c8c-969e-5d05823bcb89)):
```CSharp
namespace App
{
    class Program
    {
        ;                    // Skipped Trivia
        static void Main(string[] args)
        {
            a                // Missing ';'
            ulong ul = 1lu;  // Incorrect Numeric
            string s = """;  // Incorrect String
            char c = '';     // Incorrect Char
        }
    }

    class bControl flow
    {
        c                    // Incomplete Member
    }
}
```

These carefully selected types of syntax errors in Roslyn allows
converting the tree with any number of errors character by character to
code.

## From theory to practice

<img align="left" src="https://habrastorage.org/files/e54/08c/d6d/e5408cd6dc2f422a90287482ec9df95b.png" alt="Open Source" />

[Php](https://github.com/antlr/grammars-v4/tree/master/php),
[tsql](https://github.com/antlr/grammars-v4/blob/master/tsql/tsql.g4),
[plsql](https://github.com/antlr/grammars-v4/tree/master/plsql) grammars
illustrating the above theory were developed and maked open-source.
PHP and SQL parsers use these grammars. In order to parse Java code we used
the already existing
[java](https://github.com/antlr/grammars-v4/blob/master/java/Java.g4)
and
[java8](https://github.com/antlr/grammars-v4/blob/master/java8/Java8.g4)
grammars. We have also refined the
[C\#](https://github.com/antlr/grammars-v4/tree/master/csharp) grammar
(for versions 5 and 6) used to compare parsers based on Roslyn and
ANTLR. Below you will find the most interesting aspects of developing
and using these grammars. Although SQL-based languages are regarded as
declarative rather than imperative, T-SQL and PL/SQL extensions provide
support for imperative programming ([Control
flow](https://msdn.microsoft.com/en-us/library/ms174290.aspx)). Our
source code analyzer is mainly being developed for these aspects.

### **Java- and Java8 grammars**

In most cases, Java 7-based parser is faster than Java 8, unless there
is deep recursion, e.g., parsing of
[ManyStringConcatenation.java](https://gist.github.com/KvanTTT/bf20f6a4aac708b49df2)
file takes far longer and requires more memory resources. I would like
to note that this is not an artificial example; we really came across
such "spaghetti code". As it turned out, the problem is caused by
left-recursive grammar rules in expression. Java 8 grammar contains only
primitive recursive rules. Primitive recursive rules differ from
ordinary recursive rules in the way they refer to themselves in the left
or right side of the alternative, but not in both simultaneously. An
example of the ordinary recursive expression:

```ANTLR
expression
    : expression ('*'|'/') expression
    | expression ('+'|'-') expression
    | ID
    ;
```

The following rules are obtained after converting the rules above to
primitive left-recursive:

```ANTLR
expression
    : multExpression
    | expression ('+'|'-') multExpression
    ;

multExpression
    : ID
    | multExpression ('*'|'/') ID
    ;
```

Or even to non-recursive ones (however, it is not so easy to handle
expressions after parsing, because they are no longer binary):

```ANTLR
expression
    : multExpression (('+'|'-') multExpression)*
    ;
```

If the operation has right associativity (e.g., exponentiation),
primitive right-recursive are used:

```ANTLR
expression
    : <assoc=right> expression '^' expression
    | ID
    ;
```

```ANTLR
powExpression
    : ID '^' powExpression
    | ID
    ;
```

On the one hand, conversion of left-recursive rules allows us to address
the problem of excessive memory consumption and poor performance for
rare files with a large number of similar expressions, on the other - brings
performance issues when processing other files. It is therefore
advisable to use primitive recursion for expressions, which may be deep
(e.g., the concatenation of strings), and ordinary recursion for all
other cases (e.g., the comparison of numbers).

### PHP grammar

[Phalanger](http://www.php-compiler.net/) allows parsing PHP code on
.NET plarform. However, we are not satisfied with the fact that this project is
actually not developed and provides no Visitor interface for traversing
the AST nodes (the only interface presented is the Walker). It was
therefore agreed to develop PHP grammar for ANTLR using our own
resources.

#### Case insensitive keywords

As far as is known, all tokens in PHP, except for the names of variables
(which begin with '$'), are case-insensitive. In ANTLR case
insensitivity can be implemented in two ways:

1. Declaring fragment lexical rules to define all the Latin characters
and using them as follows:
```ANTLR
Abstract:           A B S T R A C T;
Array:              A R R A Y;
As:                 A S;
BinaryCast:         B I N A R Y;
BoolType:           B O O L E A N | B O O L;
BooleanConstant:    T R U E | F A L S E;
...
fragment A: [aA];
fragment B: [bB];
...
fragment Z: [zZ];
```
An ANTLR fragment is a part of the token, which can be used in other
token, but it is not a token itself. It is a "syntactic sugar" for
describing tokens. Without the use of fragments, the first token can
be written as `Abstract: [Aa] [Bb] [Ss] [Tt] [Rr] [Aa] [Cc] [Tt]`. 
The advantage of this approach is that the generated
lexer is independent of the runtime since the characters in upper and
 lower cases are declared directly in the grammar. The downside is that
the lexer performance achieved in this approach is lower than in the
second approach.

2. Converting the entire input stream of characters to the lower
(or upper) case and starting the lexer, in which all the tokens
described using this case. However, you need to perform this
conversion for each particular runtime (Java, C\#, JavaScript,
Python), as described in [Implement a custom File or String Stream
and Override
LA](https://theantlrguy.atlassian.net/wiki/pages/viewpage.action?pageId=2687342).
Under this approach, it is difficult to make some tokens
case-sensitive and other case-insensitive.

The first approach is used in the developed PHP grammar since lexical
analysis usually takes less time than syntactical. Despite the fact that
the grammar is still dependent from the runtime, this approach makes
grammar porting to other runtimes easier. Furthermore, we created the
Pull Request [RFC Case Insensitivity Proof of
Concept](https://github.com/antlr/antlr4/pull/1092) to facilitate the
description of case-insensitive tokens.

#### Lexical modes for PHP, HTML, CSS, and JavaScript

It is commonly known that PHP code inclusions may be placed anywhere in
the HTML code. The same HTML code may include CSS and JavaScript code
(these blocks of embedded code are known as "islands"). For example, the
following code (using [Alternative
Syntax](http://php.net/manual/en/control-structures.alternative-syntax.php))
is valid:
```PHP
<?php switch($a): case 1: // without semicolon?>
        <br>
    <?php break ?>
    <?php case 2: ?>
        <br>
    <?php break;?>
    <?php case 3: ?>
        <br>
    <?php break;?>
<?php endswitch; ?>
```

or
```PHP
<script type="text/javascript">
document.addEvent('domready', function() {
	var timers = { timer: <?=$timer?> };
	var timer = TimeTic.periodical(1000, timers);
    functionOne(<?php echo implode(', ', $arrayWithVars); ?>);
});
</script>
```

Fortunately, ANTLR provides us with a mechanism called "modes", which
allow to switch between different sets of tokens under certain
conditions. For example, the *SCRIPT* and *STYLE* modes were designed to
generate a flow of tokens for JavaScript and CSS (in fact, they are
simply ignored in this grammar). HTML tokens are generated in the
*DEFAULT_MODE*. It is worth noting that it is possible to implement the
support for Alternative Syntax in ANTLR without adding the target code
to the lexer. i.e.: *nonEmptyStatement* includes the *inlineHtml* rule,
which, in turn, includes the tokens received in the DEFAULT_MODE:

```ANTLR
nonEmptyStatement
    : identifier ':'
    | blockStatement
    | ifStatement
    | ...
    | inlineHtml
    ;
...

inlineHtml
    : HtmlComment* ((HtmlDtd | htmlElement) HtmlComment*)+
    ;
```

#### Complex context sensitive statements

We should mention that although ANTLR supports only context-free
grammars, there are also the so-called "actions" containing the
arbitrary code, which extend the number of languages to at
least context-dependent ones. Such code inclusions allow implementing
parsing of
[Heredoc](http://php.net/manual/en/language.types.string.php#language.types.string.syntax.heredoc)
and some other structures:

```PHP
<?php
    foo(<<< HEREDOC
        Heredoc line 1.
        Heredoc line 2.
        HEREDOC
)   ;
?>
```

### T-SQL grammar

Despite the common root «SQL», T-SQL (MSSQL) and PL/SQL grammars differ
greatly from each other.

It would be nice to stay off the development of our own parser for this
complex language. Nevertheless, the existing parsers did not meet the
criteria of total coverage and relevance (e.g.,
[grammar](https://github.com/karlatgit/tsql-grammar) for the deprecated
GOLD parser) and have closed source code ([General SQL
Parser](http://www.sqlparser.com/)). Finally, it was decided to recover
TSQL grammar from the MSDN documentation. The result was worth it: the
grammar covers many common syntactic constructions, looks neat, stays
independent of the runtime, and it has been tested on SQL examples from
MSDN. The complexity of the development was that some
tokens in the grammar are optional. For example, a semicolon. In this
case, error recovery during parsing is not so smooth.

### PL/SQL grammar

Refinement of PL/SQL grammar took even less time, because the very
grammar had already [existed under
ANTLR3](https://github.com/porcelli/plsql-parser). The main difficulty
was the fact that it had been developed using the java-runtime. Most
Java code insertions had been removed, since you can build AST without
using them (as mentioned earlier, the semantics can be checked at
another stage). The following insertions

```ANTLR
decimal_key
    : {input.LT(1).getText().equalsIgnoreCase("decimal")}? REGULAR_ID
```

were replaced by the fragment tokens:

`decimal_key: D E C I M A L`, as described above.

### C\# grammar

Strange as it seems, but the refinement of the grammar supporting 5 and
6 language versions, was quite a difficult task. The major concerns were
the string interpolation and proper processing of preprocessor
directives. Because these things are context-dependent, the lexer and
the parser for processing directives turned out to be dependent of the
runtime.

#### Preprocessor directives

C\# allows you to compile the following code properly (code after the
first directive cannot be compiled, still it is not included into the
compilation since false is never satisfied).

```
#if DEBUG && false
Sample not compilied wrong code
var 42 = a + ;
#else
// Compilied code
var x = a + b;
#endif
```

In order to be processed correctly, the code is split to an array of
tokens located in the default *COMMENTS\_CHANNEL* and *DIRECTIVE*
channels. The `codeTokens` list is also created; it contains the proper
tokens for parsing. Then, the preprocessor parser calculates the value
for the directive of preprocessor tokens. Special attention shall be
given to the fact that ANTLR also allows you to write the code to
calculate the value of complex logical expressions directly in the
grammar. For more details on the implementation, check the following
link [CSharpPreprocessorParser.g4](https://github.com/antlr/grammars-v4/blob/master/csharp/CSharpPreprocessorParser.g4).
A value of `true` or `false` is calculated only for `#if`, `#elif`, and `#else`,
directives, all of the remaining directives always return `true`, because
they do not affect whether or not the following code is to be compiled.
This parser also allows you to find the default Conditional Symbols
(defined as `DEBUG` by default).

After the directive value was calculated and it gets a true value, the
subsequent tokens are added to the `codeTokens` list, otherwise they are
skipped. Such an approach allows to ignore the wrong tokens (like var
`42 = a + ;` in this example) at the stage of parsing. The parsing process
is described as follows:
[CSharpAntlrParser.cs](https://gist.github.com/KvanTTT/d95579de257531a3cc15).

#### String interpolation

This feature was very challenging to develop since the closing curly
bracket may mean a part of an interpolation expression or exit of the
expression mode. A colon can also be part of the expression, and could
mean the end of the expression and description of the output format (for
example, \#0.\#\#). Additionally, such strings may be regular,
verbatim or nested. For more details about syntax [see the MSDN
page](https://msdn.microsoft.com/en-us/library/dn961160.aspx).

The above-described items are shown in the following code, which is
valid syntactically:

```CSharp
s = $"{p.Name} is \"{p.Age} year{(p.Age == 1 ? "" : "s")} old";
s = $"{(p.Age == 2 ? $"{new Person { } }" : "")}";
s = $@"\{p.Name}
                       ""\";
s = $"Color [ R={func(b: 3):#0.##}, G={G:#0.##}, B={B:#0.##}, A={A:#0.##} ]";
```

The interpolation of strings has been implemented using the stack for
calculating the current level of the interpolation string and brackets.
All of this is implemented in
[CSharpLexer.g4](https://github.com/antlr/grammars-v4/blob/master/csharp/CSharpLexer.g4).

## Testing

### Correctness of ANTLR parsers

Obviously, there is no need to test the parsing correctness of the
Roslyn parser. On the other hand, we paid a lot of attention to testing
of the ANTLR parser.

* Testing of files containing a wide diversity of
syntactic constructions. For example, test files for TSQL are
located in the official [antlr 
grammars-v4](https://github.com/antlr/grammars-v4/tree/master/tsql/examples) repository.
The
[AllInOne.cs](https://github.com/antlr/grammars-v4/blob/master/csharp/examples/AllInOne.cs)
file borrowed from Roslyn was used and refined for the C\# parser.

* Testing of specially prepared files both containing syntax errors
and free of them.

* Testing of actual projects (latest version). PHP has been tested
using [WebGoat](https://github.com/shivamdixit/WebGoatPHP),
[phpbb](https://github.com/phpbb/), and [Zend
Framework](https://github.com/zendframework/zf2). C\# parser has
been tested using
[Roslyn-1.1.1](https://github.com/dotnet/roslyn),
[Corefx-1.0.0-rc2](https://github.com/dotnet/corefx),
[ImageProcessor-2.3.0](https://github.com/JimBobSquarePants/ImageProcessor),
[Newtonsoft.Json-8.0.2](https://github.com/JamesNK/Newtonsoft.Json),
and others.

### Performance of ANTLR and Roslyn parsers

Testing was conducted in a single-threaded mode, in release
configuration without the debugger attached. **ANTLR 4 4.5.0-alpha003** and
**Roslyn (Microsoft.CodeAnalysis) 1.1.1** were tested.

#### WebGoat.PHP

The number of processed files — 885. The total number of strings — 137
248, characters — 4 461 768.

Approximate time - 00:00:31 sec (55% by lexer, 45% by parser).

#### PL/SQL Samples

The number of processed files — 175. The total number of strings — 1
909, characters — 55 741.

Approximate time &lt; 1 sec. (5% by lexer, 95% by parser).

#### CoreFX-1.0.0-rc2

The number of processed files — 7329. The total number of strings — 2
286 274, characters — 91 132 116.

Approximate time:
* Roslyn: 00:00:04 sec
* ANTLR: 00:00:24 sec. (12% by lexer, 88% by parser)

#### Roslyn-1.1.1

The number of processed files — 6527. The total number of strings — 1
967 672, characters — 74 319 082.

Approximate time:
* Roslyn: 00:00:03 sec
* ANTLR: 00:00:16 sec. (12% by lexer, 88% by parser)

According to the testing results achieved with *CoreFX* and *Roslyn*, we
may conclude that the developed C\# parser on ANTLR is less five times
slower than the Roslyn parser, which suggests the a great quality of the
last-named. It’s understood that the parser created in a week as a
kitchen-table effort will hardly ever be able to compete with such
giants of the market like Roslyn, but it can be used to parse C\# code
on Java, Python, or JavaScript (and other future languages), because the
parsing speed is still fast.

Based on the remaining tests it can be concluded that lexing is a
substantially faster stage than parsing. The exception is the PHP
lexer that spent more time on lexing compared to parsing. This appears
to be due to complex logic of the lexer and complex rules, but it is not
influenced by case insensitive keywords, since T-SQL and PL/SQL lexers
(which also contain case insensitive keywords) are much faster than
parsers (up to 20 times). For example, if you use the 
`SHARP: NEW_LINE Whitespace* '#';` instead of `SHARP: '#';`,
the lexer will be 10 times slower instead of being 7 times faster! This
is explained by the fact that any file has a lot of whitespaces, and the
lexer will try to find the `#` symbol on each string, so its performance
will be significantly slower (we were faced with such a problem, thus
checking for a directive in the new string should be carried out at the
stage of semantic analysis).

### Error handling in ANTLR and Roslyn parsers

We wrote a simple C\# file containing all parsing errors in ANTLR:

```CSharp
namespace App
{
    ©
    class Program
    {
        static void Main(string[] args)
        {
            a = 3 4 5;
        }
    }

    class B
    {
        c
    }
```

ANTLR errors

* token recognition error at: '©' at 3:5
* mismatched input '4' expecting {'as', 'is', '\[', '(', '.', ';',
'+', '-', '\*', '/', '%', '&', '|', '\^', '&lt;', '&gt;', '?',
'??', '++', '--', '&&', '||', '-&gt;', '==', '!=', '&lt;=',
'&gt;=', '&lt;&lt;'} at 8:19
* extraneous input '5' expecting {'as', 'is', '\[', '(', '.', ';',
'+', '-', '\*', '/', '%', '&', '|', '\^', '&lt;', '&gt;', '?',
'??', '++', '--', '&&', '||', '-&gt;', '==', '!=', '&lt;=',
'&gt;=', '&lt;&lt;'} at 8:21
* no viable alternative at input 'c}' at 15:5
* missing '}' at 'EOF' at 15:6

As a next step, we have tested the above-mentioned file using Roslyn compiler 
and discovered the following errors:

* test(3,5): error CS1056: Unexpected character '©'
* test(8,19): error CS1002:; expected
* test(8,21): error CS1002:; expected
* test(15,5): error CS1519: Invalid token '}' in class, struct, or
interface member declaration
* test(15,6): error CS1513: } expected

The number of errors detected using Roslyn was similar to that detected
via ANTLR. The first and the last errors differ only in the name. The
parsers have also been tested on files that are more complex. It is
clear that Roslyn detects fewer errors and these errors are more
relevant. However, in simple cases such as missing or extra tokens
(semicolon, brackets), ANTLR detects the relevant position and the
description of an error. ANTLR gives consistently worse results with
errors when the part of a lexer code is written manually (compilation
directives, interpolation strings). For example, if we write an `#if`
directive without any condition, the rest part of the code may not be
parsed correctly. However, in these cases, the code for recovering the
process of parsing should be written manually as well (as this is a
context sensitive structure).

### Memory consumption of ANTLR runtime

As mentioned above, ANTLR 4 uses the internal cache obtained in the
process of parsing in order to increase the performance of parsing the
follow-up files. If you process too many files (we performed a test on
about 70000 PHP files) or re-parse the files in the same process, memory
consumption may increase significantly up to several gigabytes. You can
clear the cashe by using the `lexer.Interpreter.ClearDFA()` interpreter
method for the lexer and `parser.Interpreter.ClearDFA()` - for the parser
after processing a certain number of files or after memory consumption
has exceeded a certain threshold value.

After solving the problem of clearing cache, we have discovered an issue
with multi-threaded parsers. By practical experience, we have found that
the use of `GetAllTokens()` and `ClearDFA()` methods from different threads
in the lexer (similar for the parser) in rare cases may lead to the
"Object reference not set to an instance of an object" exception.
Despite the fact that this behavior is due to an error in the ANTLR C\#
runtime, it can be fixed by locking with several readers (code parsers)
and one writer (a cache cleaner). In the C# runtime a `ReadWriterLockSlim` 
synchronization primitive can be used to achieve such a goal.

For obvious reasons, Roslyn parser does not consume gigabytes of memory.
The peak memory consumption did not exceed 200 MB, when parsing five
large C\# projects, *aspnet-mvc-6.0.0-rc1*, *roslyn-1.1.1*, *corefx*,
*Newtonsoft.Json-8.0.2*, and *ImageProcessor-2.3.0*.

## Conclusion

This article has covered source code parsing with ANTLR and Roslyn.
Future articles will address the following:

* Conversion of the parse trees to a unified AST using Visitor or 
Walker (Listener).
* A guide to writing an easy-to-read, efficient, and user-friendly
grammar in ANTLR 4.
* Serialization and tree structures traversing in .NET;
* Pattern matching in a unified AST.
* Development and use of DSL for describing patterns.

## References

* F. Yamaguchi. *Modeling and Discovering Vulnerabilities with Code Property Graphs*.
Proceedings of the 2014 IEEE Symposium on Security and Privacy, SP, 2014.
* Alfred V. Aho, Monica S. Lam, Ravi Sethi, and Jeffrey D.
*Compilers: Principles, Techniques, and Tools*. Pearson Education Inc, Sep. 2006.
* Terence Parr. *The Definitive ANTLR Reference*. Pragmatic Bookshelf, 2013.
* Terence Parr, Sam Harwell, Kathleen Fisher. *Adaptive LL(\*) Parsing: The Power of Dynamic Analysis*.
ACM New York, 2014.
* Roslyn. <https://github.com/dotnet/roslyn>
* ANTLR grammars. <https://github.com/antlr/grammars-v4>
* ANTLR. <https://github.com/antlr/antlr4>
