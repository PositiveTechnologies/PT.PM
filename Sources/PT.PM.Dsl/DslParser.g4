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
      '{' (arbitraryDepthExpression | Ellipsis)? '}'       #MethodDeclaration

    | expression '.' literalOrPatternId                    #MemberReferenceExpression
    | expression '(' args? ')'                             #InvocationExpression
    | expression op=('*' | '/') expression                 #BinaryOperatorExpression
    | expression op=('+' | '-') expression                 #BinaryOperatorExpression
    | expression '[' expression ']'                        #IndexerExpression
    | '(' expression '.' ')?' literalOrPatternId           #MemberReferenceOrLiteralExpression
    | expression op=('==' | '!=') expression               #ComparisonExpression
    | expression '=' expression                            #AssignmentExpression
    | literalOrPatternId expression '=' expression         #VariableDeclarationExpression
    | 'new' literalOrPatternId '(' args? ')'               #ObjectCreationExpression
    | 'function' '{' expression '}'                        #FunctionExpression
    | patternLiterals                                      #PatternLiteralExpression
    | literal                                              #LiteralExpression
    | ('<[' Expr ']>' | '#' )                              #PatternExpression
    | expression (('<[' '||' ']>' | '<|>') expression)+    #PatternOrExpression
    | expression ('<&>' expression)+                       #PatternAndExpression
    | '<~>' expression                                     #PatternNotExpression
    | arbitraryDepthExpression                             #PatternArbitraryDepthExpression
    | '(' expression ')'                                   #ParenthesisExpression
    | BaseReference                                        #BaseReferenceExpression
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
