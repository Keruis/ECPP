REQUIRE byte_vector.fs

Import ByteVectorModule

BeginModule UnicodeNodeVectorModule

Export Import ByteVectorModule

: (UnicodeNodePointerDefault) ( dest -- )
    0 SWAP !
;

BeginElementType UnicodeNodePointer
    CELL ElementSize
    CELL ElementAlignment
    RawCopyable
    RawRelocatable
    ' (UnicodeNodePointerDefault) UseDefaultConstruct
    NoDestroy
EndElementType

DefineVectorType UnicodeNodeChildren UnicodeNodePointer

Export : UnicodeNode.Free ( node -- )
    ?DUP IF
        DUP CELL+ @ ?DUP IF
            ByteVector.Free
        THEN

        DUP 4 CELLS + @ ?DUP IF
            UnicodeNodeChildren.Free
        THEN

        FREE THROW
    THEN
;

: (UnicodeNodeInitialize) ( node -- )
    DUP >R
    ByteVector.New R@ CELL+ !
    UnicodeNodeChildren.New R@ 4 CELLS + !
    R> 2DROP
;

Export : UnicodeNode.New ( -- node )
    5 CELLS ALLOCATE THROW
    DUP 5 CELLS ERASE

    DUP ['] (UnicodeNodeInitialize) CATCH
    DUP IF
        >R
        UnicodeNode.Free
        R> THROW
    THEN
    DROP
;

Export : UnicodeNode.Parent ( node -- parent )
    @
;

Export : UnicodeNode.SetParent ( parent node -- )
    !
;

Export : UnicodeNode.Name ( node -- byte-vector )
    CELL+ @
;

Export : UnicodeNode.SetName ( c-addr u node -- )
    >R
    R@ UnicodeNode.Name ByteVector.Clear
    R> UnicodeNode.Name ByteVector.AppendSlice
;

Export : UnicodeNode.AppendName ( c-addr u node -- )
    >R
    R> UnicodeNode.Name ByteVector.AppendSlice
;

Export : UnicodeNode.HasCodePoint? ( node -- flag )
    2 CELLS + @ 0<>
;

Export : UnicodeNode.CodePoint ( node -- u )
    3 CELLS + @
;

Export : UnicodeNode.GetCodePoint ( node -- code-point true | false )
    DUP UnicodeNode.HasCodePoint? IF
        3 CELLS + @ TRUE
    ELSE
        DROP FALSE
    THEN
;

Export : UnicodeNode.SetCodePoint ( code-point node -- )
    >R
    R@ 3 CELLS + !
    -1 R> 2 CELLS + !
;

Export : UnicodeNode.ClearCodePoint ( node -- )
    DUP 2 CELLS + 0 SWAP !
    DUP 3 CELLS + 0 SWAP !
    DROP
;

Export : UnicodeNode.Children ( node -- children-vector )
    4 CELLS + @
;

Export : UnicodeNode.ChildCount ( node -- u )
    UnicodeNode.Children UnicodeNodeChildren.Size
;

Export : UnicodeNode.ChildAt ( node index -- child )
    SWAP UnicodeNode.Children
    UnicodeNodeChildren.At @
;

Export : UnicodeNode.AddChild ( parent child -- )
    DUP >R
    OVER UnicodeNode.Children
    UnicodeNodeChildren.EmplaceBack
    R@ SWAP !
    2DUP UnicodeNode.SetParent
    R> DROP
    2DROP
;

: (UnicodeNodeHandleDefault) ( dest -- )
    UnicodeNode.New SWAP !
;

: (UnicodeNodeHandleMove) ( src dest -- )
    >R
    DUP @ R@ !
    0 SWAP !
    R> DROP
;

: (UnicodeNodeHandleDestroy) ( element -- )
    @ ?DUP IF
        UnicodeNode.Free
    THEN
;

: (UnicodeNodeHandleMoveAssign) ( src dest -- )
    2DUP = IF
        2DROP EXIT
    THEN

    DUP (UnicodeNodeHandleDestroy)
    (UnicodeNodeHandleMove)
;

BeginElementType UnicodeNodeHandle
    CELL ElementSize
    CELL ElementAlignment
    RawRelocatable
    NoThrowMove

    ' (UnicodeNodeHandleDefault) UseDefaultConstruct
    ' (UnicodeNodeHandleMove) UseMoveConstruct
    ' (UnicodeNodeHandleDestroy) UseDestroy
    ' (UnicodeNodeHandleMoveAssign) UseMoveAssign

    NoCopyConstruct
    NoCopyAssign
EndElementType

DefineVectorType UnicodeNodeVector UnicodeNodeHandle

Export : UnicodeNodeVector.EmplaceNode ( nodes -- node )
    UnicodeNodeVector.EmplaceBack @
;

Export : UnicodeNodeVector.NodeAt ( index nodes -- node )
    UnicodeNodeVector.At @
;

Export : UnicodeNodeVector.AddNode ( parent nodes -- node )
    >R
    R@ UnicodeNodeVector.EmplaceNode
    DUP >R
    OVER IF
        2DUP UnicodeNode.AddChild
    THEN
    2DROP
    R>
    R> DROP
;

Export : UnicodeNodePointerList.New ( -- list )
    UnicodeNodeChildren.New
;

Export : UnicodeNodePointerList.Free ( list -- )
    UnicodeNodeChildren.Free
;

Export : UnicodeNodePointerList.Size ( list -- u )
    UnicodeNodeChildren.Size
;

Export : UnicodeNodePointerList.Data ( list -- addr )
    UnicodeNodeChildren.Data
;

Export : UnicodeNodePointerList.Clear ( list -- )
    UnicodeNodeChildren.Clear
;

Export : UnicodeNodePointerList.Reserve ( min-capacity list -- )
    UnicodeNodeChildren.Reserve
;

Export : UnicodeNodePointerList.At ( index list -- node )
    UnicodeNodeChildren.At @
;

Export : UnicodeNodePointerList.Push ( node list -- )
    UnicodeNodeChildren.EmplaceBack !
;

Export : UnicodeNodePointerList.Pop ( list -- node )
    DUP UnicodeNodeChildren.Back @
    SWAP UnicodeNodeChildren.PopBack
;

0 VALUE UCN_LIST_NODE
0 VALUE UCN_LIST_INDEX
0 VALUE UCN_LIST_HANDLE

Export : UnicodeNodePointerList.Set ( node index list -- )
    TO UCN_LIST_HANDLE
    TO UCN_LIST_INDEX
    TO UCN_LIST_NODE

    UCN_LIST_INDEX UCN_LIST_HANDLE UnicodeNodeChildren.At
    UCN_LIST_NODE SWAP !
;

0 VALUE UCN_MOVE_SOURCE
0 VALUE UCN_MOVE_DESTINATION
0 VALUE UCN_MOVE_CHILDREN

Export : UnicodeNode.MoveChildren ( source destination -- )
    TO UCN_MOVE_DESTINATION
    TO UCN_MOVE_SOURCE

    UCN_MOVE_SOURCE 4 CELLS + @ TO UCN_MOVE_CHILDREN

    UCN_MOVE_DESTINATION 4 CELLS + @
    UnicodeNodeChildren.Free

    UCN_MOVE_CHILDREN
    UCN_MOVE_DESTINATION 4 CELLS + !

    UnicodeNodeChildren.New
    UCN_MOVE_SOURCE 4 CELLS + !

    UCN_MOVE_DESTINATION UnicodeNode.ChildCount 0 ?DO
        UCN_MOVE_DESTINATION I UnicodeNode.ChildAt
        UCN_MOVE_DESTINATION SWAP UnicodeNode.SetParent
    LOOP
;

EndModule

EndImport