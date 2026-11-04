FPATH PATH+ ../../../../forth

REQUIRE shell.fs
REQUIRE byte_vector.fs

DECIMAL

Import Shell
Import ByteVectorModule
+

0 VALUE CUR_FILE_DATA

: LoadFile ( c-addr u -- )
    SLURP-FILE 2DUP
    0= IF
        S" Input File Is Empty" Fail
    THEN
    -ROT
    DUP ByteVector.NewWithCapacity TO CUR_FILE_DATA
    CUR_FILE_DATA ByteVector.AppendSlice
    FREE
;

: PrepareData
    S" mkdir -p tmp" ShOk
    S" curl -L -o tmp/UnicodeData.txt https://www.unicode.org/Public/18.0.0/ucd/UnicodeData.txt" ShOk
    S" curl -L -o tmp/NameAliases.txt https://www.unicode.org/Public/18.0.0/ucd/NameAliases.txt" ShOk
;

: Build
    S" tmp/UnicodeData.txt" LoadFile
    CUR_FILE_DATA ByteVector.Size .
;

: Main ( -- )
    PrepareData
    Build
;

EndImport

Main

BYE