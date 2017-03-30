# PT.PM API

В данном документ описываются основные классы движка **PT Pattern Matching Engine** и их взаимодействие. Помимо описания прилагаются примеры с различными сценариями использования.

## Содержание

* [Workflow](#workflow)
    * [Входные параметры](#Входные-параметры)
    * [Результат](#Результат)
* [Репозитории исходного кода](#Репозитории-исходного-кода)
* [Репозитории шаблонов](#Репозитории-шаблонов)
* [Парсеры и конвертеры](#Парсеры-и-конвертеры)
* [Основные классы и интерфейсы](#Основные-классы-и-интерфейсы)
* [Логгирование](#Логгирование)
* [Узлы UST](#Узлы-UST)
    * [Основные типы узлов](#Основные-типы-узлов)
    * [Типы узлов-паттернов](#Типы-узлов-паттернов)
    * [Обход UST](#Обход-UST)
* [Ошибки и исключения](#Ошибки-и-исключения)
* [Вспомогательные классы ANTLR](#Вспомогательные-классы-ANTLR)
* [Другие вспомогательные классы](#Другие-вспомогательные-классы)

## Workflow

`Workflow` - общий класс, объединяющий этапы чтения и парсинг файлов, преобразования деревьев и сопоставления UST с шаблонами. Также этот класс несет ответственность за тайминг этих этапов. Также поддерживается распараллеливание.

### Входные параметры

* `ISourceCodeRepository SourceCodeRepository` - репозиторий исходного кода.
* `LanguageFlags languages` - список языков для обработки. На основе этих языков `Workflow` определяет, какие парсеры `ILanguageParser` и конвертеры `IParseTreeToUstConverter` необходимо создать.
* `IPatternsRepository PatternsRepository` - репозиторий шаблонов.
* `Stage` - конечный этап работы:
  * `Read` - Чтение файла
  * `Parse` - Парсинг
  * `Convert` - Конвертация в универсальное AST.
  * `Preprocess` - Препроцессинг дерева (расчет арифметических выражений, соединение строк, упрощение шаблонов).
  * `Match` - Сопоставление с шаблонами.
  * `Patterns` - Режим проверки парсинга шаблонов.

  Если указан режим `Match`, то предшествующие этапы также будут выполняться. Это не касается режима `Patterns`, который отвечает только за проверку шаблонов.
* `ILogger Logger` - логгер, который внедряется в другие внутренние объекты, реализующие `ILoggable`.

### Результат

Результат обработки получается следующим образом: `WorkflowResult result = workflow.Process()`.
При этом результирующий класс имеет следующие свойства:

* Список прочитанных исходников `IReadOnlyList<SourceCodeFile> SourceCodeFiles`
* Список деревьев разбора `IReadOnlyList<ParseTree> ParseTrees`
* Список унифицированных синтаксических деревьев: `IReadOnlyList<Ust> Usts`
* Список найденных сопоставлений ` IReadOnlyList<MatchingResult> MatchingResults`
* Время работы для всех этапов:
    * `TotalReadTimeSpan`
    * `TotalParseTimeSpan`
    * `TotalConvertTimeSpan`
    * `TotalPreprocessTimeSpan`
    * `TotalMatchTimeSpan`
    * `TotalPatternsTimeSpan`
    * `TotalLexerTicks` - общее время лексера (только для ANTLR парсеров)
    * `TotalParserTicks` - общее время парсера (только для ANTLR парсеров)
* `TotalProcessedFileCount` - количество обработанных файлов
* `TotalProcessedCharsCount` - количество обработанных символов
* `TotalProcessedLinesCount` - количество обработанных строк
* `ErrorCount` - общее количество всех ошибок
* Также `Workflow` генерирует сообщения логгера в реальном времени.

Схема работы изображена на рисунке:

![Workflow](https://habrastorage.org/files/995/1c3/952/9951c3952620446abca59beb4297e02a.png)

## Репозитории исходного кода

Все репозитории реализуют интерфейс `ISourceCodeRepository`, находится в проекте **PT.PM.Common**.

* `FileCodeRepository` - загружает исходный код из единичного файла `fileName`.
* `FilesAggregatorCodeRepository` - загружает все файлы из заданной папки `filePath`.
* `MemoryCodeRepository` - загружает исходный код из строки `fileName`.
* `ZipAtUrlCachedCodeRepository` - скачивает GitHub репозиторий и распаковывает его. Используется для тестирования парсеров и конвертеров.

## Репозитории шаблонов

Все репозитории реализуют интерфейс `IPatternsRepository`, находится в проекте **PT.PM.Patterns**.

* `FilePatternsRepository` - загружает объекты `PatternDto` из единичного файла `filePath`.
* `MemoryPatternsRepository` - позволяет добавлять объекты `PatternDto` из памяти.
* `DslPatternRepository` - позволяет добавлять паттерн из строки `patternData` для языков `languageFlags` в формате DSL.
* `DefaultPatternRepository` - в этом репозитории находятся шаблоны по-умолчанию, заданные непосредственно в коде.

## Парсеры и конвертеры

* `ILanguageParser` - описывает парсер исходного кода в дерево разбора `ParseTree`. Реализации:
    * `CSharpRoslynParser` - парсер C\# кода на основе Roslyn.
    * `JavaAntlrParser` - парсер Java кода на основе ANTLR.
    * `PhpAntlrParser` - парсер Php кода на основе ANTLR.
    * `TSqlAntlrParser` - парсер T-SQL кода на основе ANTLR.
    * `PlSqlAntlrParser` - парсер PL/SQL кода на основе ANTLR.
    * `JavaScriptAntlrParser` - парсер JavaScript кода на основе ANTLR.
* `IParseTreeToUstConverter` - описывает конвертер `ParseTree` в `Ust`. Реализации:
    * `CSharpRoslynParseTreeConverter` - конвертер C\# кода.
    * `JavaAntlrParseTreeConverter` - конвертер Java кода.
    * `PhpAntlrParseTreeConverter` - конвертер Php кода.
    * `TSqlAntlrConverter` - конвертер T-SQL кода.
    * `PlSqlAntlrConverter` - конвертер PL/SQL кода.
    * `JavaAntlrParseTreeConverter` - конвертер JavaScript кода.
* `IAstPatternMatcher<TPatternsDataStructure>` - описывает матчер `Ust` и шаблонов `Pattern`. Возвращает коллекцию из `MatchingResult`. Пока что имеет одну реализацию: `BruteForcePatternMatcher`, которая находится в **PT.PM.Matching**.

## Основные классы и интерфейсы

* `SourceCodeFile` - объект исходного кода, содержащий:
    * `Name` - имя.
    * `RelativePath` - относительный путь к файлу из корня репозитория.
    * `Code` - непосредственно исходный код.
* `ParseTree` - дерево разбора, получаемое в результате парсинга объекта исходного кода `SourceCodeFile`.
* `Ust` (Universal Syntax Tree) - универсальное абстрактное синтаксическое дерево, получаемое в результате работы конвертеров, реализующих `IParseTreeToUstConverter`.
* `Pattern` - объект, хранящий в себе шаблон в структурированной форме, готовой к использованию в движке сопоставления.
    * `Key` - уникальный идентификатор шаблона типа `string`.
    * `LanguageFlags` - список языков, для которых применим данный универсальный шаблон. Например, использование экземпляра класса `Random` подходит для C# и Java.
    * `PatternNode Data` - непосредственно фрагмент AST для шаблона, который накладывается на `Ust`.
* `PatternDto` - сериализованный `Pattern`, предназначенный для его хранения и передачи. Имеет такие же 
    * `Name` - имя (опциональное).
    * `Key` - уникальный идентификатор шаблона типа `string`.
    * `LanguageFlags` - список языков, для которых применим данный универсальный шаблон. Например, использование экземпляра класса `Random` подходит для C# и Java.
    * `DataFormat` - формат шаблона. Доступны `Json` и `Dsl`.
    * `Value` - текст шаблона в формате `Json` или `Dsl` (зависит от `DataFormat`).
    * `CweId`
    * `Description` - описание шаблона.
    * `DebugInfo` - информация, помогающая во время отладки.

    Пример сериализованного в Json шаблона:
    ```JSON
    {
      "Key": "96",
      "Name": "Hardcoded Password",
      "Languages": "CSharp, Java, PHP, PLSQL, TSQL",
      "DataFormat": "Dsl",
      "Value": "<[(?i)password]> = <[ \"\\w*\" || null ]>" 
    }   
    ```
* `MatchingResult` - результат сопоставление паттерна `Pattern` с `Ust`. Содержит следующие свойства:
    * `Pattern` - ссылка на шаблон.
    * `List<UstNode> Nodes` - список сопоставленных узлов, т.к. их может быть несколько для многострочных шаблонов.
    * `FileNode` - ссылка на файл с исходником.
* `MatchingResultDto` - результат сопоставление `MatchingResult`, пригодный для сериализации.
    * `MatchedCode` - фрагмент сопоставленного кода.
    * `BeginLine`, `BeginColumn`, `EndLine`, `EndColumn` - координаты в формате линия-колонка.
    * `PatternKey` - идентификатор шаблона.
    * `SourceFile` - файл исходного кода.

    Пример сериализованного в Json результата сопоставления:
    ```JSON
    {
      "MatchedCode": "rand()",
      "BeginLine": 60,
      "BeginColumn": 30,
      "EndLine": 60,
      "EndColumn": 36,
      "PatternKey": "27",
      "SourceFile": "Patterns.php"
    }
    ```
* `TextSpan` - линейная локация текста, которая содержит `Start` и `Length` - начало и длину соответственно. Используется во всех преобразованиях, кроме вывода.
* `LineColumnTextSpan` - локации текста, которая содержит `BeginLine`, `BeginColumn`, `EndLine` и `EndColumn`. Используется при выводе найденного шаблона в удобной форме.

## Логгирование

* `ILogger` - интерфейс, абстрагирующий методы логгирования:
    * `LogError` - логгирование ошибок в текстовом формате или в формате исключений `Exception`. Исключения бывают `ParsingException`, `ConversionException`, `MatchingException` и общий `Exception`.
    * `LogInfo` - логгирование информационных сообщений. Примеры:
        * `Command line arguments: -f Patterns.php --stage convert`
        * `File Patterns.php has been parsed (Elapsed: 00:00:00.6350338).`
    * `LogDebug` - логгирование Debug-сообщений. Они не используется в Release конфигурации. Примеры:
        * `Arithmetic expression 60 * 60 has been folded to 3600`
        * `Strings "a" + "b" has been concatenated to "ab"`

    Реализация `ConsoleLogger` находится в проекте **PT.PM.Console**. Данный логгер выводит результат в консоль и в файл. Внутри используется *NLog*.
* `ILoggable` - интерфейс со свойством `ILogger Logger`. Реализуется во всех классах, где используется логгирование. По умолчанию во всех классах используется реализация `DummyLogger`, которая не делает ничего. Используется, чтобы убрать необходимость проверять `Logger` не `null` перед вызовом методов `LogError`, `LogInfo` и т.д.

## Узлы UST

* `Children` - ближайшие потомки текущего узла (сыновья).
* `Descendant` - все потомки текущего узла (сыновья, внуки и т.д.).

Базовым классом для всех узлов является `UstNode`. Он имеет следующие свойства:
* `NodeType` - тип узла. Используется при сравнении узлов.
* `Parent` - родитель. Для корневого узла - `null`.
* `Children` - список детей. Используется при обходе `Ust`.
* `TextSpan` - локация в тексте в линейных координатах.

И методы:
* `CompareTo` - базовая реализация для сравнения двух узлов друг с другом. Подробнее о нем написано в статье "Tree structures processing and unified AST" в разделе [Algorithm for matching AST and patterns](Articles/Tree-structures-processing-and-unified-AST/English.md#algorithm-for-matching-ast-and-patterns).
* `DoesAnyDescendantMatchPredicate` - сопоставляется какой-нибудь потомок с переданным предикатом. Используется, например, для `PatternExpressionInsideExpression`.
* `DoesAllDescendantsMatchPredicate` - сопоставляются ли все потомки с переданным предикатом.
* `ApplyActionToDescendants` - применяет действие ко всем потомкам. Используется для добавления смещения локации текста при парсинге островных языков.
* `GetAllDescendants` - возвращает всех потомков данного узла.
* `ToString` - возвращает строковое представление узла, которое по форматированию похоже на C# синтаксис.

### Основные типы узлов

* `CollectionNode<TAstNode> : UstNode` - коллекция узлов
* `Statement` - утверждение (обычно заканчивается точкой с запятой)
* `Expression` - выражение, возвращающее результат (например, вызов функции, арифметический оператор и т.д.)
    * `AnonymousMethodExpression` - анонимная функция
    * `AssignmentExpression` - присваивание `a = b`
    * `BinaryOperatorExpression` - бинарное выражение : `a * b`, `a + b`
    * `CastExpression` - привеседение к типу `(type)expr`
    * `ConditionalExpression` - условное выражение `a ? b : c`
    * `IndexerExpression` - индексатор `a[b]`
    * `InvocationExpression` - вызов функции `Target(Args)`
    * `MemberReferenceExpression` - обращение к члену класса `A.B.C`
    * `UnaryOperatorExpression` - унарное выражение `++a`, `!a`
    * `VariableDeclarationExpression` - объевление переменной, `var a = b`
    * `ObjectCreateExpression` - создание объекта `new A()`
    * `MultichildExpression` - искусственное `Expression`, содержащий в себе несколько `Expression`
    * `WrapperExpression` - искусственное `Expression`, содержащий в себе узел типа `UstNode`
* `Token` - терминальный узел:
    * Идентификатор `IdToken`
    * Примитивное значение `Literal` (строка, число, булево значение и т.д.):
        * `IntLiteral` - целое число `42`
        * `BooleanLiteral` - `true` или `false`
        * `CommentLiteral` - комментарий `// password="e@jf7!ke"`
        * `FloatLiteral` - вещественное число `42.42`
        * `NullLiteral` - `null`
        * `StringLiteral` - строка `"hello world"`
        * `UnaryOperatorLiteral` - унарный литерал
        * `BinaryOperatorLiteral` - бинарный литерал

### Типы узлов-паттернов

* `DslNode` - узел для оборачивания DSL информации (целевые языки, определения переменных).
* `PatternBooleanLiteral` - булевый литерал `bool`, `true` или `false`.
* `PatternComment` - позволяет находить комментарии по регулярному выражению, эквивалетно `Comment: regex` в DSL.
* `PatternExpression` - подстановочное выражение (может быть любым), эквивалентно `#` в DSL. Также можно использовать отрицание для выражений.
* `PatternExpressionInsideExpression`- оборачивает любое выражение и указывает, что оно может встретиться на любой глубине дерева. Эквивалентно `<{ expression }>` в DSL.
* `PatternExpressions` - узел для сопоставления нескольких выражений с учетом ограничений. Например, `HashBytes(^(md2|md4|md5)$, ...)`.
* `PatternIdToken` - подстановка идентификаторов по регулярному выражению. Например, `<[\w+]>` будет сопоставляться с любыми идентификаторы. 
* `PatternIntLiteral` - подстановка целых чисел по диапазону, например `<[..-20 || -10 || -5..5 ||  010 || 0x10 || 30..]>`.
* `PatternMultipleExpressions` - подстановка для произвольного количества любых выражений. Эквивалетно `...` в DSL. Используется, например, для аргументов функций.
* `PatternStatement` - оборачивает инструкцию.
* `PatternStatements` - оборачивает несколько инструкций.
* `PatternStringLiteral` - подстановка строк по регулярному выражению. Например, `<[""]>` будет сопоставляться с любыми строками.
* `PatternTryCatchStatement` - пустая конструкция `try catch { }`.
* `PatternVarDef` - определение переменной, которая может принимать несколько значений. Может быть как именованной `<["\w*" || null]>`, так и безымянной: `<[@pwd:password]>`.
* `PatternVarRef` - ссылка на закрепляющуюся переменную. Например, `Response.Cookies.Add(<[@cookie]>);`.

### Обход UST

Интерфейсы `IUstVisitor` и `IUstListener` для обхода `Ust` находятся в проекте **PT.PM.UstPreprocessing**.
Класс `UstVisitor` реализуется интерфейс `IUstVisitor` и по-умолчанию в нем происходит глубокое копирование деревьев.
Пока что используется динамическая диспетчеризация с использованием `dynamic`.

`UstPreprocessor` испольуется для упрощения `Ust`. В нем переопределены некоторые методы `UstVisitor`. По сути на этапе препроцессинга происходит трансформация `Ust` в более простое `Ust`. Смотри также 
[Simplifying an UAST](Articles/Tree-structures-processing-and-unified-AST/English.md#simplifying-an-uast).

## Ошибки и исключения

* `ParsingException` - возникает при парсинге исходного кода или шаблона, если в них есть лексические или синтаксические ошибки. Примеры:
    * `Error: no viable alternative at input '(?' at 1:1`
    * `Error: token recognition error at: '>' at 1:18`
* `ConversionException` - возникает при преобразовании дерева разбора в универсальное AST (Ust). Также используется при преобразовании шаблонов. Например, `NullReferenceException` по ошибке в каком-нибудь методе-визиторе.
* `MatchingException` - возникает во время работы алгоритма сопоставления шаблонов с узлами Ust.
* `ShouldNotBeVisitedException` - используется для того, чтобы явно указать в визитор-методе, что он не должен посещаться.

## Вспомогательные классы ANTLR

* `AntlrCaseInsensitiveInputStream` - регистронезависимый поток ввода, который используется, например, в PHP, PL/SQL, T-SQL.
* `AntlrDefaultVisitor` - реализация визитора по-умолчанию для ANTLR деревьев разбора.
    * `Visit` - вызывает `tree.Accept(this)` для конкретного узла. Содержит обработчик исключений.
    * `VisitChildren`
        * Если child один, то для него вызывается `Visit`
        * В случае, если child несколько,  возвращает наследника `MultichildExpression`, наследника `Expression`
    * `VisitTerminal` - пытается распарсить значение разными регулярными выражениями и на основе этого определить тип (`String`, `Float`, `Int` и т.д.)
* `AntlrHelper` - конвертинг ANTLR локаций текста в унифицированные, вывод дерева разбора в удобном текстовом представлении.
* `AntlrMemoryErrorListener` - логгирование ANTLR ошибок лексера и парсера.
* `AntlrParser` - базовый класс для всех ANTLR парсеров. Релизует следующие вещи:
    * Виртуальный метод предобработки текста `virtual string PreprocessText`, который по-умолчанию нормализует разрывы строк (заменяет единичные `\r` на `\n`)
    * Парсинг кода сначала с помощью `SLL` алгоритма (более быстрого), а в случае неудачи с помощью полного `LL` (более медленного).
    * Отслеживание ANTLR кэша и его очистка при определенных условиях (потребление памяти).

## Другие вспомогательные классы

* `DummyLogger` - логгер-заглушка. Используется по-умолчанию у всех объектов, реализующих `ILoggable`, чтобы можно было писать `Logger.LogInfo` вместо `Logger?.LogInfo` и не схватить случайно `NullReferenceException`.
* `LanguageInfo` - информация о языке:
    * `Language Language` - непосредственно значение перечисления для данного языка.
    * `string Title` - титульное имя языка
    * `string[] Extensions` - расширения языка
    * `bool CaseInsensitive` - является ли язык чувствительным к регистру
    * `LanguageFlags DependentLanguages` - языки, исходники которых могут встречаться внутри исходников данного языка (островные языки). Например, `JavaScript` может встретиться внутри `PHP`.
    * `bool HaveAntlrParser` - используется ли для данного языка Antlr - парсер.
* `LanguageExt` - содержит в себе статический словарь с поддерживаемыми `LanguageInfo` и методы для работы с ними.
* `LanguageDetector` - детектор исходного кода по фрагменту. Имеет одну реализацию `ParserLanguageDetector`, которая парсит фрагмент различными парсерами и выбирает тот язык, у которого количество ошибок парсинга минимальное.
* `TextHelper` - различные утилиты для работы с текстом:
    * `LinearToLineColumn` - преобразование линейных координат в двухмерные.
    * `LineColumnToLinear` - преобразование двухмерных координат в линейные.
    * `GetLinesCount` - возвращает количество разрывов строк для строки.
    * `NormDirSeparator` - нормализовать оператор разделения директорий. Используется для приведения физического адреса к правильному формату, т.к. в Windows используются обратные слэши, а в Linux - прямые.
* `WorkflowLoggerHelper` - вывод информации и статистики после процесса сопоставления с шаблонами.
* `UstDotRenderer` - рендеринг Ust дерева в dot формат, который можно визуализировать с помощью Graphviz.
* `GraphvizGraph` - сохраняет переданный граф в формате `dot` в изображение. Поддерживаются форматы: `Bmp, Png, Jpg, Plain, Svg, Pdf, Gif, Dot`. По умолчанию используется `Png`.
* `TestHelper` - разные утилиты для юнит-тестов.