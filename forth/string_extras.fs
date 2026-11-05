REQUIRE basic/module.fs
REQUIRE char_extras.fs

Import CharExtras

BeginModule StringExtras

Export : HexStringToNumeric ( c-addr u -- u f )
    0 -ROT 0 ?DO
        DUP I + C@ DUP Digit16? 0= IF
            2DROP FALSE
            UNLOOP EXIT
        THEN
        HexCharToDigit
        >R SWAP 16 * R> + SWAP
    LOOP
    DROP
    TRUE
;

EndModule

EndImport