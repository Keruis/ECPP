REQUIRE byte_vector.fs

Import ByteVectorModule

BeginModule UnicodeMapModule

Export Import ByteVectorModule

: (ByteVectorHandleDefault) ( dest -- )
    ByteVector.New SWAP !
;

: (ByteVectorHandleMove) ( src dest -- )
    >R
    DUP @ R@ !
    0 SWAP !
    R> DROP
;

: (ByteVectorHandleDestroy) ( element -- )
    @ ?DUP IF
        ByteVector.Free
    THEN
;

: (ByteVectorHandleMoveAssign) ( src dest -- )
    2DUP = IF
        2DROP EXIT
    THEN
    DUP (ByteVectorHandleDestroy)
    (ByteVectorHandleMove)
;

BeginElementType ByteVectorHandle
    CELL ElementSize
    CELL ElementAlignment
    RawRelocatable
    NoThrowMove
    ' (ByteVectorHandleDefault) UseDefaultConstruct
    ' (ByteVectorHandleMove) UseMoveConstruct
    ' (ByteVectorHandleDestroy) UseDestroy
    ' (ByteVectorHandleMoveAssign) UseMoveAssign
    NoCopyConstruct
    NoCopyAssign
EndElementType

DefineVectorType UnicodeAliasVector ByteVectorHandle

: (UnicodeMapEntryDefault) ( dest -- )
    DUP 0 SWAP !
    UnicodeAliasVector.New SWAP CELL+ !
;

: (UnicodeMapEntryMove) ( src dest -- )
    >R
    DUP @ R@ !
    DUP CELL+ @ R@ CELL+ !
    0 OVER !
    0 SWAP CELL+ !
    R> DROP
;

: (UnicodeMapEntryDestroy) ( entry -- )
    CELL+ @ ?DUP IF
        UnicodeAliasVector.Free
    THEN
;

: (UnicodeMapEntryMoveAssign) ( src dest -- )
    2DUP = IF
        2DROP EXIT
    THEN
    DUP (UnicodeMapEntryDestroy)
    (UnicodeMapEntryMove)
;

BeginElementType UnicodeMapEntry
    2 CELLS ElementSize
    CELL ElementAlignment
    RawRelocatable
    NoThrowMove
    ' (UnicodeMapEntryDefault) UseDefaultConstruct
    ' (UnicodeMapEntryMove) UseMoveConstruct
    ' (UnicodeMapEntryDestroy) UseDestroy
    ' (UnicodeMapEntryMoveAssign) UseMoveAssign
    NoCopyConstruct
    NoCopyAssign
EndElementType

DefineVectorType UnicodeMap UnicodeMapEntry

Export : UnicodeMap.EntryCodePoint ( entry -- codepoint )
    @
;

Export : UnicodeMap.EntryAliases ( entry -- alias-vector )
    CELL+ @
;

Export : UnicodeMap.EntryAliasCount ( entry -- u )
    UnicodeMap.EntryAliases UnicodeAliasVector.Size
;

Export : UnicodeMap.EntryAliasAt ( entry index -- byte-vector )
    SWAP UnicodeMap.EntryAliases
    UnicodeAliasVector.At @
;

: (ByteVectorHandleEqual) ( alias-handle name-vector -- flag )
    >R
    @ DUP ByteVector.Data SWAP ByteVector.Size
    R@ DUP ByteVector.Data SWAP ByteVector.Size
    COMPARE 0=
    R> DROP
;

Export : UnicodeMap.EntryHasAlias? ( entry name-vector -- flag )
    OVER UnicodeMap.EntryAliases
    DUP UnicodeAliasVector.Size 0 ?DO
        DUP I SWAP UnicodeAliasVector.At
        2 PICK (ByteVectorHandleEqual) IF
            2DROP DROP
            TRUE
            UNLOOP EXIT
        THEN
    LOOP
    2DROP DROP
    FALSE
;

0 VALUE UCM_FIND_CP
0 VALUE UCM_FIND_MAP

Export : UnicodeMap.Find ( codepoint map -- entry|0 )
    TO UCM_FIND_MAP
    TO UCM_FIND_CP

    UCM_FIND_MAP UnicodeMap.Size 0 ?DO
        I UCM_FIND_MAP UnicodeMap.At

        DUP UnicodeMap.EntryCodePoint
        UCM_FIND_CP = IF
            UNLOOP EXIT
        THEN

        DROP
    LOOP

    0
;

0 VALUE UCM_SORTED_CP
0 VALUE UCM_SORTED_MAP
0 VALUE UCM_SORTED_LOW
0 VALUE UCM_SORTED_HIGH
0 VALUE UCM_SORTED_MID
0 VALUE UCM_SORTED_ENTRY

Export : UnicodeMap.FindSorted ( codepoint map -- entry|0 )
    TO UCM_SORTED_MAP
    TO UCM_SORTED_CP

    0 TO UCM_SORTED_LOW
    UCM_SORTED_MAP UnicodeMap.Size TO UCM_SORTED_HIGH

    BEGIN
        UCM_SORTED_LOW UCM_SORTED_HIGH U<
    WHILE
        UCM_SORTED_HIGH UCM_SORTED_LOW - 2/
        UCM_SORTED_LOW + TO UCM_SORTED_MID

        UCM_SORTED_MID UCM_SORTED_MAP UnicodeMap.At
        TO UCM_SORTED_ENTRY

        UCM_SORTED_ENTRY UnicodeMap.EntryCodePoint
        UCM_SORTED_CP U< IF
            UCM_SORTED_MID 1+ TO UCM_SORTED_LOW
        ELSE
            UCM_SORTED_MID TO UCM_SORTED_HIGH
        THEN
    REPEAT

    UCM_SORTED_LOW UCM_SORTED_MAP UnicodeMap.Size U< IF
        UCM_SORTED_LOW UCM_SORTED_MAP UnicodeMap.At
        DUP UnicodeMap.EntryCodePoint UCM_SORTED_CP = IF
            EXIT
        THEN
        DROP
    THEN

    0
;

: (UnicodeMapAppendEntry) ( codepoint map -- entry )
    >R
    R@ UnicodeMap.EmplaceBack
    DUP >R
    !
    R>
    R> DROP
;

Export : UnicodeMap.AppendEntry ( codepoint map -- entry )
    (UnicodeMapAppendEntry)
;

Export : UnicodeMap.GetOrAdd ( codepoint map -- entry )
    2DUP UnicodeMap.Find ?DUP IF
        -ROT 2DROP
        EXIT
    THEN
    (UnicodeMapAppendEntry)
;

Export : UnicodeMap.AppendAlias ( c-addr u entry -- )
    >R
    R@ UnicodeMap.EntryAliases
    UnicodeAliasVector.EmplaceBack
    @
    R> DROP
    ByteVector.AppendSlice
;

Export : UnicodeMap.AddAlias ( codepoint map c-addr u -- )
    >R >R
    UnicodeMap.GetOrAdd
    R> R> ROT
    UnicodeMap.AppendAlias
;

EndModule

EndImport