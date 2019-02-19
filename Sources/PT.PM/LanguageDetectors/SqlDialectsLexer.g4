lexer grammar SqlDialectsLexer;

channels {ERRORCHANNEL}

MY_SQL : DELIMITER | NAMES | UNDOFILE | ENGINE | INITIAL_SIZE 
       | REVERCE_QUOTE_ID;

fragment DELIMITER: 'DELIMITER';
fragment NAMES: 'NAMES';
fragment UNDOFILE: 'UNDOFILE';
fragment ENGINE: 'ENGINE';
fragment INITIAL_SIZE: 'INITIAL_SIZE';
fragment REVERCE_QUOTE_ID: '`' ~'`'+ '`';

PL_SQL    
      : AUTOEXTEND | REFRESH | STORE | VALIDATE | EXTENT
      | MANAGEMENT | TEMPFILE | INITRANS | LOB | WHENEVER
      | PL_NOT_EQUAL | HASHKEYS | STRUCTURE | BIGFILE | BUILD | NESTED | NEW;

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


T_SQL
    : TOP | SERVICE | PRINT | LOGIN | CONVERSATION | BROKER
    | LOCAL_SERVICE_NAME | PRIORITY_LEVEL | REMOTE_SERVICE_NAME
    | GOVERNOR | SYMMETRIC | TAPE | TARGET | OUTPUT | POOL | PROVIDER | ROUTE
    | MESSAGE | CONTRACT | SQUARE_BRACKET_ID;

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
fragment SQUARE_BRACKET_ID:  '[' ~']'+ ']';


ID:      ( [A-Z_#] | FullWidthLetter) ( [A-Z_#$@0-9] | FullWidthLetter )*;
LOCAL_ID:'@' ([A-Z_$@#0-9] | FullWidthLetter)+;
fragment FullWidthLetter
    : '\u00c0'..'\u00d6'
    | '\u00d8'..'\u00f6'
    | '\u00f8'..'\u00ff'
    | '\u0100'..'\u1fff'
    | '\u2c00'..'\u2fff'
    | '\u3040'..'\u318f'
    | '\u3300'..'\u337f'
    | '\u3400'..'\u3fff'
    | '\u4e00'..'\u9fff'
    | '\ua000'..'\ud7ff'
    | '\uf900'..'\ufaff'
    | '\uff00'..'\ufff0'
    ;
    
SINGLE_LINE_COMMENT: '--' ~('\r' | '\n')* NEWLINE_EOF   -> channel(HIDDEN);
MULTI_LINE_COMMENT:  '/*' .*? '*/'                      -> channel(HIDDEN);
fragment NEWLINE_EOF    : NEWLINE | EOF;
fragment NEWLINE        : '\r'? '\n';

ERROR_RECONGNIGION:   .   -> channel(ERRORCHANNEL);