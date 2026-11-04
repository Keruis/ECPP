REQUIRE basic/module.fs

Export BeginModule Shell
 
: ShOk ( c-addr u -- )
    2DUP SYSTEM
    $? 0<> if S" Error: Command Failed: " 2SWAP S+ Fail THEN
;

EndModule