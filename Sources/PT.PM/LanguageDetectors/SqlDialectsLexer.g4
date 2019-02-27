lexer grammar SqlDialectsLexer;

channels { MY_SQL, PL_SQL, MY_PL_SQL, PL_T_SQL, T_SQL }

@lexer::members
{protected bool IsNewlineAtPos(int pos)
{
    int la = _input.La(pos);
    return la == -1 || la == '\n';
}}

// Sql common

ADD                 : 'ADD';
ALTER               : 'ALTER';
ALL                 : 'ALL';
AND                 : 'AND';
ANY                 : 'ANY';
AS                  : 'AS';
ASC                 : 'ASC';
BETWEEN             : 'BETWEEN';
BY                  : 'BY';
CASE                : 'CASE';
CHECK               : 'CHECK';
COLUMN              : 'COLUMN';
CONSTRAINT          : 'CONSTRAIN';
CREATE              : 'CREATE';
REPLACE             : 'REPLACE';
DATABASE            : 'DATABASE';
DEFAULT             : 'DEFAULT';
DELETE              : 'DELETE';
DESC                : 'DESC';
DISTINCT            : 'DISTINCT';
DROP                : 'DROP';
EXEC                : 'EXEC';
EXISTS              : 'EXISTS';
FOREIGN             : 'FOREIGN';
INTO                : 'INTO';
IS                  : 'IS';
KEY                 : 'KEY';
FROM                : 'FROM';
FULL                : 'FULL';
NULL                : 'NULL';
JOIN                : 'JOIN';
LEFT                : 'LEFT';
LIKE                : 'LIKE';
NOT                 : 'NOT';
OR                  : 'OR';
ORDER               : 'ORDER';
PRIMARY             : 'PRIMARY';
PROCEDURE           : 'PROCEDURE';
RIGHT               : 'RIGHT';
SELECT              : 'SELECT';
SET                 : 'SET';
TABLE               : 'TABLE';
TRUNCATE            : 'TRUNCATE';
UNION               : 'UNION';
UNIQUE              : 'UNIQUE';
UPDATE              : 'UPDATE';
VALUES              : 'VALUES';
VIEW                : 'VIEW';
WHERE               : 'WHERE';
OUTER               : 'OUTER';
GROUP               : 'GROUP';
HAVING              : 'HAVING';
IN                  : 'IN';
INDEX               : 'INDEX';
INNER               : 'INNER';
INSERT              : 'INSERT';

// MySql & PlSql

BOTH                : 'BOTH'                -> channel(MY_PL_SQL);
IGNORE              : 'IGNORE'              -> channel(MY_PL_SQL);
LIMIT               : 'LIMIT'               -> channel(MY_PL_SQL);
TRIM                : 'TRIM'                -> channel(MY_PL_SQL);
STDDEV              : 'STDDEV'              -> channel(MY_PL_SQL);
SYSDATE             : 'SYSDATE'             -> channel(MY_PL_SQL);
SUBSTR              : 'SUBSTR'              -> channel(MY_PL_SQL);
INCLUDE             : 'INCLUDE'             -> channel(MY_PL_SQL);

// PlSql & TSql

STATISTICS          : 'STATISTICS'          -> channel(PL_T_SQL);
ASSEMBLY            : 'ASSEMBLY'            -> channel(PL_T_SQL);
MASTER              : 'MASTER'              -> channel(PL_T_SQL);
LOCATION            : 'LOCATION'            -> channel(PL_T_SQL);
CREDENTIAL          : 'CREDENTIAL'          -> channel(PL_T_SQL);
LIBRARY             : 'LIBRARY'             -> channel(PL_T_SQL);
AVAILABILITY        : 'AVAILABILITY'        -> channel(PL_T_SQL);

// MySql

DELIMETER           : 'DELIMITER'           -> channel(MY_SQL);
NAMES               : 'NAMES'               -> channel(MY_SQL);
UNDOFILE            : 'UNDOFILE'            -> channel(MY_SQL);
ENGINE              : 'ENGINE'              -> channel(MY_SQL);
INITIAL_SIZE        : 'INITIAL_SIZE'        -> channel(MY_SQL);
MY_SQL_ID           : '`' ~'`'+ '`'         -> channel(MY_SQL);

// PlSql

AUTOEXTEND          : 'AUTOEXTEND'          -> channel(PL_SQL);
REFRESH             : 'REFRESH'             -> channel(PL_SQL);
STORE               : 'STORE'               -> channel(PL_SQL);
VALIDATE            : 'VALIDATE'            -> channel(PL_SQL);
EXTENT              : 'EXTENT'              -> channel(PL_SQL);
MANAGEMENT          : 'MANAGEMENT'          -> channel(PL_SQL);
TEMPFILE            : 'TEMPFILE'            -> channel(PL_SQL);
INITRANS            : 'INITRANS'            -> channel(PL_SQL);
LOB                 : 'LOB'                 -> channel(PL_SQL);
WHENEVER            : 'WHENEVER'            -> channel(PL_SQL);
HASHKEYS            : 'HASHKEYS'            -> channel(PL_SQL);
STRUCTURE           : 'STRUCTURE'           -> channel(PL_SQL);
BIGFILE             : 'BIGFILE'             -> channel(PL_SQL);
BUILD               : 'BUILD'               -> channel(PL_SQL);
NESTED              : 'NESTED'              -> channel(PL_SQL);
NEW                 : 'NEW'                 -> channel(PL_SQL);
ROWNUM              : 'ROWNUM'              -> channel(PL_SQL);
KEEP                : 'KEEP'                -> channel(PL_SQL);
NVL                 : 'NVL'                 -> channel(PL_SQL);
COMPILE             : 'COMPILE'             -> channel(PL_SQL);
ASSOCIATE           : 'ASSOCIATE'           -> channel(PL_SQL);
PACKAGES            : 'PACKAGES'            -> channel(PL_SQL);
TO_CHAR             : 'TO_CHAR'             -> channel(PL_SQL);
SHRINK              : 'SHRINK'              -> channel(PL_SQL);
NOLOGGING           : 'NOLOGGING'           -> channel(PL_SQL);
GUARANTEE           : 'GUARANTEE'           -> channel(PL_SQL);
NOGUARANTEE         : 'NOGUARANTEE'         -> channel(PL_SQL);
MINUS               : 'MINUS'               -> channel(PL_SQL);
VARCHAR2            : 'VARCHAR2'            -> channel(PL_SQL);
CMD                 : '@' {IsNewlineAtPos(-2)}? ~('\r' | '\n')*                  -> channel(PL_SQL);
REMARK_COMMENT      : 'REM' {IsNewlineAtPos(-4)}? 'ARK'? (' ' ~('\r' | '\n')*)?  -> channel(PL_SQL);
PROMPT_MESSAGE      : 'PRO' {IsNewlineAtPos(-4)}? 'MPT'? (' ' ~('\r' | '\n')*)?  -> channel(PL_SQL);

// TSql

TOP                 : 'TOP'                 -> channel(T_SQL);
SERVICE             : 'SERVICE'             -> channel(T_SQL);
PRINT               : 'PRINT'               -> channel(T_SQL);
LOGIN               : 'LOGIN'               -> channel(T_SQL);
CONVERSATION        : 'CONVERSATION'        -> channel(T_SQL);
BROKER              : 'BROKER'              -> channel(T_SQL);
LOCAL_SERVICE_NAME  : 'LOCAL_SERVICE_NAME'  -> channel(T_SQL);
PRIORITY_LEVEL      : 'PRIORITY_LEVEL'      -> channel(T_SQL);
REMOTE_SERVICE_NAME : 'REMOTE_SERVICE_NAME' -> channel(T_SQL);
GOVERNOR            : 'GOVERNOR'            -> channel(T_SQL);
SYMMETRIC           : 'SYMMETRIC'           -> channel(T_SQL);
TAPE                : 'TAPE'                -> channel(T_SQL);
TARGET              : 'TARGET'              -> channel(T_SQL);
OUTPUT              : 'OUTPUT'              -> channel(T_SQL);
POOL                : 'POOL'                -> channel(T_SQL);
PROVIDER            : 'PROVIDER'            -> channel(T_SQL);
ROUTE               : 'ROUTE'               -> channel(T_SQL);
MESSAGE             : 'MESSAGE'             -> channel(T_SQL);
CONTRACT            : 'CONTRACT'            -> channel(T_SQL);
GO                  : 'GO'                  -> channel(T_SQL);
DBCC                : 'DBCC'                -> channel(T_SQL);
NO_INFOMSGS         : 'NO_INFOMSGS'         -> channel(T_SQL);
RESOURCE_MANAGER_LOCATION : 'RESOURCE_MANAGER_LOCATION' -> channel(T_SQL);
NOLOCK              : 'NOLOCK'              -> channel(T_SQL);
EXTERNAL_ACCESS     : 'EXTERNAL_ACCESS'     -> channel(T_SQL);
PERMISSION_SET      : 'PERMISSION_SET'      -> channel(T_SQL);
KEY_STORE_PROVIDER_NAME : 'KEY_STORE_PROVIDER_NAME' -> channel(T_SQL);
KEY_PATH            : 'KEY_PATH'            -> channel(T_SQL);
BRACKET_ID          : '[' (ID_FRAGMENT | [ \t])+ ']'                  -> channel(T_SQL);

L_PAREN             : '(';
R_PAREN             : ')';
EQUAL               : '=';
COMMA               : ',';
COLON               : ':';
SEMICOLON           : ';';
DOT                 : '.';
STAR                : '*';
DIV                 : '/';
MOD                 : '%';
PLUS                : '+';
HYPHEN              : '-';
GREATE              : '>';
LESS                : '<';
EXCLAMATION         : '!';
BAR                 : '|';
AMPERSAND           : '&';
AT                  : '@';
TILDE               : '~';

ID                  : ID_FRAGMENT;
MEMBER_REF          : ID_FRAGMENT ('.' (ID_FRAGMENT | '*'))+;
INT                 : DEC_DIGIT+;
REAL                : (DEC_DOT_DEC EXP? | DEC_DIGIT EXP);
SINGLE_LINE_COMMENT : ('--' | '//' ) ~('\r' | '\n')*;
MY_SQL_COMMENT      : '#' {IsNewlineAtPos(-2)}? ~('\r' | '\n')*       -> channel(MY_SQL);
MULTI_LINE_COMMENT  :  '/*' (MULTI_LINE_COMMENT | .)*? '*/';
SPACE               : [ \t]+;
NEW_LINE            : '\r'? '\n';
STRING              : ('N'? '\'' (~'\'' | '\'\'')* '\'')
                    | ('"' ( '\\'. | '""' | ~('"'| '\\') )* '"')
                    ;

OTHER               : .;

fragment ID_FRAGMENT: [A-Z_$][A-Z0-9_$#]*;
fragment EXP        : 'E' [+-]? DEC_DIGIT+;
fragment DEC_DOT_DEC: (DEC_DIGIT+ '.' DEC_DIGIT+ |  DEC_DIGIT+ '.' | '.' DEC_DIGIT+);
fragment DEC_DIGIT  : [0-9];