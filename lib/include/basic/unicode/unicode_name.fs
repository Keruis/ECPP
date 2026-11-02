FPATH PATH+ ../../../../forth

INCLUDE shell.fs

DECIMAL

Import Shell

: PrepareData
    S" mkdir -p tmp" ShOk
    S" curl -L -o tmp/UnicodeData.txt https://www.unicode.org/Public/18.0.0/ucd/UnicodeData.txt" ShOk
    S" curl -L -o tmp/NameAliases.txt https://www.unicode.org/Public/18.0.0/ucd/NameAliases.txt" ShOk
;

: Build
    
;

: Main ( -- )
    PrepareData
    Build
;

EndImport

Main

BYE