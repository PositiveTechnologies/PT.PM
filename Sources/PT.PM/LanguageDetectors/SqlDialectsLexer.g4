lexer grammar SqlDialectsLexer;

channels {ERRORCHANNEL}

SINGLE_LINE_COMMENT: '--' ~('\r' | '\n')* NEWLINE_EOF   -> channel(HIDDEN);
MULTI_LINE_COMMENT:  '/*' .*? '*/'                      -> channel(HIDDEN);
SPACE: [ \t\r\n]+                                       -> channel(HIDDEN);
SEMICOLON: ';';

fragment NEWLINE_EOF    : NEWLINE | EOF;
fragment NEWLINE        : '\r'? '\n';

MY_SQL
    : 'DELIMITER'
    | 'NAMES'
    | 'UNDOFILE'
    | 'ENGINE'
    | 'INITIAL_SIZE'
    | '`' ~'`'+ '`'
    | 'RETURNS';

PL_SQL    
    : 'AUTOEXTEND'
    | 'REFRESH'
    | 'STORE'
    | 'VALIDATE'
    | 'EXTENT'
    | 'MANAGEMENT' 
    | 'TEMPFILE' 
    | 'INITRANS' 
    | 'LOB'
    | 'WHENEVER'
    | '<>'
    | 'HASHKEYS' 
    | 'STRUCTURE' 
    | 'BIGFILE' 
    | 'BUILD' 
    | 'NESTED' 
    | 'NEW'
    | 'INTEGER';


T_SQL
    : 'TOP'
    | 'SERVICE'
    | 'PRINT'
    | 'LOGIN'
    | 'CONVERSATION'
    | 'BROKER'
    | 'LOCAL_SERVICE_NAME'
    | 'PRIORITY_LEVEL'
    | 'REMOTE_SERVICE_NAME'
    | 'GOVERNOR'
    | 'SYMMETRIC'
    | 'TAPE'
    | 'TARGET'
    | 'OUTPUT'
    | 'POOL'
    | 'PROVIDER'
    | 'ROUTE'
    | 'MESSAGE'
    | 'CONTRACT'
    | 'GO'
    | '[' ~']'+ ']';

ERROR_RECONGNIGION:   . -> channel(ERRORCHANNEL);