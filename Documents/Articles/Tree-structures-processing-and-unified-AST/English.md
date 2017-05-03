# Tree structures processing and unified AST

[The previous article in this series](http://blog.ptsecurity.com/2016/06/theory-and-practice-of-source-code.html) discussed the theory of source code
parsing in ANTLR and Roslyn. The article pointed out that a
signature-based code analysis in [PT Application Inspector](https://www.ptsecurity.com/ww-en/products/ai/) is divided into the following stages:

1. Parsing into a language dependent representation (abstract syntax
tree, AST).
2. Converting AST to a language independent unified format (unified AST,
UAST).
3. A direct comparison with patterns described in the DSL.

The current article focuses on the second stage that includes AST
processing using Visitor and Listener strategies, converting AST to a
unified format, simplifying an AST, and the algorithm for matching tree
structures.

<img alt="Visitor & Listener" align="right" src="https://habrastorage.org/getpro/habr/post_images/031/d2d/4ca/031d2d4cab81a98a67c1962ee1d75f41.png"/>

## Contents

* [AST Traversing](#ast-traversing)
    * [Visitor and Listener](#visitor-and-listener)
    * [Differences between Visitor in ANTLR and Roslyn](#differences-between-visitor-in-antlr-and-roslyn)
    * [Grammar and Visitor in ANTLR](#grammar-and-visitor-in-antlr)
    * [Alternative and Element Labels in ANTLR](#alternative-and-element-labels-in-antlr)
* [Types of nodes in a unified AST](#types-of-nodes-in-a-unified-ast)
* [Testing of converters](#testing-of-converters)
* [Simplifying an UAST](#simplifying-an-uast)
* [Algorithm for matching tree structures](#algorithm-for-matching-ast-and-patterns)
* [Conclusion](#conclusion)

## AST Traversing

As is known, a parser converts the source code into an AST, a parse tree
with redundant tokens removed. There are several different ways of
processing such a tree. Probably the easiest one implies a recursive
in-depth traversal of descendants. However, this approach can only work
for rather simple cases, when the number of node types is small and the
processing logic is plain to understand. In other cases, we need to
split the processing logic into separate methods. To achieve this goal,
we use two standard mechanisms (design patterns): Visitor and Listener.

### Visitor and Listener

In the Visitor processing of the node descendants requires invoking
their traversal methods manually. If the parent has three child nodes
and we call methods only for two of them, a part of the subtree will not
be processed. In the Listener (Walker) interface processing methods for
node descendants are called automatically. The Listener interface
contains `enterNode()` and `exitNode()` methods, which are invoked when
entering and exiting a given node. Using an event mechanism allows
implementation of these methods. Unlike methods in the Listener
interface, Visitor methods may return objects and may even be typed. For
instance, when we declare `CSharpSyntaxVisitor`, each Visit method returns
an `AstNode` object, a common ancestor of all other nodes in a unified
AST.

Thus, using Visitor’s design pattern to convert the tree provides you
with dynamic and concise code because there is no need to
store information about the visited nodes. The figure below shows that
unnecessary HTML and CSS nodes are truncated while converting the PHP
language. The order of traversal is indicated by numbers. Listener is
usually used to aggregate data (e.g. from CSV files) and to convert one
code to another (JSON -&gt; XML). For more information, refer to [The
Definitive ANTLR 4
Reference](http://www.amazon.com/Definitive-ANTLR-4-Reference/dp/1934356999).

<img alt="Visitor & Listener" align="center" src="https://habrastorage.org/files/bd1/69c/535/bd169c535e854f9681520f520d0db9c3.png"/>

### Differences between Visitor in ANTLR and Roslyn

Visitor and Listener implementations may differ in libraries. The table
below provides details about Visitor/Listener classes and methods in
Roslyn and ANTLR.

|               | ANTLR                              | Roslyn                        |
| ------------- | ---------------------------------- |-------------------------------|
| Visitor       | AbstractParseTreeVisitor< Result > | CSharpSyntaxVisitor< Result > |
| Listener      | IParseTreeListener                 | CSharpSyntaxWalker            |
| Default       | DefaultResult                      | DefaultVisit(SyntaxNode node) |
| Visit         | Visit(IParseTree tree)             | Visit(SyntaxNode node)        |

Both Roslyn and ANTLR have the methods returning the default result (if
the Visitor method is not overridden for some syntactic structure), and
the Visit method that determines which special Visitor method should be
called.

ANTLR generates a Visitor for every syntactic grammar rule. There are
also special types of methods listed below:

* `VisitChild(IRuleNode node)`; is used to implement the default
    node traversal.
* `VisitTerminal(IRuleNode node)`; is used when traversing the
    terminal nodes, i.e. tokens.
* `VisitErrorNode(IErrorNode node)`; is used when traversing the
    tokens obtained from parsing the code with lexical or syntax errors.
    For example, if a statement is missing a semicolon at the end, the
    parser will insert such a token and report it as an error. For more
    information about parsing errors, see the [previous article](http://blog.ptsecurity.com/2016/06/theory-and-practice-of-source-code.html#antlr-errors).
* `AggregateResult(AstNode aggregate, AstNode nextResult)`; a rarely
    used method intended for aggregating results derived from the
    traversal of descendants.
* `ShouldVisitNextChild(IRuleNode node, AstNode currentResult)`; a
    rarely used method intended for determining whether it is necessary
    to process the next `node` descendant depending on the result of the
    `currentNode` traversal.

Visitor pattern for Roslyn has specific methods for each syntactic
structure and a generalized Visit method that will work for all nodes.
Unlike ANTLR, it is missing methods to perform traversal of
“intermediate” structures. For example, Roslyn does not imply any method
for `VisitStatement` statements, there are only some specific methods
like `VisitDoStatement`, `VisitExpressionStatement`,
`VisitForStatement`, etc. The generalized Visit method can be used as
a `VisitStatement` method. Another difference is that the traversal
methods for SyntaxTrivia nodes (i.e. nodes, which can be easily removed
without losing code information, like a space, or a comment) are called
along with the traversal methods for the main nodes and tokens.

The drawback of using ANTLR visitors is that the names of the generated
Visitor methods are directly dependent on the style of grammar rules, so
they may fail to fit in with the overall code style. For example, SQL
grammars use the so-called *Snake case*, in which the words are
separated with underscore characters. Roslyn methods are written in the
style of C\# code. Despite the differences, processing techniques for
tree structures in Roslyn and ANTLR become more and more unified with
each new version (ANTLR version 3 and earlier had no support for Visitor
and Listener mechanisms).

### Grammar and Visitor in ANTLR

In ANTLR, the

```ANTLR
ifStatement
	: If parenthesis statement elseIfStatement* elseStatement?
	| If parenthesis ':' innerStatementList elseIfColonStatement* elseColonStatement? EndIf ';'
	; rule
```

will generate a `VisitIfStatement(PHPParser.IfStatementContext context)`
method, wherein the context will have the following fields:

* `parenthesis` – a single node.
* `elseIfStatement*` – a node array. If the syntax is missing, then
    the array length is null.
* `elseStatement?` – an optional node. If the syntax is missing, then
    the optional node is null.
* `If`, `EndIf` – terminal nodes, start with a capital letter.
* `':'`, `';'` – unnamed terminal nodes, are not contained in the
    context (available only through GetChild ()).

It is worth noting that the fewer rules involved in the grammar, the
easier and faster the Visitor can be written. However, the repeating
syntax also needs to be brought out to separate rules.

### Alternative and element labels in ANTLR

Quite often we have a situation when a rule has other alternatives, and
it would be logical to handle these alternatives in the individual
methods. Luckily, ANTLR 4 has the *alternative* labels that begin with
an `#` character and are added after each rule alternative. When
generating a parser code, a separate Visitor method is generated for
each label, which allows to avoid having a huge amount of code in case
if the rule has lots of alternatives. All the alternatives should be
marked or none of them. We can use the rule element labels to name the
terminal denoting a set of values:

```ANTLR
expression
    : op=('+'|'-'|'++'|'--') expression                   #UnaryOperatorExpression
    | expression op=('*'|'/'|'%') expression              #MultiplyExpression
    | expression op=('+'|'-') expression                  #AdditionExpression
    | expression op='&&' expression                       #LogicalAndExpression
    | expression op='?' expression op2=':' expression     #TernaryOperatorExpression
    ;
```

ANTLR generates `VisitExpression`, `VisitUnaryOperatorExpression`,
`VisitMultiplyExpression`, and some other visitors for this rule. Each
Visitor will contain an expression array consisting of 1 or 2 elements
and an op literal. Labels will keep the code clear and concise:

```CSharp
public override AstNode VisitUnaryOperatorExpression(TestParser.UnaryOperatorExpressionContext context)
{
    var op = new MyUnaryOperator(context.op().GetText());
    var expr = (Expression)VisitExpression(context.expression(0));
    return new MyUnaryExpression(op, expr);
}
public override AstNode VisitMultiplyExpression(TestParser.MultiplyExpressionContext context)
{
    var left = (Expression)VisitExpression(context.expression(0));
    var op = new MyBinaryOpeartor(context.op().GetText());
    var right = (Expression)VisitExpression(context.expression(1));
    return new MyBinaryExpression(left, op, right);
}
public override AstNode VisitTernaryOperatorExpression(TestParser.TernaryOperatorExpressionContextcontext)
{
    var first = (Expression)VisitExpression(context.expression(0));
    var second = (Expression)VisitExpression(context.expression(1));
    var third = (Expression)VisitExpression(context.expression(2));
    return new MyTernaryExpression(first, second, third);
}
...
```

Without using alternative labels, the processing of Expression is in the
same method and the code is as follows:

```CSharp
public override AstNode VisitExpression(TestParser.ExpressionContext context)
{
    Expression expr, expr2, expr3;
    if (context.ChildCount == 2) // Unary
    {
        var op = new MyUnaryOperator(context.GetChild<ITerminalNode>(0).GetText());
        expr = (Expression)VisitExpression(context.expression(0));
        return new MyUnaryExpression(op, expr);
    }
    else if (context.ChildCount == 3) // Binary
    {
        expr = (Expression)VisitExpression(context.expression(0));
        var binaryOp = new MyBinaryOpeartor(context.GetChild<ITerminalNode>(0).GetText());
        expr2 = (Expression)VisitExpression(context.expression(1));
        return new MyBinaryExpression(expr, binaryOp, expr2);
        ...
    }
    else // Ternary
    {
        var first = (Expression)VisitExpression(context.expression(0));
        var second = (Expression)VisitExpression(context.expression(1));
        var third = (Expression)VisitExpression(context.expression(2));
        return new MyTernaryExpression(first, second, third);
    }
}
```

Alternative labels exist not only in ANTLR, but also in other tools for
describing grammars. For example, unlike with ANTLR, an assignment
operator label in [Nitra](https://github.com/JetBrains/Nitra) is located
to the left of the alternative:

```
syntax Expression
    {
      | IntegerLiteral
      | BooleanLiteral
      | NullLiteral            = "null";
      | Parenthesized          = "(" Expression ")";
      | Cast1                  = "(" !Expression AnyType ")" Expression;
      | ThisAccess             = "this";
      | BaseAccessMember       = "base" "." QualifiedName;
      | RegularStringLiteral;
```

### Types of nodes in a unified AST

The development of the structure for a unified AST was guided by the
structure of the [NRefactory](https://github.com/icsharpcode/NRefactory)
AST. We find this structure quite simple, at the same time, fidelity (a
tree converted to code character by character) is not required. Every
node is inherited from the AstNode and has its own type (NodeType),
which is used at the stage of matching with patterns and deserialization
from JSON. The structure of nodes looked like this:

<img alt="UAST Types" align="center" src="https://habrastorage.org/files/13d/b83/ad5/13db83ad52fa491fbc89b8e0655912c3.png"/>

In addition to the type, each node has a property that stores the
location in the code (TextSpan), which is used to display it in the
source code when comparing with the pattern. A nonterminal node keeps a
list of child nodes, and terminal - a numeric, string or other primitive
value.

In order to compare AST nodes of different languages we created a table,
where each line represents the syntax of certain nodes and each column
is their implementation in C\#, Java, and PHP languages. The table
looked as follows:

| Node  | Args | C# | Java | PHP | MCA | MDA   |
|-------|------|----|------|-----|-----|-------|
| While | <font color="green">cond:Expression</font>; <font color="blue">stmt:Statement</font> | while (<font color="green">cond</font>) <font color="blue">stmt</font> | while (<font color="green">cond</font>) <font color="blue">stmt</font> | while (<font color="green">cond</font>) <font color="blue">stmt</font> | While(<font color="green">cond</font>, <font color="blue">stmt</font>) | While(<font color="green">cond</font>, <font color="blue">stmt</font>) |
| BinaryOp | <font color="green">l,r:Expression</font>; <font color="red">op:Literal</font> | <font color="green">l</font> <font color="red">op</font> <font color="green">r</font> | <font color="green">l</font> <font color="red">op</font> <font color="green">r</font> | <font color="green">l</font> <font color="red">op</font> <font color="green">r</font> | BinaryOp(<font color="green">l</font>, <font color="red">op</font>, <font color="green">r</font>) | BinaryOp(<font color="green">l</font>, <font color="red">op</font>, <font color="green">r</font>) |
| ... | ... | ... | ... | ... | ... | ... |
| Checked | <font color="green">checked:Expression</font> | checked(<font color="green">checked</font>) | - | - | <font color="green">checked</font> | Checked(<font color="green">checked</font>) |
| NullConditional | <font color="green">a:Expression</font>,<font color="red">b:Literal</font> | <font color="green">a</font>?.<font color="red">b</font> | - | - | <font color="green">a</font> != null ? <font color="green">a</font>.<font color="red">b</font> : null | <font color="green">a</font>?.<font color="red">b</font> |

The explanation to the terms given in this table:

* <font color="green"><strong>Expression</strong></font>; the expression has a return value.
* <font color="blue"><strong>Statement</strong></font>; the statement (instruction), has no return value.
* <font color="red"><strong>Literal</strong></font>; a terminal node.
* **Most Common Ast** (MCA) This node is built if all three languages
    contain a node of this or similar type (e.g.
    IfStatement, AssignmentExpression).
* **Most Detail Ast** (MDA) This node is built if at least one
    language contains a node of this type (e.g. FixedStatenemt
    `fixed (a) { }` for C\#). These nodes are more relevant to SQL like
    languages due to the fact they are classified as declarative, and
    the difference between T-SQL and C\# is much more significant than
    between PHP and C\#.

In addition to the nodes seen in the figure (and pattern nodes described
in the next section) there are also artificial nodes required to build
the Most Common Ast node with as little loss in syntax as possible. The
examples of such nodes are:

* `MultichildExpression` is inherited from the Expression, but it
    contains a collection of other Expression type nodes;
* `WrapperExpression` is inherited from the Expression, but it contains
    an arbitrary type node;
* `WrapperStatement` is inherited from the Statement, but contains an
    arbitrary type node.

**Expression** and **Statement** are basic constructs in imperative
programming languages. First ones have a return value, while the second
are used to execute operations. Therefore in this module we have focused
mostly on them. These constructs are the basic building blocks of both
the [CFG](https://en.wikipedia.org/wiki/Control_flow_graph)
implementation and other source code representations required for taint
analysis. The detection of vulnerabilities in the source code requires
no knowledge about the syntactic sugar, generics or other things
specific to a particular language. Thus we can rewrite the syntactic
sugar to basic constructs and remove some specific details.

Artificial nodes representing user templates are called pattern nodes.
For example, a range of numbers and regular expressions are used as
literal patterns.

### Testing of converters

Testing of the entire code (instead of its parts) is a task of high
priority for the code analyzer. To accomplish this, we decided to
override Visitor methods for all the node types. Thus if vizitor is not
used, it generates an exception `new ShouldNotBeVisitedException(context)`.
This approach simplifies the
development, because the [IntelliSense](https://en.wikipedia.org/wiki/Intelligent_code_completion)
knows which methods were overridden, and which were not. Therefore, it helps to identify which
Visitor methods have already been implemented.

We also have some suggestions on improving the code coverage analysis.
Each node of the unified AST keeps the location of the corresponding
source code. At the same time all the terminals are associated with
lexemes, i.e. certain sequences of characters. Since all the lexemes
should be processed, the coverage ratio can be expressed in the
following form where `uterms` — terminals of the unified AST, and
`terms` — terminals of the typical AST in Roslyn or ANTLR:

![cover factor](http://mathurl.com/zf79e77.png)<br>

This metric represents code coverage using a single coefficient that
should tend to unity. The evaluation through this coefficient is
approximate, however it could be used to refactor and improve the
Visitor's code. We can use graphical representation of the covered
terminals to obtain a more reliable analysis.

### Simplifying an UAST

After converting an AST to UAST the latter should be simplified. The
simplest and most effective optimization method is a [constant
folding](https://en.wikipedia.org/wiki/Constant_folding). For example,
there are some code vulnerabilities related to setting the excessively
long lifetime of the cookie: `cookie.setMaxAge(2147483647);` the
argument in brackets can be both written as a single number, e.g.
`86400`, and some arithmetic expression, `60 * 60 * 24`. Another example
is related to string concatenation when searching SQL-injection and
other vulnerabilities.

To achieve this goal Visitor for the UAST was implemented. Since
simplification of an AST reduces the number of nodes in the tree, the
Visitor is typed: it accepts and returns the same type. The reflection
feature in .NET allows the implementation of such a Visitor with small
code size. Since each node contains other nodes or terminal primitive
values, then using reflection enables to extract all possible members of
the particular node and to process them, calling other visitors in a
recursive scenario.

### Algorithm for matching AST and patterns

The algorithm is trying to match the pattern introduced as a tree
structure with a tree fragment rooted at the current node. First, the
node type is compared and then the following operations are performed
depending on its type:

* Recursive comparison of descendants.
* Comparison of simple literal types (identifier, strings,
    and numbers).
* Comparison of extended literal types (regular expressions, ranges).
    Comments are included in this type.
* Comparison of complex extended types (expressions, the
    Statement sequence).

This approach is based on simple principles to achieve high performance
with a relatively small amount of code for their implementation. The
latter is achieved due to the fact that the CompareTo method to compare
nodes is implemented for the base class, terminals, and a small number
of other nodes. It is not yet required to implement more sophisticated
finite-state machine algorithms improving the performance. However, it
is difficult (or even impossible) to use this algorithm for more
advanced analysis, e.g., the one sensitive to semantics and covering
links between different AST nodes.

## Conclusion

In this article we went over Visitor and Listener patterns used to
process trees. We also talked about the structure for a unified AST.
Next time we will tell you about:

* Methods for storing code patterns (Hardcoded, Json, DSL).
* Developing and using the DSL to describe patterns.
* Examples of some actual patterns and principles of searching them in
    open source projects.
* Building the CFG, DFG and taint analysis.