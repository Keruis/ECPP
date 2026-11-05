REQUIRE byte_vector.fs

Import ByteVectorModule

BeginModule UnicodeNameVectorModule

Export Import ByteVectorModule

: (UnicodeNameVectorElementDefault) ( dest -- )
    ByteVector.New SWAP !
;

: (UnicodeNameVectorElementMove) ( src dest -- )
    >R
    DUP @ R@ !
    0 SWAP !
    R> DROP
;

: (UnicodeNameVectorElementDestroy) ( element -- )
    @ ?DUP IF
        ByteVector.Free
    THEN
;

: (UnicodeNameVectorElementMoveAssign) ( src dest -- )
    2DUP = IF
        2DROP EXIT
    THEN

    DUP (UnicodeNameVectorElementDestroy)
    (UnicodeNameVectorElementMove)
;

BeginElementType UnicodeNameVectorElement
    CELL ElementSize
    CELL ElementAlignment

    RawRelocatable
    NoThrowMove

    ' (UnicodeNameVectorElementDefault) UseDefaultConstruct
    ' (UnicodeNameVectorElementMove) UseMoveConstruct
    ' (UnicodeNameVectorElementDestroy) UseDestroy
    ' (UnicodeNameVectorElementMoveAssign) UseMoveAssign

    NoCopyConstruct
    NoCopyAssign
EndElementType

DefineVectorType UnicodeNameVector UnicodeNameVectorElement

Export : UnicodeNameVector.NameAt ( index names -- byte-vector )
    UnicodeNameVector.At @
;

Export : UnicodeNameVector.AppendSlice ( c-addr u names -- byte-vector )
    >R

    R@ UnicodeNameVector.EmplaceBack @

    R> DROP

    DUP >R
    ByteVector.AppendSlice
    R>
;

Export : UnicodeNameVector.AppendVector ( source names -- new-byte-vector )
    >R

    DUP ByteVector.Data
    SWAP ByteVector.Size

    R> UnicodeNameVector.AppendSlice
;

0 VALUE UNV_LEFT
0 VALUE UNV_RIGHT

: (UnicodeNameVectorEqual) ( left right -- flag )
    TO UNV_RIGHT
    TO UNV_LEFT

    UNV_LEFT ByteVector.Size
    UNV_RIGHT ByteVector.Size
    = IF
        UNV_LEFT ByteVector.Size 0= IF
            TRUE
        ELSE
            UNV_LEFT ByteVector.Data
            UNV_LEFT ByteVector.Size
            UNV_RIGHT ByteVector.Data
            UNV_RIGHT ByteVector.Size
            COMPARE 0=
        THEN
    ELSE
        FALSE
    THEN
;

0 VALUE UNV_SEARCH_NAME
0 VALUE UNV_SEARCH_VECTOR

Export : UnicodeNameVector.Contains? ( byte-vector names -- flag )
    TO UNV_SEARCH_VECTOR
    TO UNV_SEARCH_NAME

    UNV_SEARCH_VECTOR UnicodeNameVector.Size 0 ?DO

        I UNV_SEARCH_VECTOR UnicodeNameVector.NameAt

        UNV_SEARCH_NAME SWAP
        (UnicodeNameVectorEqual) IF
            TRUE
            UNLOOP EXIT
        THEN

    LOOP

    FALSE
;

0 VALUE UNV_APPEND_SOURCE
0 VALUE UNV_APPEND_DEST

Export : UnicodeNameVector.AppendUnique ( source names -- flag )
    TO UNV_APPEND_DEST
    TO UNV_APPEND_SOURCE

    UNV_APPEND_SOURCE UNV_APPEND_DEST
    UnicodeNameVector.Contains? IF
        FALSE
    ELSE
        UNV_APPEND_SOURCE UNV_APPEND_DEST
        UnicodeNameVector.AppendVector DROP

        TRUE
    THEN
;

0 VALUE UNV_UNIQUE_SOURCE
0 VALUE UNV_UNIQUE_DATA
0 VALUE UNV_UNIQUE_COUNT
0 VALUE UNV_UNIQUE_READ
0 VALUE UNV_UNIQUE_WRITE
0 VALUE UNV_UNIQUE_PREV
0 VALUE UNV_UNIQUE_CUR

Export : UnicodeNameVector.Unique ( names -- )
    TO UNV_UNIQUE_SOURCE

    UNV_UNIQUE_SOURCE UnicodeNameVector.Size TO UNV_UNIQUE_COUNT
    UNV_UNIQUE_COUNT 1 <= IF
        EXIT
    THEN

    UNV_UNIQUE_SOURCE UnicodeNameVector.Data TO UNV_UNIQUE_DATA
    0 TO UNV_UNIQUE_READ
    0 TO UNV_UNIQUE_WRITE
    0 TO UNV_UNIQUE_PREV

    BEGIN
        UNV_UNIQUE_READ UNV_UNIQUE_COUNT <
    WHILE
        UNV_UNIQUE_DATA UNV_UNIQUE_READ CELLS + @ TO UNV_UNIQUE_CUR

        UNV_UNIQUE_WRITE 0= IF
            UNV_UNIQUE_CUR TO UNV_UNIQUE_PREV
            1 TO UNV_UNIQUE_WRITE
        ELSE
            UNV_UNIQUE_PREV UNV_UNIQUE_CUR
            (UnicodeNameVectorEqual) IF
                UNV_UNIQUE_CUR ByteVector.Free
                0 UNV_UNIQUE_DATA UNV_UNIQUE_READ CELLS + !
            ELSE
                UNV_UNIQUE_WRITE UNV_UNIQUE_READ <> IF
                    UNV_UNIQUE_CUR
                    UNV_UNIQUE_DATA UNV_UNIQUE_WRITE CELLS + !
                    0 UNV_UNIQUE_DATA UNV_UNIQUE_READ CELLS + !
                THEN

                UNV_UNIQUE_CUR TO UNV_UNIQUE_PREV
                UNV_UNIQUE_WRITE 1+ TO UNV_UNIQUE_WRITE
            THEN
        THEN

        UNV_UNIQUE_READ 1+ TO UNV_UNIQUE_READ
    REPEAT
    UNV_UNIQUE_WRITE UNV_UNIQUE_SOURCE UnicodeNameVector.Resize
;

Export : ByteVector.ContainsSlice? ( haystack needle -- flag )
    >R

    DUP ByteVector.Data
    SWAP ByteVector.Size

    R@ DUP ByteVector.Data
    SWAP ByteVector.Size

    SEARCH NIP NIP

    R> DROP
;

0 VALUE UNV_SORT_LEFT
0 VALUE UNV_SORT_RIGHT

: (UnicodeNameSortLess) ( left right -- flag )
    TO UNV_SORT_RIGHT
    TO UNV_SORT_LEFT

    UNV_SORT_LEFT ByteVector.Size
    UNV_SORT_RIGHT ByteVector.Size

    2DUP <> IF
        >
    ELSE
        2DROP

        UNV_SORT_LEFT ByteVector.Data
        UNV_SORT_LEFT ByteVector.Size

        UNV_SORT_RIGHT ByteVector.Data
        UNV_SORT_RIGHT ByteVector.Size

        COMPARE 0<
    THEN
;

CREATE (UnicodeNameRadixCounts) 256 CELLS ALLOT

0 VALUE UNV_RADIX_VECTOR
0 VALUE UNV_RADIX_COUNT
0 VALUE UNV_RADIX_DATA
0 VALUE UNV_RADIX_MAX_LEN
0 VALUE UNV_RADIX_TEMP
0 VALUE UNV_RADIX_HIST
0 VALUE UNV_RADIX_START
0 VALUE UNV_RADIX_END
0 VALUE UNV_RADIX_LEN
0 VALUE UNV_RADIX_I
0 VALUE UNV_RADIX_SRC
0 VALUE UNV_RADIX_DST
0 VALUE UNV_RADIX_POS
0 VALUE UNV_RADIX_BUCKET_COUNT
0 VALUE UNV_RADIX_OFFSET
0 VALUE UNV_RADIX_NAME
0 VALUE UNV_RADIX_FREQUENCY

: (UnicodeNameRadixByte) ( name index -- byte )
    SWAP ByteVector.Data + C@
;


: (UnicodeNameRadixPass) ( -- )
    (UnicodeNameRadixCounts) 256 CELLS ERASE
    0 TO UNV_RADIX_I
    BEGIN
        UNV_RADIX_I UNV_RADIX_BUCKET_COUNT <
    WHILE
        UNV_RADIX_SRC UNV_RADIX_I CELLS + @
        TO UNV_RADIX_NAME

        UNV_RADIX_NAME UNV_RADIX_POS
        (UnicodeNameRadixByte)

        (UnicodeNameRadixCounts) SWAP CELLS +
        1 SWAP +!

        UNV_RADIX_I 1+ TO UNV_RADIX_I
    REPEAT
    0 TO UNV_RADIX_OFFSET
    256 0 ?DO
        (UnicodeNameRadixCounts) I CELLS + DUP @
        TO UNV_RADIX_FREQUENCY
        UNV_RADIX_OFFSET OVER ! DROP
        UNV_RADIX_OFFSET
        UNV_RADIX_FREQUENCY +
        TO UNV_RADIX_OFFSET
    LOOP
    0 TO UNV_RADIX_I

    BEGIN
        UNV_RADIX_I UNV_RADIX_BUCKET_COUNT <
    WHILE
        UNV_RADIX_SRC UNV_RADIX_I CELLS + @
        TO UNV_RADIX_NAME

        UNV_RADIX_NAME UNV_RADIX_POS
        (UnicodeNameRadixByte)
        (UnicodeNameRadixCounts) SWAP CELLS +
        DUP @ TO UNV_RADIX_OFFSET
        1 SWAP +!
        UNV_RADIX_DST UNV_RADIX_OFFSET CELLS +
        UNV_RADIX_NAME SWAP !

        UNV_RADIX_I 1+ TO UNV_RADIX_I
    REPEAT
    UNV_RADIX_SRC UNV_RADIX_DST
    TO UNV_RADIX_SRC
    TO UNV_RADIX_DST
    UNV_RADIX_POS 1-
    TO UNV_RADIX_POS
;

: (UnicodeNameRadixBucket) ( start count len -- )
    TO UNV_RADIX_LEN
    TO UNV_RADIX_BUCKET_COUNT
    TO UNV_RADIX_START

    UNV_RADIX_BUCKET_COUNT 1 > UNV_RADIX_LEN 0> AND IF
        UNV_RADIX_DATA UNV_RADIX_START CELLS + TO UNV_RADIX_SRC
        UNV_RADIX_TEMP UNV_RADIX_START CELLS + TO UNV_RADIX_DST
        UNV_RADIX_LEN 1- TO UNV_RADIX_POS
        BEGIN
            UNV_RADIX_POS 0>=
        WHILE
            (UnicodeNameRadixPass)
        REPEAT

        UNV_RADIX_SRC UNV_RADIX_DATA UNV_RADIX_START CELLS + <>
        IF
            UNV_RADIX_SRC
            UNV_RADIX_DATA UNV_RADIX_START CELLS +
            UNV_RADIX_BUCKET_COUNT CELLS MOVE
        THEN
    THEN
;

: (UnicodeNameRadixCheckedAdd) ( u1 u2 -- sum )
    2DUP OR 0< IF
        2DROP VectorOverflowError THROW
    THEN
    + DUP 0< IF
        VectorOverflowError THROW
    THEN
;

: (UnicodeNameRadixCheckedCells) ( u -- bytes )
    DUP 0< IF
        VectorOverflowError THROW
    THEN
    1 CELLS UM*
    DUP IF
        2DROP VectorOverflowError THROW
    THEN
    DROP
    DUP 0< IF
        VectorOverflowError THROW
    THEN
;

Export : UnicodeNameVector.SortByIndex ( names -- )
    TO UNV_RADIX_VECTOR
    UNV_RADIX_VECTOR UnicodeNameVector.Size TO UNV_RADIX_COUNT

    UNV_RADIX_COUNT 1 <= IF
        EXIT
    THEN

    UNV_RADIX_VECTOR UnicodeNameVector.Data TO UNV_RADIX_DATA
    0 TO UNV_RADIX_MAX_LEN
    UNV_RADIX_COUNT 0 ?DO
        UNV_RADIX_DATA I CELLS + @ ByteVector.Size
        DUP UNV_RADIX_MAX_LEN U>
        IF
            TO UNV_RADIX_MAX_LEN
        ELSE
            DROP
        THEN
    LOOP
    UNV_RADIX_MAX_LEN 1+ DUP 0< IF
        DROP VectorOverflowError THROW
    THEN
    UNV_RADIX_COUNT SWAP (UnicodeNameRadixCheckedAdd)
    (UnicodeNameRadixCheckedCells)
    ALLOCATE THROW TO UNV_RADIX_TEMP
    UNV_RADIX_TEMP UNV_RADIX_COUNT CELLS + TO UNV_RADIX_HIST
    UNV_RADIX_HIST UNV_RADIX_MAX_LEN 1+ CELLS ERASE
    UNV_RADIX_COUNT 0 ?DO
        UNV_RADIX_DATA I CELLS + @ ByteVector.Size
        UNV_RADIX_HIST SWAP CELLS +
        1 SWAP +!
    LOOP
    0 TO UNV_RADIX_START
    UNV_RADIX_MAX_LEN 1+ 0 ?DO
        UNV_RADIX_MAX_LEN I - TO UNV_RADIX_LEN
        UNV_RADIX_HIST UNV_RADIX_LEN CELLS + @ TO UNV_RADIX_OFFSET
        UNV_RADIX_START UNV_RADIX_HIST UNV_RADIX_LEN CELLS + !
        UNV_RADIX_START UNV_RADIX_OFFSET + TO UNV_RADIX_START
    LOOP
    UNV_RADIX_COUNT 0 ?DO
        UNV_RADIX_DATA I CELLS + @ TO UNV_RADIX_NAME
        UNV_RADIX_NAME ByteVector.Size TO UNV_RADIX_LEN
        UNV_RADIX_HIST UNV_RADIX_LEN CELLS + DUP @
        DUP 1+ ROT !
        TO UNV_RADIX_END
        UNV_RADIX_TEMP UNV_RADIX_END CELLS +
        UNV_RADIX_NAME SWAP !
    LOOP
    UNV_RADIX_TEMP UNV_RADIX_DATA UNV_RADIX_COUNT CELLS MOVE
    0 TO UNV_RADIX_START
    UNV_RADIX_MAX_LEN 1+ 0 ?DO
        UNV_RADIX_MAX_LEN I - TO UNV_RADIX_LEN
        UNV_RADIX_HIST UNV_RADIX_LEN CELLS + @ TO UNV_RADIX_END
        UNV_RADIX_END UNV_RADIX_START - TO UNV_RADIX_BUCKET_COUNT
        UNV_RADIX_START UNV_RADIX_BUCKET_COUNT UNV_RADIX_LEN
        (UnicodeNameRadixBucket)
        UNV_RADIX_END TO UNV_RADIX_START
    LOOP

    UNV_RADIX_TEMP FREE THROW
;

EndModule

EndImport