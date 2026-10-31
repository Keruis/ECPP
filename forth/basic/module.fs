INCLUDE cc.fs

32 CONSTANT MAX_MOD_DEPTH

: (RefillInput) ( -- )
    REFILL 0= 
    IF
        CR ." END OF IMPUT" CR ABORT
    THEN
;

: (ParseNameRefill) ( "name" -- c-addr u )
    CC_Begin
        PARSE-NAME DUP 0=
        CC_If
            2DROP
            (RefillInput)
        CC_Else
            2DUP S" \" COMPARE 0= 
            CC_If
                2DROP
                SOURCE NIP >IN !
                (RefillInput)
                CC_Continue
            CC_Then

            2DUP S" (" COMPARE 0=
            CC_If
                2DROP
                [CHAR] ) PARSE 2DROP
                CC_Continue
            CC_Then
            EXIT
        CC_Then
    CC_Again
;

: (CreateModuleList) ( pub-wid -- base-addr )
    (ParseNameRefill) NEXTNAME
    CREATE HERE >R
    , 0 ,
    MAX_MOD_DEPTH CELLS ALLOT
    R>
;

: (GetModulePubWidAddr) ( base-addr -- pub-wid-addr )

;

: (GetModuleCount) ( base-addr -- u)
    1 CELLS +
;

: (ModuleDepthAt) ( base-addr u -- depth-addr )
    2 + CELLS +
;

: (AddModuleDepth) ( mod-addr base-addr -- )
    DUP (GetModuleCount) DUP @
    DUP MAX_MOD_DEPTH >= IF
        CR ." MODULE ERR: MODULE DEPENDENCY LIMIT EXCEEDED"
        CR ABORT
    THEN

    DUP 1+ ROT !

    ROT >R
    (ModuleDepthAt)
    R> SWAP !
;

: (GetModule) ( "name" -- base-addr )
    (ParseNameRefill) 2DUP FIND-NAME \ c-addr u nt/0
    DUP 0<> IF
        NAME>INT >BODY
        NIP NIP
        EXIT
    THEN
    DROP
    CR ." MODULE [" type ." ] NOT FOUND" CR ABORT 
;

: (GetModuleList) ( base-addr -- wid...u u )
    DUP (GetModulePubWidAddr) @
    SWAP 1 OVER (GetModuleCount)
    @ 0 U+DO
        OVER I (ModuleDepthAt) @
        SWAP >R SWAP >R
        RECURSE R> R> ROT +
    LOOP NIP
;

: BeginModule ( "name" -- old-wid pub-wid base-addr )
    GET-CURRENT
    WORDLIST DUP (CreateModuleList)
    WORDLIST DUP SET-CURRENT
    >R GET-ORDER 1+ R> SWAP SET-ORDER
;

: EndModule ( old-wid pub-wid base-addr -- )
    2DROP
    SET-CURRENT
    GET-ORDER 1- NIP SET-ORDER
;

: Export ( pub-wid base-addr -- pub-wid base-addr )
    (ParseNameRefill)
    2DUP S" BeginModule" COMPARE 0=
    IF
        2DROP
        GET-CURRENT WORDLIST DUP
        (CreateModuleList)
        OVER SET-CURRENT
        EXIT
    THEN

    2DUP S" Import" COMPARE 0=
    IF
        2DROP
        (GetModule) OVER (AddModuleDepth)
        EXIT
    THEN

    2DUP S" :" COMPARE 0=
    IF
        2DROP
        GET-CURRENT >R
        OVER SET-CURRENT
        (ParseNameRefill) NEXTNAME
        ['] : EXECUTE
        R> SET-CURRENT
        EXIT
    THEN

    CR ." MODULE ERR: [EXPORT " TYPE ." ] INVALID SYNTAX" CR
    ABORT
;

: Import ( "name" -- u )
    GET-ORDER >R

    (GetModule)
    (GetModuleList)

    R> OVER >R +
    SET-ORDER
    R>
;

: EndImport ( u -- )
    >R GET-ORDER R@ -
    R> SWAP >R
    0 ?DO
        DROP
    LOOP
    R> SET-ORDER
;