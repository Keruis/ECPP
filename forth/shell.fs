INCLUDE basic/module.fs

Export BeginModule SHELL
 
: ShOk ( c-addr u -- )
    2DUP SYSTEM
    $? 0<> if ." ERROR: COMMAND FAILED: " TYPE THEN
;

EndModule