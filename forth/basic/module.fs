REQUIRE cc.fs
REQUIRE fail.fs

32 CONSTANT MAX_MOD_DEPTH

: (RefillInput) ( -- )
    REFILL 0= 
    IF
        S" End Of Input" Fail
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

: (AddSearchWid) ( wid -- )
    >R
    GET-ORDER 1+
    R> SWAP SET-ORDER
;

: (AddModuleDepth) ( mod-addr base-addr -- )
    DUP (GetModuleCount) DUP @
    DUP MAX_MOD_DEPTH >= IF
        S" Module Err: Module Dependency Limit Exceeed" Fail
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
    S" Module [" 2SWAP S+ S" ] Not Found" S+ Fail 
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
    2 PICK (AddSearchWid)
    (AddSearchWid)
;

: EndModule ( old-wid pub-wid base-addr -- )
    2DROP SET-CURRENT
    GET-ORDER
    2 - >R
    2DROP
    R> SET-ORDER
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

    S" Module Err: [Export " 2SWAP S+ S" ] Invalid Syntax" S+ Fail 
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