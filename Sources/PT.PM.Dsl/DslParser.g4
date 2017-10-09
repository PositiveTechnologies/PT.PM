parser grammar DslParser;

options { tokenVocab=DslLexer; }

pattern
    : dslCode+ EOF
    ;

dslCode
    : statement+
    | expression
    | Comment ':' '<[' PatternString ('||' PatternString)* ']>'
    ;

statement
    : ('<[' PatternNot ']>')? expression ';'               #ExpressionStatement
    | '<[' Stmt ']>' ';'?                                  #PatternStatement
    | ('<[' Stmts ']>' ';'? | Ellipsis)                    #PatternMultipleStatement
    | 'try'
      'catch' ('(' literalOrPatternId (',' literalOrPatternId)* ')')?
      '{' Ellipsis? '}'                                    #PatternTryCatchStatement
    ;

expression
    : modifiers+=literalOrPatternId* 'class' name=literalOrPatternId?
      (':' baseTypes+=literalOrPatternId (',' baseTypes+=literalOrPatternId)*)?
      '{' arbitraryDepthExpression? '}'                    #ClassDeclaration

    | modifiers+=literalOrPatternId* methodName=literalOrPatternId '(' ')'
      '{' (expression | Ellipsis)? '}'                     #MethodDeclaration

    | Field? modifiers+=literalOrPatternId*
      type=literalOrPatternId variableName                 #VarOrFieldDeclarationExpression

    | expression '.' literalOrPatternId                    #MemberReferenceExpression
    | expression '(' args? ')'                             #InvocationExpression
    | expression op=('*' | '/') expression                 #BinaryOperatorExpression
    | expression op=('+' | '-') expression                 #BinaryOperatorExpression
    | expression '[' expression ']'                        #IndexerExpression
    | '(' expression '.' ')?' literalOrPatternId           #MemberReferenceOrLiteralExpression
    | expression op=('==' | '!=') expression               #ComparisonExpression
    | expression '=' expression                            #AssignmentExpression
    | 'new' literalOrPatternId '(' args? ')'               #ObjectCreationExpression
    | 'function' '{' expression '}'                        #FunctionExpression
    | patternLiterals                                      #PatternLiteralExpression
    | literal                                              #LiteralExpression
    | ('<[' Expr ']>' | '#' )                              #PatternExpression
    | expression (('<[' '||' ']>' | '<|>') expression)+    #PatternOrExpression
    | expression ('<&>' expression)+                       #PatternAndExpression
    | '<~>' expression                                     #PatternNotExpression
    | arbitraryDepthExpression                             #PatternArbitraryDepthExpression
    | 'throw' expression                                   #PatternThrowExpression
    | 'return' expression                                  #PatternReturnExpression
    | '(' expression ')'                                   #ParenthesisExpression
    | BaseReference                                        #BaseReferenceExpression
    ;

// We need this rule for for backward compatibility and bypass of incorrect determination
// of the $(<[""]>) as  VarOrFieldDeclarationExpression
variableName
    : literalOrPatternId
    | patternLiterals
    ;

arbitraryDepthExpression
    : '<{' expression '}>'
    ;

args
    : arg (',' arg)*
    ;

arg
    : expression
    | ('<[' (Args | PatternEllipsis) ']>' | Ellipsis | '#' '*')
    ;

literal
    : Id
    | String
    | Oct
    | Int
    | Hex
    | Bool
    | Null
    ;

literalOrPatternId
    : Id
    | '<[' patternId ('||' patternId)* ']>'
    ;

patternLiterals
    : '<[' (PatternVar | (PatternVar PatternColon)? patternNotLiteral ('||' patternNotLiteral)*) ']>'
    ;

patternNotLiteral
    : PatternNot? patternLiteral
    ;

patternLiteral
    : PatternString                                                                #PatternStringLiteral
    | patternId                                                                    #PatternIdToken
    | (  i=patternIntExpression
      | i1=patternIntExpression? PatternRange i2=patternIntExpression?)            #PatternIntLiteral
    | PatternBool                                                                  #PatternBoolLiteral
    | PatternNull                                                                  #PatternNullLiteral
    ;

patternIntExpression
    : left=patternIntExpression op=PatternMultDiv   right=patternIntExpression
    | left=patternIntExpression op=PatternPlusMinus right=patternIntExpression
    | patternInt
    ;

patternInt
    : PatternOct
    | PatternInt
    | PatternHex
    ;

patternId
    : PatternId+
    ;
