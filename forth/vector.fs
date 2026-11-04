REQUIRE elem_type.fs
REQUIRE iterator.fs

Import ElemType
Import Iterator
+

BeginModule Vector

Export Import ElemType
Export Import Iterator

VARIABLE (VectorPrivateWid)
VARIABLE (VectorPublicWid)
VARIABLE (VectorModuleBase)

GET-CURRENT (VectorPrivateWid) !
2DUP
(VectorModuleBase) !
(VectorPublicWid) !

: (EnvironmentMaxN) ( -- max-n )
    S" max-n" ENVIRONMENT? 0=
    ABORT" Vector: Gforth does not report max-n"
;

(EnvironmentMaxN) CONSTANT (MAX-N)

S" Vector: index out of range" EXCEPTION CONSTANT (VEC-ERR-INDEX)
S" Vector: negative length" EXCEPTION CONSTANT (VEC-ERR-LENGTH)
S" Vector: capacity or byte size overflow" EXCEPTION CONSTANT (VEC-ERR-OVERFLOW)
S" Vector: operation unsupported by element type" EXCEPTION CONSTANT (VEC-ERR-UNSUPPORTED)
S" Vector: internal state is inconsistent" EXCEPTION CONSTANT (VEC-ERR-STATE)

Export : VectorIndexError ( -- n )
    (VEC-ERR-INDEX)
;

Export : VectorLengthError ( -- n )
    (VEC-ERR-LENGTH)
;

Export : VectorOverflowError ( -- n )
    (VEC-ERR-OVERFLOW)
;

Export : VectorUnsupportedError ( -- n )
    (VEC-ERR-UNSUPPORTED)
;

Export : VectorTypeError ( -- n )
    ElementTypeError
;

Export : VectorStateError ( -- n )
    (VEC-ERR-STATE)
;

: (AlignUp) ( addr alignment -- aligned-addr )
    1-
    TUCK +
    SWAP INVERT AND
;

: (CheckedProduct) ( unsigned1 unsigned2 -- product )
    2DUP OR 0<
    IF
        2DROP (VEC-ERR-LENGTH) THROW
    THEN
    UM*
    DUP
    IF
        2DROP (VEC-ERR-OVERFLOW) THROW
    THEN
    DROP
    DUP (MAX-N) U>
    IF
        (VEC-ERR-OVERFLOW) THROW
    THEN
;

: (CheckedAdd) ( unsigned1 unsigned2 -- sum )
    2DUP OR 0<
    IF
        2DROP (VEC-ERR-LENGTH) THROW
    THEN
    +
    DUP 0<
    IF
        (VEC-ERR-OVERFLOW) THROW
    THEN
;

: (ValidAlignment) ( alignment -- flag )
    DUP 0>
    SWAP DUP 1- AND 0=
    AND
;

: (AllocateDataIOR) ( bytes alignment -- data ior )
    2DUP OR 0<
    IF
        2DROP (VEC-ERR-LENGTH) -1
        EXIT
    THEN
    DUP (ValidAlignment) 0=
    IF
        2DROP (VEC-ERR-LENGTH) -1
        EXIT
    THEN
    >R
    R@ 1- CELL +
    OVER +
    DUP 0<
    IF
        DROP R> DROP (VEC-ERR-OVERFLOW) -1
        EXIT
    THEN
    NIP
    ALLOCATE
    DUP
    IF
        >R
        DROP
        R> R>
        SWAP
        EXIT
    THEN
    DROP
    DUP CELL +
    R@ (AlignUp)
    TUCK
    1 CELLS - !
    R> DROP
    0
;

: (AllocateData) ( bytes alignment -- data )
    (AllocateDataIOR) THROW
;

: (AllocateCapacityData) ( capacity elem-size alignment -- data )
    >R
    (CheckedProduct)
    R>
    (AllocateData)
;

: (FreeData) ( data -- )
    ?DUP
    IF
        1 CELLS - @ FREE THROW
    THEN
;

: (AllocHeader) ( -- vec )
    3 CELLS ALLOCATE
    DUP
    IF
        NIP (VEC-ERR-STATE) THROW
    THEN
    DROP
    DUP 3 CELLS ERASE
;

: (FreeHeader) ( vec -- )
    FREE THROW
;

: (Guard2) ( x1 x2 -- x1 x1 x2 )
    OVER -ROT
;

: (Guard4) ( x1 x2 x3 x4 -- x4 x1 x2 x3 x4 )
    >R
    R@
    3 ROLL
    3 ROLL
    3 ROLL
    R>
;

: (InitVectorData) ( data capacity vec -- vec )
    >R
    SWAP
    R@ !
    R@ 2 CELLS + !
    R>
;

: (NewTempVector) ( data capacity -- temp )
    3 CELLS ALLOCATE THROW
    DUP 3 CELLS ERASE
    (InitVectorData)
;

: (FinishNew) ( capacity bytes alignment vec -- vec )
    >R
    ROT
    >R
    (AllocateData)
    R> R>
    SWAP
    OVER 2 CELLS + !
    TUCK !
;

: (NewExact) ( capacity elem-size alignment -- vec )
    OVER 0<
    IF
        2DROP (VEC-ERR-LENGTH) THROW
    THEN
    >R
    OVER >R
    (CheckedProduct)
    R>
    R>
    ROT SWAP
    (AllocHeader)
    (Guard4)
    ['] (FinishNew) CATCH
    DUP
    IF
        >R
        2DROP 2DROP
        (FreeHeader)
        R> THROW
    ELSE
        DROP
        SWAP DROP
    THEN
;

: (VecNew) ( -- vec )
    (AllocHeader)
;

: (VecMoveHandle) ( vec -- moved-vec )
    (AllocHeader)
    >R
    DUP R@ 3 CELLS MOVE
    DUP 3 CELLS ERASE
    DROP
    R>
;

: (VecSize) ( vec -- u )
    CELL + @
;

: (VecCapacity) ( vec -- u )
    2 CELLS + @
;

: (VecEmpty) ( vec -- flag )
    CELL + @ 0=
;

: (VecData) ( vec -- addr )
    @
;

: (VecIndex) ( index vec elem-size -- addr )
    >R
    R@
    SWAP
    (VecData)
    >R
    *
    R>
    +
    R> DROP
;

: (IndexedAddress) ( index base elem-size -- addr )
    >R
    SWAP
    R> *
    +
;

: (SourceIndex) ( source vec elem-size -- index|-1 )
    >R
    DUP @
    OVER CELL+ @ R@ *
    OVER +
    ROT DROP
    >R
    2DUP U<
    IF
        2DROP R> DROP R> DROP -1
        EXIT
    THEN
    R>
    2 PICK OVER U<
    IF
        DROP - R> /
        EXIT
    THEN
    DROP 2DROP R> DROP -1
;

: (RefreshSource) ( source index vec elem-size -- source' )
    >R
    >R
    DUP 0<
    IF
        DROP R> DROP R> DROP
        EXIT
    THEN
    R> R>
    (VecIndex)
    NIP
;

: (VecEndAddress) ( vec elem-size -- addr )
    >R
    DUP (VecSize)
    SWAP (VecData)
    SWAP
    R> *
    +
;

: (IncrementSize) ( vec -- vec )
    1 OVER CELL+ +!
;

: (VecAt) ( index vec elem-size -- addr )
    2 PICK
    2 PICK (VecSize) U>=
    IF
        DROP 2DROP (VEC-ERR-INDEX) THROW
    THEN
    (VecIndex)
;

: (VecFront) ( vec elem-size -- addr )
    OVER (VecSize) 0=
    IF
        2DROP (VEC-ERR-INDEX) THROW
    THEN
    DROP (VecData)
;

: (VecBack) ( vec elem-size -- addr )
    >R
    DUP
    (VecSize)
    ?DUP 0=
    IF
        DROP R> DROP (VEC-ERR-INDEX) THROW
    THEN
    1-
    SWAP
    R> (VecIndex)
;

: (NextCapacity) ( needed capacity elem-size -- capacity' )
    >R
    OVER 0<
    IF
        R> DROP (VEC-ERR-LENGTH) THROW
    THEN
    DUP 0<
    IF
        R> DROP (VEC-ERR-LENGTH) THROW
    THEN
    OVER R@ (CheckedProduct) DROP
    DUP (MAX-N) 2/ >
    IF
        DROP (MAX-N)
    ELSE
        2*
    THEN
    (MAX-N) R@ / MIN
    OVER MAX
    DUP R@ (CheckedProduct) DROP
    R> DROP
    NIP
;

: (ZeroBytes) ( addr size -- )
    ERASE
;

: (RawCopyElement) ( src dest size -- )
    MOVE
;

: (UnsupportedConstruct) ( src dest -- )
    2DROP (VEC-ERR-UNSUPPORTED) THROW
;

: (UnsupportedDestroy) ( addr -- )
    DROP (VEC-ERR-UNSUPPORTED) THROW
;

: (NoOpDrop) ( x -- )
    DROP
;

: (NoOpCopyAssign) ( src dest -- )
    2DROP (VEC-ERR-UNSUPPORTED) THROW
;

: (SwapCell) ( addr1 addr2 -- )
    >R
    DUP @
    R@ @
    SWAP
    R@ !
    SWAP !
    R> DROP
;

: (VecSwap) ( vec1 vec2 -- )
    2DUP (SwapCell)
    2DUP CELL+ SWAP CELL+ SWAP (SwapCell)
    2DUP 2 CELLS + SWAP 2 CELLS + SWAP (SwapCell)
    2DROP
;

256 CONSTANT (VECTOR-NAME-BYTES)

VARIABLE (VectorNameLen)
CREATE (VectorNameBuffer) (VECTOR-NAME-BYTES) ALLOT
VARIABLE (VectorType)

VARIABLE (VectorElemSizeXt)
VARIABLE (VectorElemAlignXt)

VARIABLE (VectorDefaultXt)
VARIABLE (VectorCopyXt)
VARIABLE (VectorMoveXt)
VARIABLE (VectorDestroyXt)
VARIABLE (VectorCopyAssignXt)
VARIABLE (VectorMoveAssignXt)

VARIABLE (VectorDefaultAdapter)
VARIABLE (VectorCopyAdapter)
VARIABLE (VectorMoveAdapter)
VARIABLE (VectorDestroyAdapter)
VARIABLE (VectorCopyAssignAdapter)
VARIABLE (VectorMoveAssignAdapter)

VARIABLE (VectorDefaultAdapterXt)
VARIABLE (VectorCopyAdapterXt)
VARIABLE (VectorMoveAdapterXt)
VARIABLE (VectorDestroyAdapterXt)
VARIABLE (VectorCopyAssignAdapterXt)
VARIABLE (VectorMoveAssignAdapterXt)

VARIABLE (VectorCopySupported)
VARIABLE (VectorMoveSupported)
VARIABLE (VectorCopyAssignSupported)
VARIABLE (VectorMoveAssignSupported)
VARIABLE (VectorRelocationKind)
VARIABLE (VectorDestroyRangeXt)
VARIABLE (VectorBuildDefaultXt)
VARIABLE (VectorBuildCopyXt)
VARIABLE (VectorBuildMoveXt)

VARIABLE (VectorIteratorNextXt)
VARIABLE (VectorIteratorPrevXt)
VARIABLE (VectorIteratorAdvanceXt)
VARIABLE (VectorIteratorDistanceXt)
VARIABLE (VectorIteratorLessXt)
VARIABLE (VectorIteratorReadXt)
VARIABLE (VectorIteratorEqualXt)
VARIABLE (VectorIteratorWriteXt)
VARIABLE (VectorIteratorSwapXt)
VARIABLE (VectorIteratorBeginXt)
VARIABLE (VectorIteratorEndXt)

0 CONSTANT (RELOCATE-UNSUPPORTED)
1 CONSTANT (RELOCATE-RAW)
2 CONSTANT (RELOCATE-MOVE)
3 CONSTANT (RELOCATE-COPY)

: (VectorName) ( suffix-addr suffix-u -- name-addr name-u )
    DUP (VectorNameLen) @ + (VECTOR-NAME-BYTES) >
    IF
        2DROP S" Vector: generated name is too long" Fail
    THEN
    DUP >R
    (VectorNameBuffer) (VectorNameLen) @ +
    SWAP
    MOVE
    R> (VectorNameLen) @ +
    (VectorNameBuffer) SWAP
;

: (StartPrivate) ( c-addr u -- xt )
    GET-CURRENT >R
    (VectorPrivateWid) @ SET-CURRENT
    NEXTNAME
    ['] : EXECUTE
    LATEST NAME>INT
    R> SET-CURRENT
;

: (StartPublic) ( c-addr u -- xt )
    GET-CURRENT >R
    (VectorPublicWid) @ SET-CURRENT
    NEXTNAME
    ['] : EXECUTE
    LATEST NAME>INT
    R> SET-CURRENT
;

: (StartPrivateConstant) ( n c-addr u -- xt )
    GET-CURRENT >R
    (VectorPrivateWid) @ SET-CURRENT
    NEXTNAME
    ['] CONSTANT EXECUTE
    LATEST NAME>INT
    R> SET-CURRENT
;

: (GenerateSimple) ( suffix-addr suffix-u xt -- )
    >R
    (VectorName)
    (StartPublic)
    R> SWAP >R
    COMPILE,
    POSTPONE ;
    R> DROP
;

: (GenerateValueWithSize) ( suffix-addr suffix-u xt -- )
    >R
    (VectorName)
    (StartPublic)
    R> SWAP >R
    (VectorElemSizeXt) @ COMPILE,
    COMPILE,
    POSTPONE ;
    R> DROP
;

: (GenerateNewWithCapacity) ( -- )
    S" .NewWithCapacity" (VectorName)
    (StartPublic)
    >R
    (VectorElemSizeXt) @ COMPILE,
    (VectorElemAlignXt) @ COMPILE,
    ['] (NewExact) COMPILE,
    R> DROP
    POSTPONE ;
;

: (GeneratePrivateOneCall) ( suffix-addr suffix-u xt -- generated-xt )
    >R
    (VectorName)
    (StartPrivate)
    R> SWAP >R
    COMPILE,
    POSTPONE ;
    R>
;

: (GeneratePrivateRaw) ( suffix-addr suffix-u size-xt helper-xt -- generated-xt )
    >R >R
    (VectorName)
    (StartPrivate)
    R> SWAP >R
    COMPILE,
    R> R> SWAP >R
    COMPILE,
    POSTPONE ;
    R>
;

: (GenerateMetadataConstants) ( -- )
    (VectorType) @ ElementTypeSize
    S" #ElementSize" (VectorName)
    (StartPrivateConstant)
    (VectorElemSizeXt) !

    (VectorType) @ ElementTypeAlignment
    S" #ElementAlignment" (VectorName)
    (StartPrivateConstant)
    (VectorElemAlignXt) !
;

: (LoadElementBindings) ( -- )
    (VectorType) @ ElementTypeDefaultConstructXt (VectorDefaultXt) !
    (VectorType) @ ElementTypeCopyConstructXt (VectorCopyXt) !
    (VectorType) @ ElementTypeMoveConstructXt (VectorMoveXt) !
    (VectorType) @ ElementTypeDestroyXt (VectorDestroyXt) !
    (VectorType) @ ElementTypeCopyAssignXt (VectorCopyAssignXt) !
    (VectorType) @ ElementTypeMoveAssignXt (VectorMoveAssignXt) !

    (VectorType) @ ElementTypeCopyConstructible? (VectorCopySupported) !
    (VectorType) @ ElementTypeMoveConstructible? (VectorMoveSupported) !
    (VectorType) @ ElementTypeCopyAssignable? (VectorCopyAssignSupported) !
    (VectorType) @ ElementTypeMoveAssignable? (VectorMoveAssignSupported) !

    (VectorType) @ ElementTypeRawRelocatable?
    IF
        (RELOCATE-RAW) (VectorRelocationKind) !
    ELSE
        (VectorType) @ ElementTypeNoThrowMove?
        (VectorMoveSupported) @ AND
        IF
            (RELOCATE-MOVE) (VectorRelocationKind) !
        ELSE
            (VectorCopySupported) @
            IF
                (RELOCATE-COPY) (VectorRelocationKind) !
            ELSE
                (VectorMoveSupported) @
                IF
                    (RELOCATE-MOVE) (VectorRelocationKind) !
                ELSE
                    (RELOCATE-UNSUPPORTED) (VectorRelocationKind) !
                THEN
            THEN
        THEN
    THEN
;

: (GenerateDefaultAdapter) ( -- )
    (VectorDefaultXt) @ ?DUP
    IF
        S" #DefaultOne" ROT (GeneratePrivateOneCall)
    ELSE
        (VectorElemSizeXt) @ ['] (ZeroBytes)
        S" #DefaultOne" 2SWAP (GeneratePrivateRaw)
    THEN
    (VectorDefaultAdapter) !
;

: (GenerateCopyAdapter) ( -- )
    (VectorCopyXt) @ ?DUP
    IF
        S" #CopyOne" ROT (GeneratePrivateOneCall)
    ELSE
        (VectorCopySupported) @
        IF
            (VectorElemSizeXt) @ ['] (RawCopyElement)
            S" #CopyOne" 2SWAP (GeneratePrivateRaw)
        ELSE
            S" #CopyOne" ['] (UnsupportedConstruct)
            (GeneratePrivateOneCall)
        THEN
    THEN
    (VectorCopyAdapter) !
;

: (GenerateMoveAdapter) ( -- )
    (VectorMoveXt) @ ?DUP
    IF
        S" #MoveOne" ROT (GeneratePrivateOneCall)
    ELSE
        (VectorMoveSupported) @
        IF
            (VectorElemSizeXt) @ ['] (RawCopyElement)
            S" #MoveOne" 2SWAP (GeneratePrivateRaw)
        ELSE
            S" #MoveOne" ['] (UnsupportedConstruct)
            (GeneratePrivateOneCall)
        THEN
    THEN
    (VectorMoveAdapter) !
;

: (GenerateDestroyAdapter) ( -- )
    (VectorDestroyXt) @ ?DUP
    IF
        S" #DestroyOne" ROT (GeneratePrivateOneCall)
    ELSE
        S" #DestroyOne" ['] (NoOpDrop)
        (GeneratePrivateOneCall)
    THEN
    (VectorDestroyAdapter) !
;

: (GenerateCopyAssignAdapter) ( -- )
    (VectorCopyAssignXt) @ ?DUP
    IF
        S" #CopyAssignOne" ROT (GeneratePrivateOneCall)
    ELSE
        (VectorCopySupported) @
        IF
            (VectorElemSizeXt) @ ['] (RawCopyElement)
            S" #CopyAssignOne" 2SWAP (GeneratePrivateRaw)
        ELSE
            S" #CopyAssignOne" ['] (NoOpCopyAssign)
            (GeneratePrivateOneCall)
        THEN
    THEN
    (VectorCopyAssignAdapter) !
;

: (GenerateMoveAssignAdapter) ( -- )
    (VectorMoveAssignXt) @ ?DUP
    IF
        S" #MoveAssignOne" ROT (GeneratePrivateOneCall)
    ELSE
        (VectorMoveSupported) @
        IF
            (VectorElemSizeXt) @ ['] (RawCopyElement)
            S" #MoveAssignOne" 2SWAP (GeneratePrivateRaw)
        ELSE
            (VectorCopyAssignSupported) @
            IF
                (VectorCopyAssignAdapter) @
                S" #MoveAssignOne" ROT (GeneratePrivateOneCall)
            ELSE
                S" #MoveAssignOne" ['] (NoOpCopyAssign)
                (GeneratePrivateOneCall)
            THEN
        THEN
    THEN
    (VectorMoveAssignAdapter) !
;

: (GenerateAdapterConstants) ( -- )
    (VectorDefaultAdapter) @
    S" #DefaultAdapterXt" (VectorName)
    (StartPrivateConstant)
    (VectorDefaultAdapterXt) !

    (VectorCopyAdapter) @
    S" #CopyAdapterXt" (VectorName)
    (StartPrivateConstant)
    (VectorCopyAdapterXt) !

    (VectorMoveAdapter) @
    S" #MoveAdapterXt" (VectorName)
    (StartPrivateConstant)
    (VectorMoveAdapterXt) !

    (VectorDestroyAdapter) @
    S" #DestroyAdapterXt" (VectorName)
    (StartPrivateConstant)
    (VectorDestroyAdapterXt) !

    (VectorCopyAssignAdapter) @
    S" #CopyAssignAdapterXt" (VectorName)
    (StartPrivateConstant)
    (VectorCopyAssignAdapterXt) !

    (VectorMoveAssignAdapter) @
    S" #MoveAssignAdapterXt" (VectorName)
    (StartPrivateConstant)
    (VectorMoveAssignAdapterXt) !
;

: (GenerateAdapters) ( -- )
    (GenerateMetadataConstants)
    (LoadElementBindings)
    (GenerateDefaultAdapter)
    (GenerateCopyAdapter)
    (GenerateMoveAdapter)
    (GenerateDestroyAdapter)
    (GenerateCopyAssignAdapter)
    (GenerateMoveAssignAdapter)
    (VectorElemSizeXt) @ ['] (RawCopyElement)
    S" #RawMoveOne" 2SWAP (GeneratePrivateRaw)
    DROP
    (GenerateAdapterConstants)
;

16384 CONSTANT (VECTOR-CODE-BYTES)
VARIABLE (CodeLen)
CREATE (CodeBuffer) (VECTOR-CODE-BYTES) ALLOT

: (CodeReset) ( -- )
    0 (CodeLen) !
;

: (Code+) ( c-addr u -- )
    DUP (CodeLen) @ + (VECTOR-CODE-BYTES) >
    IF
        2DROP S" Vector: generated method is too long" Fail
    THEN
    DUP >R
    (CodeBuffer) (CodeLen) @ +
    SWAP MOVE
    R> (CodeLen) +!
;

: (CodeName+) ( suffix-addr suffix-u -- )
    (VectorName) (Code+)
    S"  " (Code+)
;

: (CodePrivateDefine) ( suffix-addr suffix-u -- )
    (CodeReset)
    S" : " (Code+)
    (CodeName+)
    S"  " (Code+)
;

: (EvaluatePrivateCode) ( -- xt )
    GET-CURRENT >R
    (VectorPrivateWid) @ SET-CURRENT
    GET-ORDER 1+
    (VectorPrivateWid) @
    SWAP SET-ORDER
    (CodeBuffer) (CodeLen) @ EVALUATE
    LATEST NAME>INT
    GET-ORDER 1- >R DROP R> SET-ORDER
    R> SET-CURRENT
;

: (CodePrivateFinish) ( -- xt )
    S"  ;" (Code+)
    (EvaluatePrivateCode)
;

: (GenerateVectorRangeWords) ( -- )
    S" #DestroyRange" (CodePrivateDefine)
    S" DUP >R OVER CELL+ @ SWAP ?DO I OVER " (Code+)
    S" #ElementSize" (CodeName+)
    S" (VecIndex) " (Code+)
    S" #DestroyOne" (CodeName+)
    S" LOOP R> OVER CELL+ !" (Code+)
    (CodePrivateFinish) (VectorDestroyRangeXt) !

    S" #AppendDefaults" (CodePrivateDefine)
    S" 0 ?DO DUP " (Code+)
    S" #ElementSize" (CodeName+)
    S" (VecEndAddress) " (Code+)
    S" #DefaultOne" (CodeName+)
    S" 1 OVER CELL+ +! LOOP" (Code+)
    (CodePrivateFinish) (VectorBuildDefaultXt) !

    S" #AppendCopies" (CodePrivateDefine)
    S" >R >R 2DUP @ SWAP @ R> R> OVER + SWAP ?DO " (Code+)
    S" I 2 PICK " (Code+)
    S" #ElementSize" (CodeName+)
    S" (IndexedAddress) I 2 PICK " (Code+)
    S" #ElementSize" (CodeName+)
    S" (IndexedAddress) " (Code+)
    S" #CopyOne" (CodeName+)
    S" 1 4 PICK CELL+ +! LOOP 2DROP DROP" (Code+)
    (CodePrivateFinish) (VectorBuildCopyXt) !

    S" #AppendMoves" (CodePrivateDefine)
    S" >R >R 2DUP @ SWAP @ R> R> OVER + SWAP ?DO " (Code+)
    S" I 2 PICK " (Code+)
    S" #ElementSize" (CodeName+)
    S" (IndexedAddress) I 2 PICK " (Code+)
    S" #ElementSize" (CodeName+)
    S" (IndexedAddress) " (Code+)
    S" #MoveOne" (CodeName+)
    S" 1 4 PICK CELL+ +! LOOP 2DROP DROP" (Code+)
    (CodePrivateFinish) (VectorBuildMoveXt) !

    S" #AppendRaw" (CodePrivateDefine)
    S" >R >R 2DUP @ SWAP @ R> R> OVER + SWAP ?DO " (Code+)
    S" I 2 PICK " (Code+)
    S" #ElementSize" (CodeName+)
    S" (IndexedAddress) I 2 PICK " (Code+)
    S" #ElementSize" (CodeName+)
    S" (IndexedAddress) " (Code+)
    S" #RawMoveOne" (CodeName+)
    S" 1 4 PICK CELL+ +! LOOP 2DROP DROP" (Code+)
    (CodePrivateFinish) DROP

    S" #MoveIndexed" (CodePrivateDefine)
    S" >R OVER R@ " (Code+)
    S" #ElementSize" (CodeName+)
    S" (VecIndex) SWAP R@ " (Code+)
    S" #ElementSize" (CodeName+)
    S" (VecIndex) " (Code+)
    S" #MoveOne" (CodeName+)
    S" DROP R> DROP" (Code+)
    (CodePrivateFinish) DROP

    S" #AppendPointer" (CodePrivateDefine)
    S" OVER 0 ?DO DUP " (Code+)
    S" #ElementSize" (CodeName+)
    S" (VecEndAddress) I 4 PICK " (Code+)
    S" #ElementSize" (CodeName+)
    S" (IndexedAddress) SWAP " (Code+)
    S" #CopyOne" (CodeName+)
    S" 1 OVER CELL+ +! LOOP 2DROP DROP" (Code+)
    (CodePrivateFinish) DROP
;

: (GenerateCoreLifecycleWords) ( -- )
    S" #Clear" (CodePrivateDefine)
    S" 0 " (Code+)
    S" #DestroyRange" (CodeName+)
    S" DROP" (Code+)
    (CodePrivateFinish)
    S" .Clear" ROT (GenerateSimple)

    S" #Free" (CodePrivateDefine)
    S" DUP " (Code+)
    S" #Clear" (CodeName+)
    S" DUP @ ?DUP IF (FreeData) THEN (FreeHeader)" (Code+)
    (CodePrivateFinish)
    S" .Free" ROT (GenerateSimple)

    S" #PopBack" (CodePrivateDefine)
    S" DUP " (Code+)
    S" #ElementSize" (CodeName+)
    S" (VecBack) " (Code+)
    S" #DestroyOne" (CodeName+)
    S" DUP CELL+ @ 1- SWAP CELL+ !" (Code+)
    (CodePrivateFinish)
    S" .PopBack" ROT (GenerateSimple)

    S" #NewWithSize" (CodePrivateDefine)
    S" DUP " (Code+)
    S" #ElementSize" (CodeName+)
    S" #ElementAlignment" (CodeName+)
    S" (NewExact) SWAP (Guard2) ['] " (Code+)
    S" #AppendDefaults" (CodeName+)
    S" CATCH DUP IF >R 2DROP DUP " (Code+)
    S" #Clear" (CodeName+)
    S" (FreeHeader) R> THROW ELSE DROP SWAP DROP THEN" (Code+)
    (CodePrivateFinish)
    S" .NewWithSize" ROT (GenerateSimple)

    S" #CopyInto" (CodePrivateDefine)
    S" DUP CELL+ @ 0 SWAP " (Code+)
    S" #AppendCopies" (CodeName+)
    (CodePrivateFinish)
    DROP

    S" #Copy" (CodePrivateDefine)
    S" DUP CELL+ @ DUP " (Code+)
    S" #ElementSize" (CodeName+)
    S" #ElementAlignment" (CodeName+)
    S" (NewExact) -ROT DROP (Guard2) ['] " (Code+)
    S" #CopyInto" (CodeName+)
    S" CATCH DUP IF >R 2DROP DUP " (Code+)
    S" #Clear" (CodeName+)
    S" (FreeHeader) R> THROW ELSE DROP SWAP DROP THEN" (Code+)
    (CodePrivateFinish)
    S" .Copy" ROT (GenerateSimple)

    S" #Erase" (CodePrivateDefine)
    S" 2DUP " (Code+)
    S" #ElementSize" (CodeName+)
    S" (VecAt) " (Code+)
    S" #DestroyOne" (CodeName+)
    S" DUP CELL+ @ ROT SWAP 1- SWAP ?DO " (Code+)
    S" I 1+ OVER " (Code+)
    S" #ElementSize" (CodeName+)
    S" (VecIndex) I 2 PICK " (Code+)
    S" #ElementSize" (CodeName+)
    S" (VecIndex) " (Code+)
    S" #MoveAssignOne" (CodeName+)
    S" LOOP DUP CELL+ @ 1- OVER " (Code+)
    S" #ElementSize" (CodeName+)
    S" (VecIndex) " (Code+)
    S" #DestroyOne" (CodeName+)
    S" -1 OVER CELL+ +! DROP" (Code+)
    (CodePrivateFinish)
    S" .Erase" ROT (GenerateSimple)
;

: (GenerateRawRelocation) ( -- )
    S" #FillRaw" (CodePrivateDefine)
    S" 2DUP 2>R DUP CELL+ @ 0 SWAP " (Code+)
    S" #AppendRaw" (CodeName+)
    S" 2R> ROT DROP" (Code+)
    (CodePrivateFinish) DROP

    S" #Relocate" (CodePrivateDefine)
    S" >R (Guard2) ['] (NewTempVector) CATCH DUP IF >R 2DROP " (Code+)
    S" (FreeHeader) R> THROW ELSE DROP SWAP DROP THEN R> " (Code+)
    S" (Guard2) ['] " (Code+)
    S" #FillRaw" (CodeName+)
    S" CATCH DUP IF >R 2DROP DUP " (Code+)
    S" #Clear" (CodeName+)
    S" (FreeHeader) R> THROW ELSE DROP ROT DROP THEN " (Code+)
    S" 2DUP (VecSwap) SWAP " (Code+)
    S" DUP @ ?DUP IF (FreeData) THEN (FreeHeader)" (Code+)
    (CodePrivateFinish) DROP
;

: (GenerateCopyRelocation) ( -- )
    S" #FillCopy" (CodePrivateDefine)
    S" 2DUP 2>R DUP CELL+ @ 0 SWAP " (Code+)
    S" #AppendCopies" (CodeName+)
    S" 2R> ROT DROP" (Code+)
    (CodePrivateFinish) DROP

    S" #Relocate" (CodePrivateDefine)
    S" >R (Guard2) ['] (NewTempVector) CATCH DUP IF >R 2DROP " (Code+)
    S" (FreeHeader) R> THROW ELSE DROP SWAP DROP THEN R> " (Code+)
    S" (Guard2) ['] " (Code+)
    S" #FillCopy" (CodeName+)
    S" CATCH DUP IF >R 2DROP DUP " (Code+)
    S" #Clear" (CodeName+)
    S" (FreeHeader) R> THROW ELSE DROP ROT DROP THEN " (Code+)
    S" DUP " (Code+)
    S" #Clear" (CodeName+)
    S" 2DUP (VecSwap) SWAP " (Code+)
    S" DUP @ ?DUP IF (FreeData) THEN (FreeHeader)" (Code+)
    (CodePrivateFinish) DROP
;

: (GenerateMoveRelocation) ( -- )
    S" #FillMove" (CodePrivateDefine)
    S" 2DUP 2>R DUP CELL+ @ 0 SWAP " (Code+)
    S" #AppendMoves" (CodeName+)
    S" 2R> ROT DROP" (Code+)
    (CodePrivateFinish) DROP

    S" #Relocate" (CodePrivateDefine)
    S" >R (Guard2) ['] (NewTempVector) CATCH DUP IF >R 2DROP " (Code+)
    S" (FreeHeader) R> THROW ELSE DROP SWAP DROP THEN R> " (Code+)
    S" (Guard2) ['] " (Code+)
    S" #FillMove" (CodeName+)
    S" CATCH DUP IF >R 2DROP DUP " (Code+)
    S" #Clear" (CodeName+)
    S" (FreeHeader) R> THROW ELSE DROP ROT DROP THEN " (Code+)
    S" DUP " (Code+)
    S" #Clear" (CodeName+)
    S" 2DUP (VecSwap) SWAP " (Code+)
    S" DUP @ ?DUP IF (FreeData) THEN (FreeHeader)" (Code+)
    (CodePrivateFinish) DROP
;

: (GenerateUnsupportedRelocation) ( -- )
    S" #Relocate" (CodePrivateDefine)
    S" >R >R (FreeData) R> DROP R> DROP " (Code+)
    S" (VEC-ERR-UNSUPPORTED) THROW" (Code+)
    (CodePrivateFinish) DROP
;

: (GenerateGrowthWords) ( -- )
    (VectorRelocationKind) @ (RELOCATE-RAW) =
    IF
        (GenerateRawRelocation)
    ELSE
        (VectorRelocationKind) @ (RELOCATE-COPY) =
        IF
            (GenerateCopyRelocation)
        ELSE
            (VectorRelocationKind) @ (RELOCATE-MOVE) =
            IF
                (GenerateMoveRelocation)
            ELSE
                (GenerateUnsupportedRelocation)
            THEN
        THEN
    THEN

    S" #Reallocate" (CodePrivateDefine)
    S" >R DUP " (Code+)
    S" #ElementSize" (CodeName+)
    S" #ElementAlignment" (CodeName+)
    S" (AllocateCapacityData) R> SWAP -ROT " (Code+)
    S" #Relocate" (CodeName+)
    (CodePrivateFinish) DROP

    S" #GrowTo" (CodePrivateDefine)
    S" 2DUP 2 CELLS + @ U<= IF 2DROP EXIT THEN " (Code+)
    S" #Reallocate" (CodeName+)
    (CodePrivateFinish) DROP

    S" #Reserve" (CodePrivateDefine)
    S" OVER 0< IF 2DROP (VEC-ERR-LENGTH) THROW THEN " (Code+)
    S" DUP 2 CELLS + @ 2 PICK U>= IF 2DROP EXIT THEN " (Code+)
    S" >R R@ CELL+ @ " (Code+)
    S" #ElementSize" (CodeName+)
    S" (NextCapacity) R> " (Code+)
    S" #GrowTo" (CodeName+)
    S" DROP" (Code+)
    (CodePrivateFinish)
    S" .Reserve" ROT (GenerateSimple)

    S" #EnsureOne" (CodePrivateDefine)
    S" DUP CELL+ @ 1 (CheckedAdd) OVER 2 CELLS + @ U> IF " (Code+)
    S" DUP CELL+ @ 1 (CheckedAdd) OVER 2 CELLS + @ " (Code+)
    S" #ElementSize" (CodeName+)
    S" (NextCapacity) SWAP " (Code+)
    S" #GrowTo" (CodeName+)
    S" THEN" (Code+)
    (CodePrivateFinish) DROP
;

: (GeneratePushWords) ( -- )
    S" #PushBack" (CodePrivateDefine)
    S" 2DUP " (Code+)
    S" #ElementSize" (CodeName+)
    S" (SourceIndex) SWAP " (Code+)
    S" #EnsureOne" (CodeName+)
    S" DUP >R " (Code+)
    S" #ElementSize" (CodeName+)
    S" (RefreshSource) R> DUP " (Code+)
    S" #ElementSize" (CodeName+)
    S" (VecEndAddress) ROT SWAP " (Code+)
    S" #CopyOne" (CodeName+)
    S" (IncrementSize) DROP" (Code+)
    (CodePrivateFinish)
    S" .PushBack" ROT (GenerateSimple)

    S" #PushBackMove" (CodePrivateDefine)
    S" 2DUP " (Code+)
    S" #ElementSize" (CodeName+)
    S" (SourceIndex) SWAP " (Code+)
    S" #EnsureOne" (CodeName+)
    S" DUP >R " (Code+)
    S" #ElementSize" (CodeName+)
    S" (RefreshSource) R> DUP " (Code+)
    S" #ElementSize" (CodeName+)
    S" (VecEndAddress) ROT SWAP " (Code+)
    S" #MoveOne" (CodeName+)
    S" (IncrementSize) DROP" (Code+)
    (CodePrivateFinish)
    S" .PushBackMove" ROT (GenerateSimple)

    S" #EmplaceBack" (CodePrivateDefine)
    S" DUP >R " (Code+)
    S" #EnsureOne" (CodeName+)
    S" R> DROP DUP " (Code+)
    S" #ElementSize" (CodeName+)
    S" (VecEndAddress) >R R@ " (Code+)
    S" #DefaultOne" (CodeName+)
    S" (IncrementSize) R> SWAP DROP" (Code+)
    (CodePrivateFinish)
    S" .EmplaceBack" ROT (GenerateSimple)

    S" #Resize" (CodePrivateDefine)
    S" OVER 0< IF 2DROP (VEC-ERR-LENGTH) THROW THEN " (Code+)
    S" DUP CELL+ @ 2 PICK SWAP < IF SWAP " (Code+)
    S" #DestroyRange" (CodeName+)
    S" DROP ELSE DUP CELL+ @ 2 PICK < IF " (Code+)
    S" DUP CELL+ @ >R DUP >R OVER SWAP " (Code+)
    S" #Reserve" (CodeName+)
    S" R> R> ROT SWAP - " (Code+)
    S" #AppendDefaults" (CodeName+)
    S" DROP ELSE 2DROP THEN THEN" (Code+)
    (CodePrivateFinish)
    S" .Resize" ROT (GenerateSimple)

    S" #ShrinkToFit" (CodePrivateDefine)
    S" DUP CELL+ @ OVER 2 CELLS + @ 2DUP U>= IF 2DROP DROP EXIT THEN " (Code+)
    S" DROP DUP 0= IF DROP DUP @ ?DUP IF (FreeData) THEN 0 OVER ! " (Code+)
    S" 0 OVER 2 CELLS + ! DROP ELSE SWAP " (Code+)
    S" #Reallocate" (CodeName+)
    S" DROP THEN" (Code+)
    (CodePrivateFinish)
    S" .ShrinkToFit" ROT (GenerateSimple)

    S" #Insert" (CodePrivateDefine)
    S" DUP CELL+ @ 2 PICK U>= 0= IF 2DROP DROP " (Code+)
    S" (VEC-ERR-INDEX) THROW THEN " (Code+)
    S" >R >R " (Code+)
    S" #ElementSize" (CodeName+)
    S" #ElementAlignment" (CodeName+)
    S" (AllocateData) >R R@ SWAP (Guard2) SWAP ['] " (Code+)
    S" #CopyOne" (CodeName+)
    S" CATCH DUP IF >R 2DROP (FreeData) R> THROW ELSE DROP DROP THEN " (Code+)
    S" R> R> R> " (Code+)
    S" #EnsureOne" (CodeName+)
    S" DUP CELL+ @ 2 PICK - 0 ?DO " (Code+)
    S" DUP CELL+ @ 1- I - DUP 1+ 2 PICK " (Code+)
    S" #MoveIndexed" (CodeName+)
    S" LOOP " (Code+)
    S" DUP CELL+ @ 2 PICK SWAP < IF 2DUP " (Code+)
    S" #ElementSize" (CodeName+)
    S" (VecIndex) " (Code+)
    S" #DestroyOne" (CodeName+)
    S" THEN 2DUP " (Code+)
    S" #ElementSize" (CodeName+)
    S" (VecIndex) 3 PICK SWAP " (Code+)
    S" #CopyOne" (CodeName+)
    S" 1 OVER CELL+ +! " (Code+)
    S" ROT DUP " (Code+)
    S" #DestroyOne" (CodeName+)
    S" (FreeData) 2DROP" (Code+)
    (CodePrivateFinish)
    S" .Insert" ROT (GenerateSimple)

    S" #AppendSlice" (CodePrivateDefine)
    S" OVER 0< IF 2DROP DROP (VEC-ERR-LENGTH) THROW THEN " (Code+)
    S" SWAP >R " (Code+)
    S" 2DUP " (Code+)
    S" #ElementSize" (CodeName+)
    S" (SourceIndex) R> ROT " (Code+)
    S" 2 PICK 0>= IF " (Code+)
    S" OVER 3 PICK (CheckedAdd) 1 PICK CELL+ @ 2DUP U<= IF " (Code+)
    S" 2DROP ELSE 2DROP 2DROP (VEC-ERR-INDEX) THROW THEN " (Code+)
    S" THEN >R R@ CELL+ @ OVER (CheckedAdd) R@ " (Code+)
    S" #Reserve" (CodeName+)
    S" R> SWAP >R DUP >R " (Code+)
    S" #ElementSize" (CodeName+)
    S" (RefreshSource) R> R> SWAP " (Code+)
    S" #AppendPointer" (CodeName+)
    (CodePrivateFinish)
    S" .AppendSlice" ROT (GenerateSimple)
;

: (GenerateBasicAPI) ( -- )
    S" .New" ['] (VecNew) (GenerateSimple)
    (GenerateNewWithCapacity)
    S" .Move" ['] (VecMoveHandle) (GenerateSimple)
    S" .Size" ['] (VecSize) (GenerateSimple)
    S" .Capacity" ['] (VecCapacity) (GenerateSimple)
    S" .Empty" ['] (VecEmpty) (GenerateSimple)
    S" .Data" ['] (VecData) (GenerateSimple)
    S" .Swap" ['] (VecSwap) (GenerateSimple)
    S" .Index" ['] (VecIndex) (GenerateValueWithSize)
    S" .At" ['] (VecAt) (GenerateValueWithSize)
    S" .Front" ['] (VecFront) (GenerateValueWithSize)
    S" .Back" ['] (VecBack) (GenerateValueWithSize)
;

: (GenerateVectorIteratorWords) ( -- )
    0 (VectorIteratorWriteXt) !
    0 (VectorIteratorSwapXt) !

    S" #IteratorNext" (CodePrivateDefine)
    S" #ElementSize" (CodeName+)
    S" +" (Code+)
    (CodePrivateFinish) (VectorIteratorNextXt) !

    S" #IteratorPrev" (CodePrivateDefine)
    S" #ElementSize" (CodeName+)
    S" -" (Code+)
    (CodePrivateFinish) (VectorIteratorPrevXt) !

    S" #IteratorAdvance" (CodePrivateDefine)
    S" #ElementSize" (CodeName+)
    S" * +" (Code+)
    (CodePrivateFinish) (VectorIteratorAdvanceXt) !

    S" #IteratorDistance" (CodePrivateDefine)
    S" SWAP - " (Code+)
    S" #ElementSize" (CodeName+)
    S" /" (Code+)
    (CodePrivateFinish) (VectorIteratorDistanceXt) !

    S" #IteratorLess" (CodePrivateDefine)
    S" U<" (Code+)
    (CodePrivateFinish) (VectorIteratorLessXt) !

    S" #IteratorRead" (CodePrivateDefine)
    (CodePrivateFinish) (VectorIteratorReadXt) !

    S" #IteratorEqual" (CodePrivateDefine)
    S" =" (Code+)
    (CodePrivateFinish) (VectorIteratorEqualXt) !

    S" #IteratorBegin" (CodePrivateDefine)
    S" (VecData)" (Code+)
    (CodePrivateFinish) (VectorIteratorBeginXt) !

    S" #IteratorEnd" (CodePrivateDefine)
    S" #ElementSize" (CodeName+)
    S" (VecEndAddress)" (Code+)
    (CodePrivateFinish) (VectorIteratorEndXt) !

    (VectorCopyAssignSupported) @
    IF
        S" #IteratorWrite" (CodePrivateDefine)
        S" #CopyAssignOne" (CodeName+)
        (CodePrivateFinish) (VectorIteratorWriteXt) !
    THEN

    (VectorType) @ ElementTypeRawCopyable?
    (VectorDestroyXt) @ 0= AND
    (VectorType) @ ElementTypeSize CELL MOD 0= AND
    IF
        S" #IteratorSwap" (CodePrivateDefine)
        S" #ElementSize" (CodeName+)
        S" CELL / 0 ?DO 2DUP (SwapCell) CELL+ SWAP CELL+ SWAP LOOP 2DROP" (Code+)
        (CodePrivateFinish) (VectorIteratorSwapXt) !
    THEN
;

: (GenerateVectorIterator) ( -- )
    (GenerateVectorIteratorWords)
    ContiguousIteratorCategory
    S" Iterator" (VectorName)
    BeginIteratorTypeNamed
        (VectorIteratorNextXt) @ UseIteratorNext
        (VectorIteratorPrevXt) @ UseIteratorPrev
        (VectorIteratorAdvanceXt) @ UseIteratorAdvance
        (VectorIteratorDistanceXt) @ UseIteratorDistance
        (VectorIteratorLessXt) @ UseIteratorLess
        (VectorIteratorReadXt) @ UseIteratorRead
        (VectorIteratorEqualXt) @ UseIteratorEqual
        (VectorIteratorEqualXt) @ UseIteratorSentinelEqual
        (VectorIteratorBeginXt) @ UseIteratorBegin
        (VectorIteratorEndXt) @ UseIteratorEnd
        (VectorCopyAssignSupported) @
        IF
            (VectorIteratorWriteXt) @ UseIteratorWrite
        THEN
        (VectorIteratorSwapXt) @ ?DUP
        IF
            UseIteratorSwap
        THEN
    EndIteratorType
    S" .Begin" (VectorIteratorBeginXt) @ (GenerateSimple)
    S" .End" (VectorIteratorEndXt) @ (GenerateSimple)
;

: (VectorRequiredName) ( "name" -- c-addr u )
    PARSE-NAME
    DUP 0=
    IF
        S" Vector: a name is required" Fail
    THEN
;

Export : DefineVectorType ( "name" "element-type-name" -- )
    (VectorRequiredName)
    DUP (VECTOR-NAME-BYTES) >
    IF
        2DROP S" Vector: vector type name is too long" Fail
    THEN
    DUP (VectorNameLen) !
    (VectorNameBuffer) SWAP MOVE

    (VectorRequiredName)
    FIND-NAME ?DUP
    IF
        NAME>INT EXECUTE
    ELSE
        S" Vector: element type was not found" Fail
    THEN
    DUP (VectorType) !
    ElementTypeSize 0<=
    IF
        ElementTypeError THROW
    THEN

    (GenerateAdapters)
    (GenerateVectorRangeWords)
    (GenerateCoreLifecycleWords)
    (GenerateGrowthWords)
    (GeneratePushWords)
    (GenerateBasicAPI)
    (GenerateVectorIterator)
;

EndModule
EndImport