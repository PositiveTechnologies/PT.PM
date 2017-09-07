lexer grammar DslLexer;

tokens { PatternMultDiv, PatternPlusMinus, PatternColon }

@lexer::members
{int PrevTokenType = Eof;

public override IToken NextToken()
{
	IToken token = base.NextToken();
	PrevTokenType = token.Type;
	return token;
}

private bool IsPrevTokenNumeric()
{
	return PrevTokenType == DslLexer.PatternInt ||
           PrevTokenType == DslLexer.PatternHex ||
           PrevTokenType == DslLexer.PatternOct;
}}

Whitespace: [ \r\n\t]+           -> skip;
SingleLineComment: '//' ~[\r\n]* -> channel(HIDDEN);
MultiLineComment: '/*' .*? '*/'  -> channel(HIDDEN);

Comment:   C 'omment';
Null:     'null';
Bool:     'true' | 'false';
New:      'new';
Try:      'try';
Catch:    'catch';
Finally:  'finally';
If:       'if';
Function: 'function';
Class:    'class';
BaseReference:       'base';
OpenCurlyBracket:    '{';
CloseCurlyBracket:   '}';

Sharp:                    '#';
Ellipsis:                 '...';
Range:                    '..';
Dot:                      '.';
Plus:                     '+';
OpenSquareBracket:        '[';
CloseSquareBraket:        ']';
Semicolon:                ';';
Colon:                    ':';
Not:                      '~';
Compare:                  '==';
NotEq:                    '!=';
Eq:                       '=';
Comma:                    ',';
Minus:                    '-';
OpenRoundBracket:         '(';
CloseRoundBracket:        ')';
Opt:                      '?';
Star:                     '*';
Slash:                    '/';
OpenPatternMark:          '<['    -> pushMode(PATTERN_MODE);
ShortPatternOr:           '<|>';
ShortPatternAnd:          '<&>';
ShortPatternNot:          '<~>';
OpenArbitraryDepthMark:   '<{';
CloseArbitraryDepthMark:  '}>';
CloseParenthesisQuestion: ')?';
DslEnd:                   '?' '>'   -> skip, popMode;

Id:                   [a-zA-Z_$][a-zA-Z0-9_$]*;
String:               '"' (~[\"] | '\\' . )* '"';
Oct:                  '0'   OctDigit+;
Int:                  '-'?  Digit+;
Hex:                  '0' X HexDigit+;
Float:                '-'? Digit* '.'? Digit+;

mode PATTERN_MODE;

PatternIdWhitespace:  [ \r\n\t]+            -> skip;

Args:                 'args';
Stmt:                 'stmt';
Stmts:                'stmts';
Expr:                 'expr';
PatternNull:          'null';
PatternBool:          'bool' | 'true' | 'false';
PatternString:        '"' (~[\"] | '\\' . )* '"';
PatternOct:           '0'   OctDigit+;
PatternInt:           '-'?  Digit+;
PatternHex:           '0' X HexDigit+;
PatternVar:           '@' [a-zA-Z_][a-zA-Z0-9_]*;
PatternId:            ~(' ' | '\t' | [\]|.~:*/+-])+;
ClosePatternMark:     ']>'               -> popMode;
PatternOr:            '||';
PatternEllipsis:      '...';
PatternRange:         '..';
PatternNot:           '~';
PatternColon:         ':' { PrevTokenType == DslLexer.PatternVar }? -> type(PatternColon);
PatternMultDiv:       [*/] { IsPrevTokenNumeric() }?                -> type(PatternMultDiv);
PatternPlusMinus:     [+-] { IsPrevTokenNumeric() }?                -> type(PatternPlusMinus);
PatternMore:          [\]|.:*/+-]                                   -> type(PatternId);

fragment A: [aA];
fragment B: [bB];
fragment C: [cC];
fragment D: [dD];
fragment E: [eE];
fragment F: [fF];
fragment G: [gG];
fragment H: [hH];
fragment I: [iI];
fragment J: [jJ];
fragment K: [kK];
fragment L: [lL];
fragment M: [mM];
fragment N: [nN];
fragment O: [oO];
fragment P: [pP];
fragment Q: [qQ];
fragment R: [rR];
fragment S: [sS];
fragment T: [tT];
fragment U: [uU];
fragment V: [vV];
fragment W: [wW];
fragment X: [xX];
fragment Y: [yY];
fragment Z: [zZ];

fragment OctDigit:     [0-7];
fragment Digit:        [0-9];
fragment HexDigit:     [0-9a-fA-F];