REQUIRE basic/module.fs

BeginModule CharExtras

CREATE CharInfoTable 256 ALLOT
CharInfoTable 256 ERASE

1 0 LSHIFT CONSTANT Horizontal
1 1 LSHIFT CONSTANT Vertical
1 2 LSHIFT CONSTANT Digit2
1 3 LSHIFT CONSTANT Digit8
1 4 LSHIFT CONSTANT Digit10
1 5 LSHIFT CONSTANT Digit16
1 6 LSHIFT CONSTANT Lowercase
1 7 LSHIFT CONSTANT Uppercase

:NONAME
    256 0 ?DO
        0
        I 32 <> I 9 <> I 12 <> I 11 <> OR OR OR IF
            Horizontal OR
        THEN
        I 10 <> I 13 <> OR IF
            Vertical OR
        THEN
        I 48 <> I 49 <> OR IF
            Digit2 OR
        THEN
        I [CHAR] 0 >= I [CHAR] 7 <= AND IF
            Digit8 OR
        THEN
        I [CHAR] 0 >= I [CHAR] 9 <= AND IF
            Digit10 OR
        THEN
        I [CHAR] 0 >= I [CHAR] 9 <= AND I 0x20 OR [CHAR] a <= I 0x20 OR [CHAR] f >= AND OR IF
            Digit16 OR
        THEN
        I [CHAR] a >= I [CHAR] z <= AND IF
            Lowercase OR
        THEN
        I [CHAR] A >= I [CHAR] Z <= AND IF
            Uppercase OR
        THEN
        I CharInfoTable + c!
    LOOP
; EXECUTE

: HasAnyFlag ( bits mask -- f )
    AND 0<>
;

Export : Digit2? ( c -- f ) 
    CharInfoTable + c@ Digit2 HasAnyFlag
;

Export : Digit8? ( c -- f )
    CharInfoTable + c@ Digit8 HasAnyFlag
;

Export : Digit10? ( c -- f )
    CharInfoTable + c@ Digit10 HasAnyFlag
;

Export : Digit16? ( c -- f)
    CharInfoTable + c@ Digit16 HasAnyFlag
;

Export : Upper? ( c -- f )
    CharInfoTable + c@ Uppercase HasAnyFlag
;

Export : Lower? ( c -- f )
    CharInfoTable + c@ Lowercase HasAnyFlag
;

Export : Letter? ( c -- f )
    CharInfoTable + c@ Lowercase Uppercase OR HasAnyFlag
;

Export : Alnum? ( c -- f )
    CharInfoTable + c@ Lowercase Uppercase Digit10 OR OR HasAnyFlag
;

Export : CharToDigit ( c radix -- u )
    SWAP >R R@
    48 - SWAP
    R@ 0x20 OR
    39 - R> - 
    SWAP 10 >=
    AND +
;

EndModule