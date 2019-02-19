lexer grammar SqlDialectsLexer;

channels {ERRORCHANNEL}

SINGLE_LINE_COMMENT: '--' ~('\r' | '\n')* NEWLINE_EOF   -> channel(HIDDEN);
MULTI_LINE_COMMENT:  '/*' .*? '*/'                      -> channel(HIDDEN);
SPACE: [ \t\r\n]+                                       -> channel(HIDDEN);
SEICOLON: ';';

MY_SQL : DELIMITER | NAMES | UNDOFILE | ENGINE | INITIAL_SIZE 
       | REVERCE_QUOTE_ID | RETURNS;

fragment DELIMITER: 'DELIMITER';
fragment NAMES: 'NAMES';
fragment UNDOFILE: 'UNDOFILE';
fragment ENGINE: 'ENGINE';
fragment INITIAL_SIZE: 'INITIAL_SIZE';
fragment REVERCE_QUOTE_ID: '`' ~'`'+ '`';
fragment RETURNS: 'RETURNS';

PL_SQL    
      : AUTOEXTEND | REFRESH | STORE | VALIDATE | EXTENT
      | MANAGEMENT | TEMPFILE | INITRANS | LOB | WHENEVER
      | PL_NOT_EQUAL | HASHKEYS | STRUCTURE | BIGFILE 
      | BUILD | NESTED | NEW | INTEGER;

fragment AUTOEXTEND: 'AUTOEXTEND'; 
fragment REFRESH: 'REFRESH'; 
fragment STORE: 'STORE'; 
fragment VALIDATE: 'VALIDATE'; 
fragment EXTENT: 'EXTENT'; 
fragment MANAGEMENT: 'MANAGEMENT'; 
fragment TEMPFILE: 'TEMPFILE'; 
fragment INITRANS: 'INITRANS'; 
fragment LOB: 'LOB'; 
fragment WHENEVER: 'WHENEVER';
fragment PL_NOT_EQUAL: '<>';
fragment HASHKEYS: 'HASHKEYS'; 
fragment STRUCTURE: 'STRUCTURE'; 
fragment BIGFILE: 'BIGFILE'; 
fragment BUILD: 'BUILD'; 
fragment NESTED: 'NESTED'; 
fragment NEW: 'NEW';
fragment INTEGER: 'INTEGER';


T_SQL
    : TOP | SERVICE | PRINT | LOGIN | CONVERSATION | BROKER
    | LOCAL_SERVICE_NAME | PRIORITY_LEVEL | REMOTE_SERVICE_NAME
    | GOVERNOR | SYMMETRIC | TAPE | TARGET | OUTPUT | POOL | PROVIDER | ROUTE
    | MESSAGE | CONTRACT | GO | SQUARE_BRACKET_ID;

fragment TOP: 'TOP';
fragment SERVICE: 'SERVICE';
fragment PRINT: 'PRINT';
fragment LOGIN: 'LOGIN';
fragment CONVERSATION: 'CONVERSATION';
fragment BROKER: 'BROKER';
fragment LOCAL_SERVICE_NAME: 'LOCAL_SERVICE_NAME';
fragment PRIORITY_LEVEL: 'PRIORITY_LEVEL';
fragment REMOTE_SERVICE_NAME: 'REMOTE_SERVICE_NAME';
fragment GOVERNOR: 'GOVERNOR';
fragment SYMMETRIC: 'SYMMETRIC';
fragment TAPE: 'TAPE';
fragment TARGET: 'TARGET';
fragment OUTPUT: 'OUTPUT';
fragment POOL: 'POOL';
fragment PROVIDER: 'PROVIDER';
fragment ROUTE: 'ROUTE';
fragment MESSAGE: 'MESSAGE';
fragment CONTRACT: 'CONTRACT';
fragment GO: 'GO';
fragment SQUARE_BRACKET_ID:  '[' ~']'+ ']';

ERROR_RECONGNIGION:   . -> channel(ERRORCHANNEL);

fragment NEWLINE_EOF    : NEWLINE | EOF;
fragment NEWLINE        : '\r'? '\n';