INCLUDE basic/module.fs

Export BeginModule Shell
 
: ShOk ( c-addr u -- )
    2DUP SYSTEM
    $? 0<> if S" ERROR: COMMAND FAILED: " 2SWAP S+ Fail THEN
;

EndModule