FPATH PATH+ ../../../../../forth

REQUIRE shell.fs
REQUIRE char_extras.fs
REQUIRE string_extras.fs
REQUIRE byte_vector.fs
REQUIRE unicode_map.fs
REQUIRE unicode_name_vector.fs
REQUIRE unicode_node_vector.fs

DECIMAL

Import Shell
Import CharExtras
Import StringExtras
Import ByteVectorModule
Import UnicodeMapModule
Import UnicodeNodeVectorModule
Import UnicodeNameVectorModule
+ + + + + +

0 VALUE CUR_FILE_DATA

: LoadUnicodeFile ( c-addr u -- )
    SLURP-FILE 2DUP
    0= IF
        S" Input File Is Empty" Fail
    THEN
    -ROT
    CUR_FILE_DATA ?DUP IF
        ByteVector.Clear
    ELSE
        DUP ByteVector.NewWithCapacity TO CUR_FILE_DATA
    THEN
    CUR_FILE_DATA ByteVector.AppendSlice
    FREE
;

VARIABLE CH_SEMI
59 CH_SEMI !
45 CONSTANT CH_DASH
ByteVector.New VALUE LINE
0 VALUE SEMI1
0 VALUE SEMI2
0 VALUE LINE_CP
0 VALUE LINE_ENTRY
FALSE VALUE PARSE_PRIMARY_UNICODE_DATA
ByteVector.New VALUE LNAME
UnicodeMap.New VALUE UNICODE_MAP

: ParseLine ( c-addr u -- )
    LINE ByteVector.Clear
    LINE ByteVector.AppendSlice
    LINE ByteVector.Size 0= IF
        EXIT
    THEN
    LINE ByteVector.Front C@ Digit16? 0= IF
        EXIT
    THEN
    LINE ByteVector.Begin LINE ByteVector.End CH_SEMI ByteVectorAlgorithm.Find
    DUP LINE ByteVector.End = IF
        DROP EXIT
    THEN
    TO SEMI1
    LINE ByteVector.Data LINE ByteVector.Begin SEMI1 ByteVectorAlgorithmIterator.Distance
    HexStringToNumeric 0= IF
        DROP EXIT
    THEN
    TO LINE_CP
    SEMI1 ByteVectorAlgorithmIterator.Next LINE ByteVector.End CH_SEMI ByteVectorAlgorithm.Find
    DUP LINE ByteVector.End = IF
        DROP EXIT
    THEN
    TO SEMI2

    PARSE_PRIMARY_UNICODE_DATA IF
        LINE_CP UNICODE_MAP UnicodeMap.AppendEntry
    ELSE
        LINE_CP UNICODE_MAP UnicodeMap.FindSorted
        ?DUP 0= IF
            S" NameAliases Code Point is absent from UnicodeData.txt" Fail
        THEN
    THEN
    TO LINE_ENTRY

    LNAME ByteVector.Clear
    SEMI1 ByteVectorAlgorithmIterator.Next ByteVectorAlgorithmIterator.Read SEMI1 ByteVectorAlgorithmIterator.Next SEMI2 ByteVectorAlgorithmIterator.Distance
    LNAME ByteVector.AppendSlice
    LNAME ByteVector.Empty IF
        EXIT
    THEN

    LNAME ByteVector.Begin LNAME ByteVector.End ByteVectorAlgorithm.AllUnicodeChars 0= IF
        EXIT
    THEN

    LINE_ENTRY DUP LNAME UnicodeMap.EntryHasAlias? IF
        DROP
    ELSE
        LNAME ByteVector.Data
        LNAME ByteVector.Size
        ROT
        UnicodeMap.AppendAlias
    THEN
;

0 VALUE DATA_START
0 VALUE DATA_I
0 VALUE DATA_ADDRESS
0 VALUE DATA_SIZE

: ParseData ( -- )
    CUR_FILE_DATA ByteVector.Data TO DATA_ADDRESS
    CUR_FILE_DATA ByteVector.Size TO DATA_SIZE

    0 TO DATA_START
    0 TO DATA_I
    BEGIN
        DATA_I DATA_SIZE U<
    WHILE
        DATA_ADDRESS DATA_I + C@ 10 = IF
            DATA_ADDRESS DATA_START +
            DATA_I DATA_START -
            ParseLine
            DATA_I 1+ TO DATA_START
        THEN
        DATA_I 1+ TO DATA_I
    REPEAT

    DATA_I DATA_START U> IF
        DATA_ADDRESS DATA_START +
        DATA_I DATA_START -
        ParseLine
    THEN
;

FALSE VALUE BEFORE_MEDIAL
UnicodeNodeVector.New VALUE NODES
UnicodeNodePointerList.New VALUE LEAF_NODES
UnicodeNameVector.New VALUE NAMES

0 VALUE UNICODE_ROOT

0 VALUE TREE_ENTRY
0 VALUE TREE_STRING
0 VALUE TREE_CODE_POINT
0 VALUE TREE_STRING_SIZE
0 VALUE TREE_POS
0 VALUE TREE_TOKEN_SIZE

0 VALUE TREE_NODE
0 VALUE TREE_CHILD
0 VALUE TREE_FOUND

0 VALUE TREE_FIND_PARENT
0 VALUE TREE_FIND_NAME

0 VALUE TREE_COMPARE_LEFT
0 VALUE TREE_COMPARE_RIGHT

ByteVector.New VALUE TREE_TOKEN

: TreeByteVectorEqual? ( bv1 bv2 -- flag )
    TO TREE_COMPARE_RIGHT
    TO TREE_COMPARE_LEFT

    TREE_COMPARE_LEFT ByteVector.Size
    TREE_COMPARE_RIGHT ByteVector.Size
    <> IF
        FALSE
        EXIT
    THEN

    TREE_COMPARE_LEFT ByteVector.Data
    TREE_COMPARE_LEFT ByteVector.Size

    TREE_COMPARE_RIGHT ByteVector.Data
    TREE_COMPARE_RIGHT ByteVector.Size

    COMPARE 0=
;

: TreeIsMedial? ( -- flag )
    TREE_POS 2 + TREE_STRING_SIZE < 0= IF
        FALSE
        EXIT
    THEN
    TREE_STRING ByteVector.Data
    TREE_POS +
    C@
    Alnum? 0= IF
        FALSE
        EXIT
    THEN
    TREE_STRING ByteVector.Data
    TREE_POS 1+ +
    C@
    CH_DASH <> IF
        FALSE
        EXIT
    THEN
    TREE_STRING ByteVector.Data
    TREE_POS 2 + +
    C@
    Alnum?
;

: TreeFindChild ( parent name-vector -- child|0 )
    TO TREE_FIND_NAME
    TO TREE_FIND_PARENT
    0 TO TREE_FOUND
    TREE_FIND_PARENT UnicodeNode.ChildCount 0 ?DO
        TREE_FIND_PARENT I UnicodeNode.ChildAt
        DUP UnicodeNode.Name
        TREE_FIND_NAME
        TreeByteVectorEqual? IF
            TO TREE_FOUND
            LEAVE
        ELSE
            DROP
        THEN
    LOOP

    TREE_FOUND
;

: BuildTree ( -- )
    0 NODES UnicodeNodeVector.AddNode TO UNICODE_ROOT
    UNICODE_MAP UnicodeMap.Size 0 ?DO
        I UNICODE_MAP UnicodeMap.At
        TO TREE_ENTRY
        TREE_ENTRY UnicodeMap.EntryCodePoint
        TO TREE_CODE_POINT
        TREE_ENTRY UnicodeMap.EntryAliasCount 0 ?DO
            TREE_ENTRY I UnicodeMap.EntryAliasAt
            TO TREE_STRING
            TREE_STRING ByteVector.Size
            TO TREE_STRING_SIZE
            UNICODE_ROOT TO TREE_NODE
            0 TO TREE_POS
            FALSE TO BEFORE_MEDIAL
            BEGIN
                TREE_POS TREE_STRING_SIZE <
            WHILE
                TreeIsMedial?
                TO BEFORE_MEDIAL
                BEFORE_MEDIAL IF
                    3 TO TREE_TOKEN_SIZE
                ELSE
                    1 TO TREE_TOKEN_SIZE
                THEN
                TREE_TOKEN ByteVector.Clear
                TREE_STRING ByteVector.Data
                TREE_POS +
                TREE_TOKEN_SIZE
                TREE_TOKEN ByteVector.AppendSlice
                TREE_NODE TREE_TOKEN
                TreeFindChild
                TO TREE_CHILD
                TREE_CHILD 0= IF
                    TREE_NODE NODES
                    UnicodeNodeVector.AddNode
                    TO TREE_CHILD
                    TREE_TOKEN ByteVector.Data
                    TREE_TOKEN ByteVector.Size
                    TREE_CHILD UnicodeNode.SetName
                THEN
                TREE_CHILD TO TREE_NODE
                TREE_POS TREE_TOKEN_SIZE +
                TO TREE_POS
            REPEAT
            TREE_CODE_POINT TREE_NODE
            UnicodeNode.SetCodePoint
            TREE_NODE LEAF_NODES
            UnicodeNodePointerList.Push
        LOOP
    LOOP
;

0 VALUE COMPRESS_NODE
0 VALUE COMPRESS_PARENT
0 VALUE COMPRESS_NODE_NAME
0 VALUE COMPRESS_NODE_CP

: CompressTree ( -- )
    BEGIN
        LEAF_NODES UnicodeNodePointerList.Size 0>
    WHILE
        LEAF_NODES UnicodeNodePointerList.Pop
        TO COMPRESS_NODE
        COMPRESS_NODE UnicodeNode.Parent
        TO COMPRESS_PARENT
        COMPRESS_PARENT IF
            COMPRESS_PARENT LEAF_NODES
            UnicodeNodePointerList.Push
            COMPRESS_PARENT UnicodeNode.ChildCount 1 =
            COMPRESS_PARENT UnicodeNode.HasCodePoint? 0= AND
            COMPRESS_NODE UnicodeNode.Name
            ByteVector.Size
            COMPRESS_PARENT UnicodeNode.Name
            ByteVector.Size
            + 32 <= AND
        ELSE
            FALSE
        THEN
        IF
            COMPRESS_NODE UnicodeNode.Name
            DUP ByteVector.Data
            SWAP ByteVector.Size

            COMPRESS_PARENT UnicodeNode.AppendName
            COMPRESS_NODE UnicodeNode.GetCodePoint IF
                COMPRESS_PARENT UnicodeNode.SetCodePoint
            ELSE
                COMPRESS_PARENT UnicodeNode.ClearCodePoint
            THEN
            COMPRESS_NODE COMPRESS_PARENT
            UnicodeNode.MoveChildren
        ELSE
            COMPRESS_NODE UnicodeNode.Name
            NAMES UnicodeNameVector.AppendVector DROP
        THEN
    REPEAT
;

ByteVector.New VALUE UNICODE_DICT

0 VALUE DICT_NAME
0 VALUE DICT_DATA
0 VALUE DICT_SIZE
0 VALUE DICT_NAME_DATA
0 VALUE DICT_NAME_SIZE

: BuildDictionary ( -- )
    UNICODE_DICT ByteVector.Clear
    0xFFFF UNICODE_DICT ByteVector.Reserve
    S"  _-ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
    UNICODE_DICT ByteVector.AppendSlice
    UNICODE_DICT ByteVector.Data TO DICT_DATA
    UNICODE_DICT ByteVector.Size TO DICT_SIZE
    NAMES UnicodeNameVector.Size 0 ?DO
        I NAMES UnicodeNameVector.NameAt
        TO DICT_NAME
        DICT_NAME ByteVector.Size
        TO DICT_NAME_SIZE
        DICT_NAME_SIZE 1 > IF
            DICT_NAME ByteVector.Data
            TO DICT_NAME_DATA
            DICT_DATA DICT_SIZE
            DICT_NAME_DATA DICT_NAME_SIZE
            SEARCH IF
                2DROP
            ELSE
                2DROP
                DICT_SIZE DICT_NAME_SIZE +
                0xFFFF U> IF
                    S" Dictionary exceeds 16-bit offsets" Fail
                THEN
                DICT_NAME_DATA DICT_NAME_SIZE
                UNICODE_DICT ByteVector.AppendSlice
                DICT_SIZE DICT_NAME_SIZE +
                TO DICT_SIZE
            THEN
        THEN
    LOOP
;

ByteVector.New VALUE UNICODE_NAME_NODES
UnicodeNodePointerList.New VALUE ENCODE_QUEUE
UnicodeNodePointerList.New VALUE ENCODE_PATCH_OFFSETS
UnicodeNodePointerList.New VALUE ENCODE_PATCH_CODE_POINT_FLAGS

0 VALUE ENCODE_INDEX
0 VALUE ENCODE_NODE
0 VALUE ENCODE_NODE_NAME
0 VALUE ENCODE_NODE_NAME_SIZE
0 VALUE ENCODE_NODE_NAME_INDEX
0 VALUE ENCODE_NODE_PARENT
0 VALUE ENCODE_NODE_CODE_POINT
0 VALUE ENCODE_NODE_HEADER
0 VALUE ENCODE_CHILDREN_OFFSET
0 VALUE ENCODE_CHILD
0 VALUE ENCODE_PATCH_OFFSET
0 VALUE ENCODE_PATCH_HAS_CODE_POINT
0 VALUE ENCODE_CHILD_OFFSET
0 VALUE ENCODE_PATCH_DESTINATION
0 VALUE ENCODE_DICT_BYTE
0 VALUE ENCODE_DICT_DATA
0 VALUE ENCODE_DICT_SIZE
0 VALUE ENCODE_OUTPUT_DATA
0 VALUE ENCODE_QUEUE_DATA
0 VALUE ENCODE_PATCH_OFFSETS_DATA
0 VALUE ENCODE_PATCH_CODE_POINT_FLAGS_DATA
0 VALUE ENCODE_QUEUE_COUNT
0 VALUE ENCODE_CHILD_COUNT
0 VALUE ENCODE_PATCH_FLAGS

CREATE (EncodeDictByteIndices) 256 CELLS ALLOT

FALSE VALUE ENCODE_NODE_HAS_CODE_POINT
FALSE VALUE ENCODE_NODE_HAS_CHILDREN
FALSE VALUE ENCODE_NODE_HAS_SIBLING

: AppendNodeByte ( byte -- )
    UNICODE_NAME_NODES ByteVector.EmplaceBack C!
;

: BuildDictionaryByteIndices ( -- )
    256 0 ?DO
        -1 (EncodeDictByteIndices) I CELLS + !
    LOOP

    UNICODE_DICT ByteVector.Data TO ENCODE_DICT_DATA
    UNICODE_DICT ByteVector.Size TO ENCODE_DICT_SIZE

    ENCODE_DICT_SIZE 0 ?DO
        ENCODE_DICT_DATA I + C@ TO ENCODE_DICT_BYTE
        (EncodeDictByteIndices) ENCODE_DICT_BYTE CELLS + DUP @
        -1 = IF
            I SWAP !
        ELSE
            DROP
        THEN
    LOOP
;

: DictionaryByteIndex ( byte -- index )
    TO ENCODE_DICT_BYTE

    (EncodeDictByteIndices) ENCODE_DICT_BYTE CELLS + @
    DUP -1 = IF
        DROP
        S" Single-byte node name is absent from the dictionary" Fail
    THEN
;

: DictionaryNameIndex ( name -- index )
    TO ENCODE_NODE_NAME
    ENCODE_NODE_NAME ByteVector.Size TO ENCODE_NODE_NAME_SIZE

    ENCODE_DICT_SIZE ENCODE_NODE_NAME_SIZE U< IF
        S" Long node name is absent from the dictionary" Fail
    THEN
    ENCODE_DICT_DATA ENCODE_DICT_SIZE
    ENCODE_NODE_NAME ByteVector.Data
    ENCODE_NODE_NAME_SIZE
    SEARCH 0= IF
        2DROP
        S" Long node name is absent from the dictionary" Fail
    THEN

    DROP
    ENCODE_DICT_DATA -
;

: NodeHasSibling? ( node -- flag )
    UnicodeNode.Parent TO ENCODE_NODE_PARENT

    ENCODE_NODE_PARENT
    ENCODE_NODE_PARENT UnicodeNode.ChildCount 1-
    UnicodeNode.ChildAt
    ENCODE_NODE <>
;

: BackfillChildrenOffset ( -- )
    ENCODE_PATCH_OFFSETS_DATA ENCODE_INDEX CELLS + @
    TO ENCODE_PATCH_OFFSET
    ENCODE_PATCH_CODE_POINT_FLAGS_DATA ENCODE_INDEX CELLS + @
    TO ENCODE_PATCH_FLAGS
    ENCODE_PATCH_FLAGS 1 AND
    TO ENCODE_PATCH_HAS_CODE_POINT

    ENCODE_PATCH_OFFSET 0< IF
        EXIT
    THEN

    UNICODE_NAME_NODES ByteVector.Size TO ENCODE_CHILD_OFFSET
    ENCODE_CHILD_OFFSET 0xFFFFFF > IF
        S" Children offset exceeds 24 bits" Fail
    THEN

    ENCODE_OUTPUT_DATA ENCODE_PATCH_OFFSET +
    TO ENCODE_PATCH_DESTINATION

    ENCODE_PATCH_HAS_CODE_POINT IF
        ENCODE_CHILD_OFFSET 16 RSHIFT
        ENCODE_PATCH_DESTINATION C!
    ELSE
        ENCODE_CHILD_OFFSET 16 RSHIFT
        ENCODE_PATCH_DESTINATION C@ OR
        ENCODE_PATCH_DESTINATION C!
    THEN

    ENCODE_CHILD_OFFSET 8 RSHIFT 0xFF AND
    ENCODE_PATCH_DESTINATION 1+ C!
    ENCODE_CHILD_OFFSET 0xFF AND
    ENCODE_PATCH_DESTINATION 2 + C!
;

: EncodeNodeName ( -- )
    ENCODE_NODE UnicodeNode.Name TO ENCODE_NODE_NAME
    ENCODE_NODE_NAME ByteVector.Size TO ENCODE_NODE_NAME_SIZE

    0 TO ENCODE_NODE_HEADER
    ENCODE_NODE_HAS_CODE_POINT IF
        ENCODE_NODE_HEADER 0x80 OR TO ENCODE_NODE_HEADER
    THEN

    ENCODE_NODE_NAME_SIZE 1 = IF
        ENCODE_NODE_NAME ByteVector.Data C@
        DictionaryByteIndex
        TO ENCODE_NODE_NAME_INDEX
        ENCODE_NODE_NAME_INDEX 0x40 >= IF
            S" Single-byte dictionary index exceeds 6 bits" Fail
        THEN

        ENCODE_NODE_HEADER ENCODE_NODE_NAME_INDEX OR
        AppendNodeByte
    ELSE
        ENCODE_NODE_NAME_SIZE 0= IF
            S" Trie node name is empty" Fail
        THEN
        ENCODE_NODE_NAME_SIZE 0x3F > IF
            S" Long node name length exceeds 63 bytes" Fail
        THEN

        ENCODE_NODE_HEADER 0x40 OR
        ENCODE_NODE_NAME_SIZE OR
        AppendNodeByte

        ENCODE_NODE_NAME DictionaryNameIndex
        TO ENCODE_NODE_NAME_INDEX
        ENCODE_NODE_NAME_INDEX 0xFFFF > IF
            S" Dictionary offset exceeds 16 bits" Fail
        THEN

        ENCODE_NODE_NAME_INDEX 8 RSHIFT 0xFF AND AppendNodeByte
        ENCODE_NODE_NAME_INDEX 0xFF AND AppendNodeByte
    THEN
;

: EnqueueNodeChildren ( -- )
    ENCODE_NODE UnicodeNode.ChildCount DUP TO ENCODE_CHILD_COUNT 0 ?DO
        ENCODE_NODE I UnicodeNode.ChildAt TO ENCODE_CHILD
        ENCODE_CHILD ENCODE_QUEUE UnicodeNodePointerList.Push
        ENCODE_QUEUE_COUNT 1+ TO ENCODE_QUEUE_COUNT

        I 0= IF
            ENCODE_CHILDREN_OFFSET
            ENCODE_PATCH_OFFSETS
            UnicodeNodePointerList.Push
            ENCODE_NODE_HAS_CODE_POINT IF 1 ELSE 0 THEN
            I 1+ ENCODE_CHILD_COUNT U< IF 2 OR THEN
            ENCODE_PATCH_CODE_POINT_FLAGS
            UnicodeNodePointerList.Push
        ELSE
            -1 ENCODE_PATCH_OFFSETS UnicodeNodePointerList.Push
            I 1+ ENCODE_CHILD_COUNT U< IF 2 ELSE 0 THEN
            ENCODE_PATCH_CODE_POINT_FLAGS UnicodeNodePointerList.Push
        THEN
    LOOP
;

: EncodeNode ( -- )
    BackfillChildrenOffset

    ENCODE_NODE UnicodeNode.HasCodePoint? TO ENCODE_NODE_HAS_CODE_POINT
    ENCODE_NODE UnicodeNode.ChildCount 0<> TO ENCODE_NODE_HAS_CHILDREN
    ENCODE_PATCH_CODE_POINT_FLAGS_DATA ENCODE_INDEX CELLS + @
    2 AND 0<> TO ENCODE_NODE_HAS_SIBLING

    EncodeNodeName

    ENCODE_NODE_HAS_CODE_POINT IF
        ENCODE_NODE UnicodeNode.CodePoint TO ENCODE_NODE_CODE_POINT
        ENCODE_NODE_CODE_POINT 0x3FFFFF > IF
            S" Code point exceeds 22 bits" Fail
        THEN

        ENCODE_NODE_CODE_POINT 16 RSHIFT 0x3F AND
        ENCODE_NODE_HAS_CHILDREN IF 0x40 OR THEN
        ENCODE_NODE_HAS_SIBLING IF 0x80 OR THEN
        AppendNodeByte
        ENCODE_NODE_CODE_POINT 8 RSHIFT 0xFF AND AppendNodeByte
        ENCODE_NODE_CODE_POINT 0xFF AND AppendNodeByte

        ENCODE_NODE_HAS_CHILDREN IF
            UNICODE_NAME_NODES ByteVector.Size TO ENCODE_CHILDREN_OFFSET
            0 AppendNodeByte
            0 AppendNodeByte
            0 AppendNodeByte
        THEN
    ELSE
        ENCODE_NODE_HAS_CHILDREN IF
            UNICODE_NAME_NODES ByteVector.Size TO ENCODE_CHILDREN_OFFSET
            0x40
            ENCODE_NODE_HAS_SIBLING IF 0x80 OR THEN
            AppendNodeByte
            0 AppendNodeByte
            0 AppendNodeByte
        ELSE
            ENCODE_NODE_HAS_SIBLING IF 0x80 ELSE 0 THEN
            AppendNodeByte
        THEN
    THEN

    ENCODE_NODE_HAS_CHILDREN IF
        EnqueueNodeChildren
    THEN
;

: BuildUnicodeNameNodes ( -- )
    UNICODE_DICT ByteVector.Size 0xFFFF > IF
        S" Dictionary exceeds 16-bit offsets" Fail
    THEN
    NODES UnicodeNodeVector.Size 9 *
    UNICODE_NAME_NODES ByteVector.Reserve
    UNICODE_NAME_NODES ByteVector.Clear
    UNICODE_NAME_NODES ByteVector.Data TO ENCODE_OUTPUT_DATA
    ENCODE_QUEUE UnicodeNodePointerList.Clear
    ENCODE_PATCH_OFFSETS UnicodeNodePointerList.Clear
    ENCODE_PATCH_CODE_POINT_FLAGS UnicodeNodePointerList.Clear

    NODES UnicodeNodeVector.Size
    DUP ENCODE_QUEUE UnicodeNodePointerList.Reserve
    DUP ENCODE_PATCH_OFFSETS UnicodeNodePointerList.Reserve
    DUP ENCODE_PATCH_CODE_POINT_FLAGS UnicodeNodePointerList.Reserve
    DROP

    ENCODE_QUEUE UnicodeNodePointerList.Data TO ENCODE_QUEUE_DATA
    ENCODE_PATCH_OFFSETS UnicodeNodePointerList.Data
    TO ENCODE_PATCH_OFFSETS_DATA
    ENCODE_PATCH_CODE_POINT_FLAGS UnicodeNodePointerList.Data
    TO ENCODE_PATCH_CODE_POINT_FLAGS_DATA
    BuildDictionaryByteIndices

    0 TO ENCODE_QUEUE_COUNT
    UNICODE_ROOT UnicodeNode.ChildCount DUP TO ENCODE_CHILD_COUNT 0 ?DO
        UNICODE_ROOT I UnicodeNode.ChildAt
        ENCODE_QUEUE UnicodeNodePointerList.Push
        ENCODE_QUEUE_COUNT 1+ TO ENCODE_QUEUE_COUNT
        -1 ENCODE_PATCH_OFFSETS UnicodeNodePointerList.Push

        UNICODE_ROOT UnicodeNode.HasCodePoint? IF 1 ELSE 0 THEN
        I 1+ ENCODE_CHILD_COUNT U< IF 2 OR THEN
        ENCODE_PATCH_CODE_POINT_FLAGS UnicodeNodePointerList.Push
    LOOP

    0 TO ENCODE_INDEX
    BEGIN
        ENCODE_INDEX ENCODE_QUEUE_COUNT <
    WHILE
        ENCODE_QUEUE_DATA ENCODE_INDEX CELLS + @
        TO ENCODE_NODE
        EncodeNode
        ENCODE_INDEX 1+ TO ENCODE_INDEX
    REPEAT
;

ByteVector.New VALUE GENERATED_HEADER

0 VALUE GENERATED_HAYSTACK
0 VALUE GENERATED_NEEDLE
0 VALUE GENERATED_NEEDLE_SIZE
0 VALUE GENERATED_START_OFFSET
0 VALUE GENERATED_END_OFFSET
0 VALUE GENERATED_PREFIX_END
0 VALUE GENERATED_LINE_OFFSET
0 VALUE GENERATED_ARRAY_BYTES
0 VALUE GENERATED_ARRAY_NAME
0 VALUE GENERATED_ARRAY_NAME_SIZE
0 VALUE GENERATED_ARRAY_INDEX
0 VALUE GENERATED_ARRAY_COLUMN
0 VALUE GENERATED_HEADER_FILE

: AppendGeneratedByte ( byte -- )
    GENERATED_HEADER ByteVector.EmplaceBack C!
;

: AppendGeneratedSlice ( c-addr u -- )
    GENERATED_HEADER ByteVector.AppendSlice
;

: HexDigit ( nibble -- char )
    DUP 10 < IF
        [CHAR] 0 +
    ELSE
        10 - [CHAR] A +
    THEN
;

: AppendHexByte ( byte -- )
    [CHAR] 0 AppendGeneratedByte
    [CHAR] x AppendGeneratedByte
    DUP 4 RSHIFT HexDigit AppendGeneratedByte
    0xF AND HexDigit AppendGeneratedByte
;

: ByteVectorFindSliceOffset ( vector c-addr u -- offset|-1 )
    TO GENERATED_NEEDLE_SIZE
    TO GENERATED_NEEDLE
    TO GENERATED_HAYSTACK

    GENERATED_HAYSTACK ByteVector.Size
    GENERATED_NEEDLE_SIZE
    < IF
        -1 EXIT
    THEN

    GENERATED_HAYSTACK ByteVector.Size
    GENERATED_NEEDLE_SIZE
    - 1+
    0 ?DO
        GENERATED_HAYSTACK ByteVector.Data I +
        GENERATED_NEEDLE_SIZE
        GENERATED_NEEDLE
        GENERATED_NEEDLE_SIZE
        COMPARE 0= IF
            I UNLOOP EXIT
        THEN
    LOOP

    -1
;

: HeaderLineEnd ( offset -- offset-after-line )
    TO GENERATED_LINE_OFFSET

    BEGIN
        GENERATED_LINE_OFFSET CUR_FILE_DATA ByteVector.Size <
    WHILE
        GENERATED_LINE_OFFSET CUR_FILE_DATA ByteVector.At C@ 10 = IF
            GENERATED_LINE_OFFSET 1+ EXIT
        THEN
        GENERATED_LINE_OFFSET 1+ TO GENERATED_LINE_OFFSET
    REPEAT

    S" Generated-array marker line is incomplete" Fail
;

: EmitGeneratedArray ( bytes name-addr name-size -- )
    TO GENERATED_ARRAY_NAME_SIZE
    TO GENERATED_ARRAY_NAME
    TO GENERATED_ARRAY_BYTES

    S" constexpr std::array<char8_t, " GENERATED_ARRAY_BYTES ByteVector.Size 0 <# #S #> S" > " S+ S+ AppendGeneratedSlice
    GENERATED_ARRAY_NAME GENERATED_ARRAY_NAME_SIZE AppendGeneratedSlice
    S"  {{" AppendGeneratedSlice
    10 AppendGeneratedByte

    0 TO GENERATED_ARRAY_INDEX
    BEGIN
        GENERATED_ARRAY_INDEX GENERATED_ARRAY_BYTES ByteVector.Size <
    WHILE
        S"     " AppendGeneratedSlice
        0 TO GENERATED_ARRAY_COLUMN
        BEGIN
            GENERATED_ARRAY_COLUMN 16 <
            GENERATED_ARRAY_INDEX GENERATED_ARRAY_BYTES ByteVector.Size <
            AND
        WHILE
            GENERATED_ARRAY_INDEX GENERATED_ARRAY_BYTES ByteVector.At C@
            AppendHexByte

            GENERATED_ARRAY_INDEX 1+ TO GENERATED_ARRAY_INDEX
            GENERATED_ARRAY_COLUMN 1+ TO GENERATED_ARRAY_COLUMN
            GENERATED_ARRAY_INDEX GENERATED_ARRAY_BYTES ByteVector.Size < IF
                [CHAR] , AppendGeneratedByte
                GENERATED_ARRAY_COLUMN 16 < IF
                    BL AppendGeneratedByte
                THEN
            THEN
        REPEAT
        10 AppendGeneratedByte
    REPEAT

    S" }};" AppendGeneratedSlice
    10 AppendGeneratedByte
;

: WriteGeneratedHeader ( -- )
    S" ../unicode_name_set.h" LoadUnicodeFile

    CUR_FILE_DATA S" // START" ByteVectorFindSliceOffset
    TO GENERATED_START_OFFSET
    GENERATED_START_OFFSET 0< IF
        S" // START marker was not found in ../unicode_name_set.h" Fail
    THEN

    CUR_FILE_DATA S" // END" ByteVectorFindSliceOffset
    TO GENERATED_END_OFFSET
    GENERATED_END_OFFSET GENERATED_START_OFFSET <= IF
        S" // END marker was not found after // START" Fail
    THEN

    GENERATED_START_OFFSET HeaderLineEnd TO GENERATED_PREFIX_END

    GENERATED_HEADER ByteVector.Clear
    CUR_FILE_DATA ByteVector.Data
    GENERATED_PREFIX_END
    AppendGeneratedSlice

    UNICODE_DICT S" unicode_name_dict" EmitGeneratedArray
    10 AppendGeneratedByte
    UNICODE_NAME_NODES S" unicode_name_nodes" EmitGeneratedArray
    10 AppendGeneratedByte

    CUR_FILE_DATA ByteVector.Data GENERATED_END_OFFSET +
    CUR_FILE_DATA ByteVector.Size GENERATED_END_OFFSET -
    AppendGeneratedSlice

    S" unicode_name_set.h.tmp" W/O BIN CREATE-FILE THROW
    TO GENERATED_HEADER_FILE
    GENERATED_HEADER ByteVector.Data
    GENERATED_HEADER ByteVector.Size
    GENERATED_HEADER_FILE WRITE-FILE THROW
    GENERATED_HEADER_FILE CLOSE-FILE THROW

    S" unicode_name_set.h.tmp"
    S" ../unicode_name_set.h"
    RENAME-FILE THROW
;

: PrepareData
    S" mkdir -p tmp" ShOk
    S" curl -L -o tmp/UnicodeData.txt https://www.unicode.org/Public/18.0.0/ucd/UnicodeData.txt" ShOk
    S" curl -L -o tmp/NameAliases.txt https://www.unicode.org/Public/18.0.0/ucd/NameAliases.txt" ShOk
;

: Build
    TRUE TO PARSE_PRIMARY_UNICODE_DATA
    S" tmp/UnicodeData.txt" LoadUnicodeFile ParseData
    FALSE TO PARSE_PRIMARY_UNICODE_DATA
    S" tmp/NameAliases.txt" LoadUnicodeFile ParseData

    CR ." [1] BuildTree..." CR
    BuildTree
    ." [1] BuildTree Done" CR

    ." [2] CompressTree..." CR
    CompressTree
    ." [2] CompressTree Done" CR

    ." [3] Sort names..." CR
    NAMES UnicodeNameVector.SortByIndex
    ." [3] Sort Done" CR

    ." [4] Unique names..." CR
    NAMES UnicodeNameVector.Unique

    ." Unique names: "
    NAMES UnicodeNameVector.Size . CR

    ." [4] Unique Done" CR

    ." [5] BuildDictionary..." CR
    BuildDictionary
    ." [5] Dictionary Done" CR

    ." [6] BuildUnicodeNameNodes..." CR
    BuildUnicodeNameNodes
    ." [6] BuildUnicodeNameNodes Done" CR

    ." [7] WriteGeneratedHeader..." CR
    WriteGeneratedHeader
    ." [7] WriteGeneratedHeader Done" CR
;

: DeleteFileIfExits ( c-addr u -- )
    2DUP FILE-STATUS NIP 0= IF
        DELETE-FILE DROP
    ELSE
        2DROP
    THEN
;

: Main ( -- )
    S" tmp/UnicodeData.txt" DeleteFileIfExits
    S" tmp/NameAliases.txt" DeleteFileIfExits
    PrepareData
    Build

    ." UnicodeMap entries: "
    UNICODE_MAP UnicodeMap.Size . CR

    ." Trie nodes: "
    NODES UnicodeNodeVector.Size . CR

    ." Leaf nodes: "
    LEAF_NODES UnicodeNodePointerList.Size . CR

    ." Unicode name node bytes: "
    UNICODE_NAME_NODES ByteVector.Size . CR
;

EndImport

Main

BYE