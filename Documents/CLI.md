# PT.PM CLI

**PT.PM** can be used in CLI (Command Line Interface) mode. In this
case, the following parameters can be specified:

## -f, -files

The path to the source code file or directory. PT.PM determines whether
the path is a file or directory. One of the parameters, **-f** or
**--p**, must be specified.

## -l, --languages

Languages to be processed. **CSharp**, **Java**, **Php**, **PlSql**,
**TSql**, **Aspx**, and **JavaScript** are supported (case-insensitive).
Whitespace used as a separating character. If the parameter is not
set, the language is determined automatically depending on the
extension. If the extension is not set, or there is an ambiguity (the
.sql extension can be both defined as T-SQL and PL/SQL), the algorithm
try to determine language by file content (a file is parsed and the
language with less parsing errors is chosen). If the language could not
be determined for some reason after parsing a file, this file is
ignored. If JavaScript is specified, JavaScript insertions will be
processed inside PHP code.

## -p, --patterns

If the parameter value ends with **.json**, patterns are loaded from the
passed file name. Otherwise, patterns are loaded from the string. The file
and string patterns have the same format except that a line is achieved by
compression and conversion.

Each pattern is represented as follows:
* **Key** — pattern ID (`string` or `long`)
* ***Name*** — \[optionally\] pattern name
* ***Languages*** — \[optionally\] target pattern languages (**CSharp**,
**Java**, **PHP**, **PLSQL**, **TSQL**, **JavaScript** a combination
thereof, for example, **CSharp, Java**). All languages are supported
by default.
* ***DataFormat*** — \[optionally\] format for describing a 
pattern (**JSON**, **DSL**). Default format: **DSL**.
DSL description is located in document: [DSL.md](DSL.md).
* **Value**—pattern value in DataFormat.
* ***CweId*** — \[optionally\] Common Weakness Enumeration Id.
* ***Description*** — \[optionally\] pattern description.

An example of **Hardcoded Password Pattern**:

    {
      "Key": "96",
      "Languages": "CSharp, Java, PHP, PLSQL, TSQL",
      "DataFormat": "Dsl",
      "Value": "<[(?i)password]> = <[ \"\\w*\" || null ]>" 
    }

JSON backslash (**\\**) and double-quote mark (**"**) characters should be
escaped with a backslash **\\**. Actually, Value looks more simple:
`<[(?i)password(?-i)]> = <[ "\w*" || null ]>`.

Compression is performed using the standard `GZipStream.NET` class. When
converting to base64, the source string is converted into an array of
bytes using `Encoding.UTF8`; then, the resulting array is compressed.
Before writing an array to the first four bytes of the resulting array,
the length of this array is written (this length is used when
unpacking).

If patterns are not set, the default built-in patterns are used for
matching.

If **--p** parameter is set and **-f** parameter is not set,
pattern validation mode will be activated.

## -t, --threads

The maximum number of threads in use. **0** — at the discretion of the OS.
Default value: **1**.

## -s, --stage

Phase at which the parsing is stopped Default value: **match**.

* **read** — file reading
* **parse** — parsing
* **convert** — converting to UST
* **preprocess** — tree preprocessing (calculating arithmetic
    expressions, joining lines, simplifying patterns)
* **match** — matching with patterns
* **patterns** — mode for checking the patterns

## -m, --memory

Approximate memory consumption (MB) of ANTLR parsers. Default value:
**500 MB**. ANTLR сaches information on parsing; thus, memory
consumption may increase significantly up to several gigabytes.
Therefore, it's necessary to clear the cache from time to time.

## --max-stack-size

The maximum stack size for the thread (in bytes). Used if the StackOverflow
exception occurred during ANTLR parsing. You can first start parsing of
a file without this parameter; then, if an exception occurred and the
process is abruptly interrupted, you can use this parameter (For
example, specify the size of `int.MaxValue / 8`).

## --logs-dir

The path to the directory to which the info, errors, and
matching\_result logs will be written while the engine is running. By
default, the parameter is set to `%LOCALAPPDATA%\PT.PM\Logs` directory.

## --log-errors

Is error logging required? Default value: **false**.

## -v, --version

Is it necessary to output the engine's version? Default value: **true**.
