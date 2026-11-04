REQUIRE iterator.fs

Import Iterator
VARIABLE (AlgorithmImportCount)
(AlgorithmImportCount) !

BeginModule Algorithm

Export Import Iterator

VARIABLE (AlgorithmPrivateWid)
VARIABLE (AlgorithmPublicWid)
VARIABLE (AlgorithmModuleBase)

GET-CURRENT (AlgorithmPrivateWid) !
2DUP
(AlgorithmModuleBase) !
(AlgorithmPublicWid) !

S" Algorithm: operation unsupported by iterator context" EXCEPTION CONSTANT (ALG-ERR-OP)
S" Algorithm: iterator category requirement not met" EXCEPTION CONSTANT (ALG-ERR-CATEGORY)

Export : AlgorithmOperationError ( -- n )
    (ALG-ERR-OP)
;

Export : AlgorithmCategoryError ( -- n )
    (ALG-ERR-CATEGORY)
;

0 CONSTANT (OPS-CATEGORY)
CELL CONSTANT (OPS-NEXT-XT)
2 CELLS CONSTANT (OPS-PREV-XT)
3 CELLS CONSTANT (OPS-ADVANCE-XT)
4 CELLS CONSTANT (OPS-DISTANCE-XT)
5 CELLS CONSTANT (OPS-EQUAL-XT)
6 CELLS CONSTANT (OPS-SENTINEL-EQUAL-XT)
7 CELLS CONSTANT (OPS-LESS-XT)
8 CELLS CONSTANT (OPS-READ-XT)
9 CELLS CONSTANT (OPS-WRITE-XT)
10 CELLS CONSTANT (OPS-SWAP-XT)
11 CELLS CONSTANT (OPS-DESCRIPTOR-BYTES)

VARIABLE (AlgOpsA)
VARIABLE (AlgOpsB)
VARIABLE (AlgOpsOut)
VARIABLE (AlgOpsBuild)
VARIABLE (AlgTypeBuild)

VARIABLE (AlgPredicateXt)
VARIABLE (AlgTransformXt)
VARIABLE (AlgComparatorXt)
VARIABLE (AlgBinaryXt)
VARIABLE (AlgGeneratorXt)
VARIABLE (AlgProjectionXt)

VARIABLE (AlgTempNameAddr)
VARIABLE (AlgTempNameLen)
VARIABLE (AlgTempEmitXt)
VARIABLE (AlgorithmHelperSerial)

256 CONSTANT (ALGORITHM-NAME-BYTES)
VARIABLE (AlgorithmNameLen)
CREATE (AlgorithmNameBuffer) (ALGORITHM-NAME-BYTES) ALLOT

65536 CONSTANT (ALGORITHM-CODE-BYTES)
VARIABLE (AlgorithmCodeLen)
CREATE (AlgorithmCodeBuffer) (ALGORITHM-CODE-BYTES) ALLOT

: (AlgorithmMissing) ( ... -- )
    (ALG-ERR-OP) THROW
;

: (Identity) ( x -- x )
;

DEFER (ALG-A-NEXT)
DEFER (ALG-A-PREV)
DEFER (ALG-A-ADVANCE)
DEFER (ALG-A-DISTANCE)
DEFER (ALG-A-EQUAL)
DEFER (ALG-A-SENTINEL)
DEFER (ALG-A-LESS)
DEFER (ALG-A-READ)
DEFER (ALG-A-WRITE)
DEFER (ALG-A-SWAP)

DEFER (ALG-B-NEXT)
DEFER (ALG-B-PREV)
DEFER (ALG-B-ADVANCE)
DEFER (ALG-B-DISTANCE)
DEFER (ALG-B-EQUAL)
DEFER (ALG-B-SENTINEL)
DEFER (ALG-B-LESS)
DEFER (ALG-B-READ)
DEFER (ALG-B-WRITE)
DEFER (ALG-B-SWAP)

DEFER (ALG-O-NEXT)
DEFER (ALG-O-PREV)
DEFER (ALG-O-ADVANCE)
DEFER (ALG-O-DISTANCE)
DEFER (ALG-O-EQUAL)
DEFER (ALG-O-SENTINEL)
DEFER (ALG-O-LESS)
DEFER (ALG-O-READ)
DEFER (ALG-O-WRITE)
DEFER (ALG-O-SWAP)

DEFER (ALG-PRED)
DEFER (ALG-TRANS)
DEFER (ALG-CMP)
DEFER (ALG-BIN)
DEFER (ALG-GEN)
DEFER (ALG-PROJ)
DEFER (ALG-HELPER)
DEFER (ALG-HELPER2)

: A-NEXT ['] (ALG-A-NEXT) DEFER@ COMPILE, ; IMMEDIATE
: A-PREV ['] (ALG-A-PREV) DEFER@ COMPILE, ; IMMEDIATE
: A-ADVANCE ['] (ALG-A-ADVANCE) DEFER@ COMPILE, ; IMMEDIATE
: A-DISTANCE ['] (ALG-A-DISTANCE) DEFER@ COMPILE, ; IMMEDIATE
: A-EQUAL ['] (ALG-A-EQUAL) DEFER@ COMPILE, ; IMMEDIATE
: A-SENTINEL ['] (ALG-A-SENTINEL) DEFER@ COMPILE, ; IMMEDIATE
: A-LESS ['] (ALG-A-LESS) DEFER@ COMPILE, ; IMMEDIATE
: A-READ ['] (ALG-A-READ) DEFER@ COMPILE, ; IMMEDIATE
: A-WRITE ['] (ALG-A-WRITE) DEFER@ COMPILE, ; IMMEDIATE
: A-SWAP ['] (ALG-A-SWAP) DEFER@ COMPILE, ; IMMEDIATE

: B-NEXT ['] (ALG-B-NEXT) DEFER@ COMPILE, ; IMMEDIATE
: B-PREV ['] (ALG-B-PREV) DEFER@ COMPILE, ; IMMEDIATE
: B-ADVANCE ['] (ALG-B-ADVANCE) DEFER@ COMPILE, ; IMMEDIATE
: B-DISTANCE ['] (ALG-B-DISTANCE) DEFER@ COMPILE, ; IMMEDIATE
: B-EQUAL ['] (ALG-B-EQUAL) DEFER@ COMPILE, ; IMMEDIATE
: B-SENTINEL ['] (ALG-B-SENTINEL) DEFER@ COMPILE, ; IMMEDIATE
: B-LESS ['] (ALG-B-LESS) DEFER@ COMPILE, ; IMMEDIATE
: B-READ ['] (ALG-B-READ) DEFER@ COMPILE, ; IMMEDIATE
: B-WRITE ['] (ALG-B-WRITE) DEFER@ COMPILE, ; IMMEDIATE
: B-SWAP ['] (ALG-B-SWAP) DEFER@ COMPILE, ; IMMEDIATE

: O-NEXT ['] (ALG-O-NEXT) DEFER@ COMPILE, ; IMMEDIATE
: O-PREV ['] (ALG-O-PREV) DEFER@ COMPILE, ; IMMEDIATE
: O-ADVANCE ['] (ALG-O-ADVANCE) DEFER@ COMPILE, ; IMMEDIATE
: O-DISTANCE ['] (ALG-O-DISTANCE) DEFER@ COMPILE, ; IMMEDIATE
: O-EQUAL ['] (ALG-O-EQUAL) DEFER@ COMPILE, ; IMMEDIATE
: O-SENTINEL ['] (ALG-O-SENTINEL) DEFER@ COMPILE, ; IMMEDIATE
: O-LESS ['] (ALG-O-LESS) DEFER@ COMPILE, ; IMMEDIATE
: O-READ ['] (ALG-O-READ) DEFER@ COMPILE, ; IMMEDIATE
: O-WRITE ['] (ALG-O-WRITE) DEFER@ COMPILE, ; IMMEDIATE
: O-SWAP ['] (ALG-O-SWAP) DEFER@ COMPILE, ; IMMEDIATE

: PRED ['] (ALG-PRED) DEFER@ COMPILE, ; IMMEDIATE
: TRANS ['] (ALG-TRANS) DEFER@ COMPILE, ; IMMEDIATE
: CMP ['] (ALG-CMP) DEFER@ COMPILE, ; IMMEDIATE
: BIN ['] (ALG-BIN) DEFER@ COMPILE, ; IMMEDIATE
: GEN ['] (ALG-GEN) DEFER@ COMPILE, ; IMMEDIATE
: PROJ ['] (ALG-PROJ) DEFER@ COMPILE, ; IMMEDIATE
: HELPER ['] (ALG-HELPER) DEFER@ COMPILE, ; IMMEDIATE
: HELPER2 ['] (ALG-HELPER2) DEFER@ COMPILE, ; IMMEDIATE

: (SetMissing) ( defer-xt -- )
    ['] (AlgorithmMissing) SWAP DEFER!
;

' (ALG-A-NEXT) (SetMissing)
' (ALG-A-PREV) (SetMissing)
' (ALG-A-ADVANCE) (SetMissing)
' (ALG-A-DISTANCE) (SetMissing)
' (ALG-A-EQUAL) (SetMissing)
' (ALG-A-SENTINEL) (SetMissing)
' (ALG-A-LESS) (SetMissing)
' (ALG-A-READ) (SetMissing)
' (ALG-A-WRITE) (SetMissing)
' (ALG-A-SWAP) (SetMissing)
' (ALG-B-NEXT) (SetMissing)
' (ALG-B-PREV) (SetMissing)
' (ALG-B-ADVANCE) (SetMissing)
' (ALG-B-DISTANCE) (SetMissing)
' (ALG-B-EQUAL) (SetMissing)
' (ALG-B-SENTINEL) (SetMissing)
' (ALG-B-LESS) (SetMissing)
' (ALG-B-READ) (SetMissing)
' (ALG-B-WRITE) (SetMissing)
' (ALG-B-SWAP) (SetMissing)
' (ALG-O-NEXT) (SetMissing)
' (ALG-O-PREV) (SetMissing)
' (ALG-O-ADVANCE) (SetMissing)
' (ALG-O-DISTANCE) (SetMissing)
' (ALG-O-EQUAL) (SetMissing)
' (ALG-O-SENTINEL) (SetMissing)
' (ALG-O-LESS) (SetMissing)
' (ALG-O-READ) (SetMissing)
' (ALG-O-WRITE) (SetMissing)
' (ALG-O-SWAP) (SetMissing)
' (ALG-PRED) (SetMissing)
' (ALG-TRANS) (SetMissing)
' (ALG-CMP) (SetMissing)
' (ALG-BIN) (SetMissing)
' (ALG-GEN) (SetMissing)
' (ALG-PROJ) (SetMissing)
' (ALG-HELPER) (SetMissing)
' (ALG-HELPER2) (SetMissing)

: (OrDefault) ( xt -- xt' )
    ?DUP
    IF
        EXIT
    THEN
    ['] (AlgorithmMissing)
;

: (RequiredName) ( "name" -- c-addr u )
    PARSE-NAME
    DUP 0=
    IF
        S" Algorithm: a name is required" Fail
    THEN
;

: (CreateNamed) ( c-addr u -- body-addr )
    NEXTNAME
    CREATE
    HERE
;

: (OpsField) ( ops offset -- addr )
    +
;

: (CopyOpsXt) ( source-ops source-offset dest-ops dest-offset -- )
    >R
    SWAP
    +
    @
    R>
    +
    !
;

: (IteratorLookup) ( c-addr u -- iterator-type )
    FIND-NAME ?DUP 0=
    IF
        S" Algorithm: iterator type was not found" Fail
    THEN
    NAME>INT EXECUTE
;

Export : DefineAlgorithmOps ( "ops-name" "iterator-type-name" -- )
    (RequiredName)
    DUP (ALGORITHM-NAME-BYTES) >
    IF
        2DROP S" Algorithm: context name is too long" Fail
    THEN
    DUP (AlgorithmNameLen) !
    2DUP
    (AlgorithmNameBuffer) SWAP MOVE
    (CreateNamed)
    (OPS-DESCRIPTOR-BYTES) ALLOT
    (AlgOpsBuild) !

    (RequiredName)
    (IteratorLookup)
    (AlgTypeBuild) !
    (AlgTypeBuild) @ IteratorTypeCategory
    (AlgOpsBuild) @ (OPS-CATEGORY) + !
    (AlgTypeBuild) @ IteratorTypeNextXt (OrDefault)
    (AlgOpsBuild) @ (OPS-NEXT-XT) + !
    (AlgTypeBuild) @ IteratorTypePrevXt (OrDefault)
    (AlgOpsBuild) @ (OPS-PREV-XT) + !
    (AlgTypeBuild) @ IteratorTypeAdvanceXt (OrDefault)
    (AlgOpsBuild) @ (OPS-ADVANCE-XT) + !
    (AlgTypeBuild) @ IteratorTypeDistanceXt (OrDefault)
    (AlgOpsBuild) @ (OPS-DISTANCE-XT) + !
    (AlgTypeBuild) @ IteratorTypeEqualXt (OrDefault)
    (AlgOpsBuild) @ (OPS-EQUAL-XT) + !
    (AlgTypeBuild) @ IteratorTypeSentinelEqualXt (OrDefault)
    (AlgOpsBuild) @ (OPS-SENTINEL-EQUAL-XT) + !
    (AlgTypeBuild) @ IteratorTypeLessXt (OrDefault)
    (AlgOpsBuild) @ (OPS-LESS-XT) + !
    (AlgTypeBuild) @ IteratorTypeReadXt (OrDefault)
    (AlgOpsBuild) @ (OPS-READ-XT) + !
    (AlgTypeBuild) @ IteratorTypeWriteXt (OrDefault)
    (AlgOpsBuild) @ (OPS-WRITE-XT) + !
    (AlgTypeBuild) @ IteratorTypeSwapXt (OrDefault)
    (AlgOpsBuild) @ (OPS-SWAP-XT) + !
;

: (BindDeferred) ( xt deferred-xt -- )
    (OrDefault)
    DEFER!
;

: (BindField) ( ops offset deferred-xt -- )
    >R
    + @
    R>
    (BindDeferred)
;

: (BindContextA) ( ops -- )
    (AlgOpsA) !
    (AlgOpsA) @ (OPS-NEXT-XT) ['] (ALG-A-NEXT) (BindField)
    (AlgOpsA) @ (OPS-PREV-XT) ['] (ALG-A-PREV) (BindField)
    (AlgOpsA) @ (OPS-ADVANCE-XT) ['] (ALG-A-ADVANCE) (BindField)
    (AlgOpsA) @ (OPS-DISTANCE-XT) ['] (ALG-A-DISTANCE) (BindField)
    (AlgOpsA) @ (OPS-EQUAL-XT) ['] (ALG-A-EQUAL) (BindField)
    (AlgOpsA) @ (OPS-SENTINEL-EQUAL-XT) ['] (ALG-A-SENTINEL) (BindField)
    (AlgOpsA) @ (OPS-LESS-XT) ['] (ALG-A-LESS) (BindField)
    (AlgOpsA) @ (OPS-READ-XT) ['] (ALG-A-READ) (BindField)
    (AlgOpsA) @ (OPS-WRITE-XT) ['] (ALG-A-WRITE) (BindField)
    (AlgOpsA) @ (OPS-SWAP-XT) ['] (ALG-A-SWAP) (BindField)
;

: (BindContextB) ( ops -- )
    (AlgOpsB) !
    (AlgOpsB) @ (OPS-NEXT-XT) ['] (ALG-B-NEXT) (BindField)
    (AlgOpsB) @ (OPS-PREV-XT) ['] (ALG-B-PREV) (BindField)
    (AlgOpsB) @ (OPS-ADVANCE-XT) ['] (ALG-B-ADVANCE) (BindField)
    (AlgOpsB) @ (OPS-DISTANCE-XT) ['] (ALG-B-DISTANCE) (BindField)
    (AlgOpsB) @ (OPS-EQUAL-XT) ['] (ALG-B-EQUAL) (BindField)
    (AlgOpsB) @ (OPS-SENTINEL-EQUAL-XT) ['] (ALG-B-SENTINEL) (BindField)
    (AlgOpsB) @ (OPS-LESS-XT) ['] (ALG-B-LESS) (BindField)
    (AlgOpsB) @ (OPS-READ-XT) ['] (ALG-B-READ) (BindField)
    (AlgOpsB) @ (OPS-WRITE-XT) ['] (ALG-B-WRITE) (BindField)
    (AlgOpsB) @ (OPS-SWAP-XT) ['] (ALG-B-SWAP) (BindField)
;

: (BindContextOut) ( ops -- )
    (AlgOpsOut) !
    (AlgOpsOut) @ (OPS-NEXT-XT) ['] (ALG-O-NEXT) (BindField)
    (AlgOpsOut) @ (OPS-PREV-XT) ['] (ALG-O-PREV) (BindField)
    (AlgOpsOut) @ (OPS-ADVANCE-XT) ['] (ALG-O-ADVANCE) (BindField)
    (AlgOpsOut) @ (OPS-DISTANCE-XT) ['] (ALG-O-DISTANCE) (BindField)
    (AlgOpsOut) @ (OPS-EQUAL-XT) ['] (ALG-O-EQUAL) (BindField)
    (AlgOpsOut) @ (OPS-SENTINEL-EQUAL-XT) ['] (ALG-O-SENTINEL) (BindField)
    (AlgOpsOut) @ (OPS-LESS-XT) ['] (ALG-O-LESS) (BindField)
    (AlgOpsOut) @ (OPS-READ-XT) ['] (ALG-O-READ) (BindField)
    (AlgOpsOut) @ (OPS-WRITE-XT) ['] (ALG-O-WRITE) (BindField)
    (AlgOpsOut) @ (OPS-SWAP-XT) ['] (ALG-O-SWAP) (BindField)
;

: (BindUnary) ( xt -- )
    DUP 0=
    IF
        DROP S" Algorithm: predicate is required" Fail
    THEN
    (AlgPredicateXt) !
    (AlgPredicateXt) @ ['] (ALG-PRED) (BindDeferred)
;

: (BindTransform) ( xt -- )
    DUP 0=
    IF
        DROP S" Algorithm: transform is required" Fail
    THEN
    (AlgTransformXt) !
    (AlgTransformXt) @ ['] (ALG-TRANS) (BindDeferred)
;

: (BindBinary) ( xt -- )
    DUP 0=
    IF
        DROP S" Algorithm: binary operation is required" Fail
    THEN
    (AlgBinaryXt) !
    (AlgBinaryXt) @ ['] (ALG-BIN) (BindDeferred)
;

: (BindGenerator) ( xt -- )
    DUP 0=
    IF
        DROP S" Algorithm: generator is required" Fail
    THEN
    (AlgGeneratorXt) !
    (AlgGeneratorXt) @ ['] (ALG-GEN) (BindDeferred)
;

: (BindComparator) ( xt -- )
    DUP 0=
    IF
        DROP S" Algorithm: comparator is required" Fail
    THEN
    (AlgComparatorXt) !
    (AlgComparatorXt) @ ['] (ALG-CMP) (BindDeferred)
;

: (BindProjection) ( xt -- )
    (AlgProjectionXt) !
    (AlgProjectionXt) @ ?DUP
    IF
        ['] (ALG-PROJ) (BindDeferred)
    ELSE
        ['] (Identity) ['] (ALG-PROJ) (BindDeferred)
    THEN
;

: (ParseAlgorithmName) ( "name" -- )
    (RequiredName)
    DUP (ALGORITHM-NAME-BYTES) >
    IF
        2DROP S" Algorithm: generated name is too long" Fail
    THEN
    DUP (AlgorithmNameLen) !
    (AlgorithmNameBuffer) SWAP MOVE
;

: (CodeReset) ( -- )
    0 (AlgorithmCodeLen) !
;

: (Code+) ( c-addr u -- )
    DUP (AlgorithmCodeLen) @ + (ALGORITHM-CODE-BYTES) >
    IF
        2DROP S" Algorithm: generated algorithm is too long" Fail
    THEN
    DUP >R
    (AlgorithmCodeBuffer) (AlgorithmCodeLen) @ +
    SWAP MOVE
    R> (AlgorithmCodeLen) +!
;

: (EvaluatePublicCode) ( -- xt )
    GET-CURRENT >R
    (AlgorithmPublicWid) @ SET-CURRENT
    GET-ORDER 1+
    (AlgorithmPrivateWid) @
    SWAP SET-ORDER
    (AlgorithmCodeBuffer) (AlgorithmCodeLen) @ EVALUATE
    LATEST NAME>INT
    GET-ORDER 1- >R DROP R> SET-ORDER
    R> SET-CURRENT
;

: (EvaluatePrivateCode) ( -- xt )
    GET-CURRENT >R
    (AlgorithmPrivateWid) @ SET-CURRENT
    GET-ORDER 1+
    (AlgorithmPrivateWid) @
    SWAP SET-ORDER
    (AlgorithmCodeBuffer) (AlgorithmCodeLen) @ EVALUATE
    LATEST NAME>INT
    GET-ORDER 1- >R DROP R> SET-ORDER
    R> SET-CURRENT
;

: (CodeUnsigned) ( u -- )
    0 <# #S #> (Code+)
;

: (EmitHelper) ( body-addr body-u -- )
    (CodeReset)
    S" : (ALG-HELPER-" (Code+)
    (AlgorithmHelperSerial) @ (CodeUnsigned)
    S" )  " (Code+)
    (Code+)
    S"  ;" (Code+)
    (EvaluatePrivateCode)
    ['] (ALG-HELPER) (BindDeferred)
    1 (AlgorithmHelperSerial) +!
;

: (EmitHelper2) ( body-addr body-u -- )
    (CodeReset)
    S" : (ALG-HELPER2-" (Code+)
    (AlgorithmHelperSerial) @ (CodeUnsigned)
    S" )  " (Code+)
    (Code+)
    S"  ;" (Code+)
    (EvaluatePrivateCode)
    ['] (ALG-HELPER2) (BindDeferred)
    1 (AlgorithmHelperSerial) +!
;

: (NeedOp) ( xt c-addr u -- )
    ROT
    DUP ['] (AlgorithmMissing) =
    IF
        DROP
        Fail
    ELSE
        DROP 2DROP
    THEN
;

: (OpsFieldXt) ( ops offset -- xt )
    + @
;

: (RequireA) ( offset c-addr u -- )
    >R >R
    (AlgOpsA) @
    SWAP
    +
    @
    R>
    R> (NeedOp)
;

: (RequireB) ( offset c-addr u -- )
    >R >R
    (AlgOpsB) @
    SWAP
    +
    @
    R>
    R> (NeedOp)
;

: (RequireOut) ( offset c-addr u -- )
    >R >R
    (AlgOpsOut) @
    SWAP
    +
    @
    R>
    R> (NeedOp)
;

: (RequireCallback) ( xt c-addr u -- )
    ROT ?DUP
    IF
        DROP 2DROP
    ELSE
        DROP
        Fail
    THEN
;

: (RequireCategory) ( ops min -- )
    >R
    (OPS-CATEGORY) + @
    R@ <
    IF
        (ALG-ERR-CATEGORY) THROW
    THEN
    R> DROP
;

: (RequireACategory) ( min -- )
    >R (AlgOpsA) @ R> (RequireCategory)
;

: (RequireBCategory) ( min -- )
    >R (AlgOpsB) @ R> (RequireCategory)
;

: (RequireOutCategory) ( min -- )
    >R (AlgOpsOut) @ R> (RequireCategory)
;

: (RequireAAtLeast) ( category -- )
    (AlgOpsA) @ (OPS-CATEGORY) + @
    OVER >= 0=
    IF
        (ALG-ERR-CATEGORY) THROW
    THEN
    DROP
;

: (CategoryAtLeast) ( category ops min -- )
    >R
    (OPS-CATEGORY) + @
    R> >= 0=
    IF
        (ALG-ERR-CATEGORY) THROW
    THEN
;

Export : AlgorithmOpsCategory ( ops -- category )
    (OPS-CATEGORY) + @
;

Export : AlgorithmOpsNextXt ( ops -- xt )
    (OPS-NEXT-XT) + @
;

Export : AlgorithmOpsPrevXt ( ops -- xt )
    (OPS-PREV-XT) + @
;

Export : AlgorithmOpsAdvanceXt ( ops -- xt )
    (OPS-ADVANCE-XT) + @
;

Export : AlgorithmOpsDistanceXt ( ops -- xt )
    (OPS-DISTANCE-XT) + @
;

Export : AlgorithmOpsEqualXt ( ops -- xt )
    (OPS-EQUAL-XT) + @
;

Export : AlgorithmOpsReadXt ( ops -- xt )
    (OPS-READ-XT) + @
;

Export : AlgorithmOpsWriteXt ( ops -- xt )
    (OPS-WRITE-XT) + @
;

Export : AlgorithmOpsSwapXt ( ops -- xt )
    (OPS-SWAP-XT) + @
;


Export : AlgorithmOpsLessXt ( ops -- xt )
    (OPS-LESS-XT) + @
;

Export : AlgorithmOpsSentinelEqualXt ( ops -- xt )
    (OPS-SENTINEL-EQUAL-XT) + @
;

: (ClearCallbacks) ( -- )
    0 (AlgPredicateXt) !
    0 (AlgTransformXt) !
    0 (AlgComparatorXt) !
    0 (AlgBinaryXt) !
    0 (AlgGeneratorXt) !
    0 (AlgProjectionXt) !
    ['] (AlgorithmMissing) ['] (ALG-HELPER) (BindDeferred)
    ['] (AlgorithmMissing) ['] (ALG-HELPER2) (BindDeferred)
;

: (PrepareA) ( ops -- )
    (ClearCallbacks)
    0 (AlgOpsB) !
    0 (AlgOpsOut) !
    (BindContextA)
;

: (PrepareAB) ( ops-a ops-b -- )
    (ClearCallbacks)
    0 (AlgOpsOut) !
    (AlgOpsB) !
    (BindContextA)
    (AlgOpsB) @ (BindContextB)
;

: (PrepareAO) ( ops-a ops-out -- )
    (ClearCallbacks)
    0 (AlgOpsB) !
    (AlgOpsOut) !
    (BindContextA)
    (AlgOpsOut) @ (BindContextOut)
;

: (PrepareABO) ( ops-a ops-b ops-out -- )
    (ClearCallbacks)
    (AlgOpsOut) !
    (AlgOpsB) !
    (BindContextA)
    (AlgOpsB) @ (BindContextB)
    (AlgOpsOut) @ (BindContextOut)
;

: (FinishNamedAlgorithm) ( body-addr body-u -- )
    (CodeReset)
    S" : " (Code+)
    (AlgorithmNameBuffer) (AlgorithmNameLen) @ (Code+)
    S"  " (Code+)
    (Code+)
    S"  ;" (Code+)
    (EvaluatePublicCode)
    DROP
;

: (FinishAlgorithm) ( body-addr body-u -- )
    (ParseAlgorithmName)
    (FinishNamedAlgorithm)
;

Export : DefineAllOf ( predicate-xt input-ops "name" -- )
    SWAP >R
    (PrepareA)
    R> (BindUnary)
    S" -1 >R BEGIN 2DUP A-SENTINEL 0= WHILE DUP A-READ PRED 0= IF R> DROP 0 >R NIP DUP ELSE SWAP A-NEXT SWAP THEN REPEAT 2DROP R>"
    (FinishAlgorithm)
;

Export : DefineAnyOf ( predicate-xt input-ops "name" -- )
    SWAP >R
    (PrepareA)
    R> (BindUnary)
    S" 0 >R BEGIN 2DUP A-SENTINEL 0= WHILE DUP A-READ PRED IF R> DROP -1 >R NIP DUP ELSE SWAP A-NEXT SWAP THEN REPEAT 2DROP R>"
    (FinishAlgorithm)
;

Export : DefineNoneOf ( predicate-xt input-ops "name" -- )
    SWAP >R
    (PrepareA)
    R> (BindUnary)
    S" -1 >R BEGIN 2DUP A-SENTINEL 0= WHILE DUP A-READ PRED IF R> DROP 0 >R NIP DUP ELSE SWAP A-NEXT SWAP THEN REPEAT 2DROP R>"
    (FinishAlgorithm)
;

Export : DefineForEach ( operation-xt input-ops "name" -- )
    SWAP >R
    (PrepareA)
    R> (BindUnary)
    S" BEGIN 2DUP A-SENTINEL 0= WHILE DUP A-READ PRED DROP SWAP A-NEXT SWAP REPEAT 2DROP"
    (FinishAlgorithm)
;

Export : DefineForEachN ( operation-xt input-ops "name" -- )
    SWAP >R
    (PrepareA)
    R> (BindUnary)
    S" BEGIN DUP 0> WHILE OVER A-READ PRED DROP SWAP A-NEXT SWAP 1- REPEAT SWAP DROP"
    (FinishAlgorithm)
;

Export : DefineCount ( input-ops "name" -- )
    (PrepareA)
    S" 0 >R BEGIN 2DUP A-SENTINEL 0= WHILE SWAP A-NEXT SWAP R> 1+ >R REPEAT 2DROP R>"
    (FinishAlgorithm)
;

Export : DefineCountIf ( predicate-xt input-ops "name" -- )
    SWAP >R
    (PrepareA)
    R> (BindUnary)
    S" 0 >R BEGIN 2DUP A-SENTINEL 0= WHILE DUP A-READ PRED IF R> 1+ >R THEN SWAP A-NEXT SWAP REPEAT 2DROP R>"
    (FinishAlgorithm)
;

Export : DefineFind ( comparator-xt input-ops "name" -- )
    SWAP >R
    (PrepareA)
    R> (BindComparator)
    S" OVER >R BEGIN 2 PICK 2 PICK A-SENTINEL 0= WHILE 2 PICK A-READ OVER CMP IF R> DROP 2 PICK >R NIP OVER SWAP ELSE ROT A-NEXT -ROT THEN REPEAT DROP 2DROP R>"
    (FinishAlgorithm)
;

Export : DefineFindIf ( predicate-xt input-ops "name" -- )
    SWAP >R
    (PrepareA)
    R> (BindUnary)
    S" DUP >R BEGIN 2DUP A-SENTINEL 0= WHILE DUP A-READ PRED IF R> DROP OVER >R NIP DUP ELSE SWAP A-NEXT SWAP THEN REPEAT 2DROP R>"
    (FinishAlgorithm)
;

Export : DefineFindIfNot ( predicate-xt input-ops "name" -- )
    SWAP >R
    (PrepareA)
    R> (BindUnary)
    S" DUP >R BEGIN 2DUP A-SENTINEL 0= WHILE DUP A-READ PRED 0= IF R> DROP OVER >R NIP DUP ELSE SWAP A-NEXT SWAP THEN REPEAT 2DROP R>"
    (FinishAlgorithm)
;

Export : DefineFindFirstOf ( predicate-xt ops-a ops-b "name" -- )
    (ParseAlgorithmName)
    ROT (AlgTempEmitXt) !
    (PrepareAB)
    (AlgTempEmitXt) @ (BindUnary)
    InputIteratorCategory (RequireACategory)
    InputIteratorCategory (RequireBCategory)
    (OPS-NEXT-XT) S" input Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" input SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" input Read is required" (RequireA)
    (OPS-NEXT-XT) S" candidate Next is required" (RequireB)
    (OPS-SENTINEL-EQUAL-XT) S" candidate SentinelEqual is required" (RequireB)
    (OPS-READ-XT) S" candidate Read is required" (RequireB)
    S" >R 0 >R BEGIN 2DUP B-SENTINEL 0= WHILE OVER B-READ 2R@ DROP PRED IF R> DROP -1 >R NIP DUP ELSE SWAP B-NEXT SWAP THEN REPEAT 2DROP R> R> DROP"
    (EmitHelper)
    S" DUP >R 2>R BEGIN 2DUP A-SENTINEL 0= WHILE OVER A-READ 2R@ 2 ROLL HELPER IF DROP DUP ELSE SWAP A-NEXT SWAP THEN REPEAT 2R> 2DROP R> DROP NIP"
    (FinishNamedAlgorithm)
;

Export : DefineAdjacentFind ( comparator-xt input-ops "name" -- )
    (ParseAlgorithmName)
    SWAP (AlgTempEmitXt) !
    (PrepareA)
    (AlgTempEmitXt) @ (BindComparator)
    InputIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" input Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" input SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" input Read is required" (RequireA)
    S" DUP >R BEGIN 2DUP A-SENTINEL 0= WHILE DUP A-NEXT OVER A-SENTINEL IF NIP DROP DUP ELSE 2 PICK A-READ OVER A-READ CMP IF R> DROP 2 PICK >R NIP DROP DUP ELSE NIP SWAP THEN THEN REPEAT 2DROP R>"
    (FinishNamedAlgorithm)
;

Export : DefineSearch ( comparator-xt ops-a ops-b "name" -- )
    (ParseAlgorithmName)
    ROT (AlgTempEmitXt) !
    (PrepareAB)
    (AlgTempEmitXt) @ (BindComparator)
    InputIteratorCategory (RequireACategory)
    InputIteratorCategory (RequireBCategory)
    (OPS-NEXT-XT) S" haystack Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" haystack SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" haystack Read is required" (RequireA)
    (OPS-NEXT-XT) S" needle Next is required" (RequireB)
    (OPS-SENTINEL-EQUAL-XT) S" needle SentinelEqual is required" (RequireB)
    (OPS-READ-XT) S" needle Read is required" (RequireB)
    S" >R 0 >R BEGIN 2DUP B-SENTINEL 0= WHILE OVER B-READ 2R@ DROP CMP IF R> DROP -1 >R NIP DUP ELSE SWAP B-NEXT SWAP THEN REPEAT 2DROP R> R> DROP"
    (EmitHelper)
    S" DUP >R 2>R BEGIN 2DUP A-SENTINEL 0= WHILE OVER A-READ 2R@ 2 ROLL HELPER IF DROP DUP ELSE SWAP A-NEXT SWAP THEN REPEAT 2R> 2DROP R> DROP NIP"
    (FinishNamedAlgorithm)
;

Export : DefineEqual ( comparator-xt ops-a ops-b "name" -- )
    (ParseAlgorithmName)
    ROT (AlgTempEmitXt) !
    (PrepareAB)
    (AlgTempEmitXt) @ (BindComparator)
    InputIteratorCategory (RequireACategory)
    InputIteratorCategory (RequireBCategory)
    (OPS-NEXT-XT) S" left Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" left SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" left Read is required" (RequireA)
    (OPS-NEXT-XT) S" right Next is required" (RequireB)
    (OPS-SENTINEL-EQUAL-XT) S" right SentinelEqual is required" (RequireB)
    (OPS-READ-XT) S" right Read is required" (RequireB)
    S" >R SWAP >R 0 >R BEGIN 2DUP A-SENTINEL 0= WHILE SWAP A-NEXT SWAP R> 1+ >R REPEAT 2DROP R> R> R> 0 >R BEGIN 2DUP B-SENTINEL 0= WHILE SWAP B-NEXT SWAP R> 1+ >R REPEAT 2DROP R>"
    (EmitHelper)
    S" >R SWAP R> 2OVER 2OVER HELPER 2DUP = 0= IF 2DROP 2DROP 2DROP 0 ELSE DROP -1 SWAP 0 ?DO 3 PICK B-READ 5 PICK A-READ SWAP CMP 0= IF DROP 0 LEAVE THEN 2SWAP A-NEXT B-NEXT 2SWAP LOOP 2SWAP 2DROP 2DROP THEN"
    (FinishNamedAlgorithm)
;

Export : DefineMismatch ( comparator-xt ops-a ops-b "name" -- )
    (ParseAlgorithmName)
    ROT (AlgTempEmitXt) !
    (PrepareAB)
    (AlgTempEmitXt) @ (BindComparator)
    InputIteratorCategory (RequireACategory)
    InputIteratorCategory (RequireBCategory)
    (OPS-NEXT-XT) S" left Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" left SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" left Read is required" (RequireA)
    (OPS-NEXT-XT) S" right Next is required" (RequireB)
    (OPS-SENTINEL-EQUAL-XT) S" right SentinelEqual is required" (RequireB)
    (OPS-READ-XT) S" right Read is required" (RequireB)
    S" >R SWAP >R 0 >R BEGIN 2DUP A-SENTINEL 0= WHILE SWAP A-NEXT SWAP R> 1+ >R REPEAT 2DROP R> R> R> 0 >R BEGIN 2DUP B-SENTINEL 0= WHILE SWAP B-NEXT SWAP R> 1+ >R REPEAT 2DROP R>"
    (EmitHelper)
    S" >R SWAP R> 2OVER 2OVER HELPER MIN 0 ?DO 2 PICK B-READ 4 PICK A-READ SWAP CMP 0= IF LEAVE THEN 2SWAP A-NEXT B-NEXT 2SWAP LOOP 2DROP"
    (FinishNamedAlgorithm)
;

Export : DefineLexicographicalCompare ( comparator-xt ops-a ops-b "name" -- )
    (ParseAlgorithmName)
    ROT (AlgTempEmitXt) !
    (PrepareAB)
    (AlgTempEmitXt) @ (BindComparator)
    InputIteratorCategory (RequireACategory)
    InputIteratorCategory (RequireBCategory)
    (OPS-NEXT-XT) S" left Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" left SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" left Read is required" (RequireA)
    (OPS-NEXT-XT) S" right Next is required" (RequireB)
    (OPS-SENTINEL-EQUAL-XT) S" right SentinelEqual is required" (RequireB)
    (OPS-READ-XT) S" right Read is required" (RequireB)
    S" >R SWAP >R 0 >R BEGIN 2DUP A-SENTINEL 0= WHILE SWAP A-NEXT SWAP R> 1+ >R REPEAT 2DROP R> R> R> 0 >R BEGIN 2DUP B-SENTINEL 0= WHILE SWAP B-NEXT SWAP R> 1+ >R REPEAT 2DROP R>"
    (EmitHelper)
    S" SWAP 0 0 ?DO 2 PICK A-READ 2 PICK B-READ CMP IF DROP -1 LEAVE THEN 2 PICK B-READ 3 PICK A-READ CMP IF DROP 1 LEAVE THEN 2SWAP A-NEXT B-NEXT 2SWAP LOOP"
    (EmitHelper2)
    S" >R SWAP R> 2OVER 2OVER HELPER >R >R 2SWAP 2R@ MIN HELPER2 2DROP R> R> 2 PICK IF 2DROP ELSE ROT DROP 2DUP < IF 2DROP 1 ELSE 2DUP SWAP < IF 2DROP -1 ELSE 2DROP 0 THEN THEN THEN"
    (FinishNamedAlgorithm)
;

Export : DefineCopy ( ops-a ops-out "name" -- )
    (ParseAlgorithmName)
    (PrepareAO)
    InputIteratorCategory (RequireACategory)
    InputIteratorCategory (RequireOutCategory)
    (OPS-NEXT-XT) S" source Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" source SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" source Read is required" (RequireA)
    (OPS-NEXT-XT) S" output Next is required" (RequireOut)
    (OPS-WRITE-XT) S" output Write is required" (RequireOut)
    S" 2 PICK A-READ 1 PICK O-WRITE ROT A-NEXT -ROT O-NEXT"
    (EmitHelper)
    S" BEGIN >R 2DUP A-SENTINEL 0= R> SWAP WHILE HELPER REPEAT NIP SWAP DROP"
    (FinishNamedAlgorithm)
;

Export : DefineCopyN ( ops-a ops-out "name" -- )
    (ParseAlgorithmName)
    (PrepareAO)
    InputIteratorCategory (RequireACategory)
    InputIteratorCategory (RequireOutCategory)
    (OPS-NEXT-XT) S" source Next is required" (RequireA)
    (OPS-READ-XT) S" source Read is required" (RequireA)
    (OPS-NEXT-XT) S" output Next is required" (RequireOut)
    (OPS-WRITE-XT) S" output Write is required" (RequireOut)
    S" OVER A-READ OVER O-WRITE SWAP A-NEXT SWAP O-NEXT"
    (EmitHelper)
    S" SWAP >R BEGIN R@ 0> WHILE HELPER R> 1- >R REPEAT SWAP DROP R> DROP"
    (FinishNamedAlgorithm)
;

Export : DefineMove ( ops-a ops-out "name" -- )
    DefineCopy
;

Export : DefineMoveN ( ops-a ops-out "name" -- )
    DefineCopyN
;

Export : DefineCopyIf ( predicate-xt ops-a ops-out "name" -- )
    (ParseAlgorithmName)
    ROT (AlgTempEmitXt) !
    (PrepareAO)
    (AlgTempEmitXt) @ (BindUnary)
    InputIteratorCategory (RequireACategory)
    InputIteratorCategory (RequireOutCategory)
    (OPS-NEXT-XT) S" source Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" source SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" source Read is required" (RequireA)
    (OPS-NEXT-XT) S" output Next is required" (RequireOut)
    (OPS-WRITE-XT) S" output Write is required" (RequireOut)
    S" 2 PICK A-READ DUP PRED IF 1 PICK O-WRITE O-NEXT ELSE DROP THEN ROT A-NEXT -ROT"
    (EmitHelper)
    S" BEGIN >R 2DUP A-SENTINEL 0= R> SWAP WHILE HELPER REPEAT NIP SWAP DROP"
    (FinishNamedAlgorithm)
;

Export : DefineTransform ( transform-xt ops-a ops-out "name" -- )
    (ParseAlgorithmName)
    ROT (AlgTempEmitXt) !
    (PrepareAO)
    (AlgTempEmitXt) @ (BindTransform)
    InputIteratorCategory (RequireACategory)
    InputIteratorCategory (RequireOutCategory)
    (OPS-NEXT-XT) S" source Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" source SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" source Read is required" (RequireA)
    (OPS-NEXT-XT) S" output Next is required" (RequireOut)
    (OPS-READ-XT) S" output Read is required" (RequireOut)
    S" 2 PICK A-READ 1 PICK O-READ TRANS ROT A-NEXT -ROT O-NEXT"
    (EmitHelper)
    S" BEGIN >R 2DUP A-SENTINEL 0= R> SWAP WHILE HELPER REPEAT NIP SWAP DROP"
    (FinishNamedAlgorithm)
;

Export : DefineReplaceIf ( predicate-xt ops-a "name" -- )
    (ParseAlgorithmName)
    SWAP (AlgTempEmitXt) !
    (PrepareA)
    (AlgTempEmitXt) @ (BindUnary)
    InputIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" replace Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" replace SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" replace Read is required" (RequireA)
    (OPS-WRITE-XT) S" replace Write is required" (RequireA)
    S" 2 PICK A-READ DUP PRED IF DROP DUP 3 PICK A-WRITE ELSE DROP THEN ROT A-NEXT -ROT"
    (EmitHelper)
    S" >R BEGIN 2DUP A-SENTINEL 0= R> SWAP WHILE HELPER >R REPEAT 2DROP DROP R> DROP"
    (FinishNamedAlgorithm)
;

Export : DefineReplace ( predicate-xt ops-a "name" -- )
    (ParseAlgorithmName)
    SWAP (AlgTempEmitXt) !
    (PrepareA)
    (AlgTempEmitXt) @ (BindUnary)
    InputIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" replace Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" replace SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" replace Read is required" (RequireA)
    (OPS-WRITE-XT) S" replace Write is required" (RequireA)
    S" 3 PICK A-READ 2 PICK PRED IF DUP 4 PICK A-WRITE THEN ROT A-NEXT -ROT"
    (EmitHelper)
    S" >R >R BEGIN 2DUP A-SENTINEL 0= WHILE R> R> HELPER >R >R REPEAT 2DROP R> R> 2DROP"
    (FinishNamedAlgorithm)
;

Export : DefineRemoveIf ( predicate-xt ops-a "name" -- )
    (ParseAlgorithmName)
    SWAP (AlgTempEmitXt) !
    (PrepareA)
    (AlgTempEmitXt) @ (BindUnary)
    InputIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" remove Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" remove SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" remove Read is required" (RequireA)
    (OPS-WRITE-XT) S" remove Write is required" (RequireA)
    S" 2 PICK A-READ DUP PRED IF DROP ELSE 1 PICK A-WRITE A-NEXT THEN ROT A-NEXT -ROT"
    (EmitHelper)
    S" OVER >R BEGIN 2DUP A-SENTINEL 0= WHILE R> HELPER >R REPEAT 2DROP R>"
    (FinishNamedAlgorithm)
;

Export : DefineSwapRanges ( ops-a ops-b "name" -- )
    (ParseAlgorithmName)
    (PrepareAB)
    InputIteratorCategory (RequireACategory)
    InputIteratorCategory (RequireBCategory)
    (OPS-NEXT-XT) S" left Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" left SentinelEqual is required" (RequireA)
    (OPS-SWAP-XT) S" left Swap is required" (RequireA)
    (OPS-NEXT-XT) S" right Next is required" (RequireB)
    S" DUP 2 PICK A-SWAP ROT A-NEXT -ROT B-NEXT"
    (EmitHelper)
    S" BEGIN >R 2DUP A-SENTINEL 0= R> SWAP WHILE HELPER REPEAT NIP SWAP DROP"
    (FinishNamedAlgorithm)
;

Export : DefineReverse ( ops-a "name" -- )
    (ParseAlgorithmName)
    (PrepareA)
    RandomAccessIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" reverse Next is required" (RequireA)
    (OPS-PREV-XT) S" reverse Prev is required" (RequireA)
    (OPS-DISTANCE-XT) S" reverse Distance is required" (RequireA)
    (OPS-SWAP-XT) S" reverse Swap is required" (RequireA)
    S" DUP A-PREV DUP >R 2 PICK A-SWAP SWAP A-NEXT SWAP R> NIP"
    (EmitHelper)
    S" 2DUP A-DISTANCE 2/ 0 ?DO HELPER LOOP 2DROP"
    (FinishNamedAlgorithm)
;

Export : DefineIsSortedUntil ( comparator-xt ops-a "name" -- )
    (ParseAlgorithmName)
    SWAP (AlgTempEmitXt) !
    (PrepareA)
    (AlgTempEmitXt) @ (BindComparator)
    InputIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" sorted Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" sorted SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" sorted Read is required" (RequireA)
    S" DUP >R BEGIN 2DUP A-SENTINEL 0= WHILE DUP A-NEXT OVER A-SENTINEL IF NIP DROP DUP ELSE 2 PICK A-READ OVER A-READ CMP IF NIP SWAP ELSE R> DROP 2 PICK >R NIP DROP DUP THEN THEN REPEAT 2DROP R>"
    (FinishNamedAlgorithm)
;

Export : DefineIsSorted ( comparator-xt ops-a "name" -- )
    (ParseAlgorithmName)
    SWAP (AlgTempEmitXt) !
    (PrepareA)
    (AlgTempEmitXt) @ (BindComparator)
    InputIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" sorted Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" sorted SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" sorted Read is required" (RequireA)
    S" -1 >R BEGIN 2DUP A-SENTINEL 0= WHILE DUP A-NEXT OVER A-SENTINEL IF NIP DROP DUP ELSE 2 PICK A-READ OVER A-READ CMP IF NIP SWAP ELSE R> DROP 0 >R NIP DROP DUP THEN THEN REPEAT 2DROP R>"
    (FinishNamedAlgorithm)
;

Export : DefineSort ( comparator-xt ops-a "name" -- )
    (ParseAlgorithmName)
    SWAP (AlgTempEmitXt) !
    (PrepareA)
    (AlgTempEmitXt) @ (BindComparator)
    RandomAccessIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" sort Next is required" (RequireA)
    (OPS-ADVANCE-XT) S" sort Advance is required" (RequireA)
    (OPS-DISTANCE-XT) S" sort Distance is required" (RequireA)
    (OPS-SWAP-XT) S" sort Swap is required" (RequireA)
    S" 2DUP A-DISTANCE DUP 1 > IF 1 ?DO I BEGIN DUP 0> WHILE DUP 1- 3 PICK A-ADVANCE 3 PICK 2 PICK A-ADVANCE 2DUP CMP IF 2DROP DROP 0 ELSE A-SWAP 1- THEN REPEAT LOOP THEN 2DROP"
    (FinishNamedAlgorithm)
;

Export : DefineStableSort ( comparator-xt ops-a "name" -- )
    (ParseAlgorithmName)
    SWAP (AlgTempEmitXt) !
    (PrepareA)
    (AlgTempEmitXt) @ (BindComparator)
    RandomAccessIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" stable sort Next is required" (RequireA)
    (OPS-ADVANCE-XT) S" stable sort Advance is required" (RequireA)
    (OPS-DISTANCE-XT) S" stable sort Distance is required" (RequireA)
    (OPS-SWAP-XT) S" stable sort Swap is required" (RequireA)
    S" 2DUP A-DISTANCE DUP 1 > IF 1 ?DO I BEGIN DUP 0> WHILE DUP 1- 3 PICK A-ADVANCE 3 PICK 2 PICK A-ADVANCE 2DUP SWAP CMP IF A-SWAP 1- ELSE 2DROP DROP 0 THEN REPEAT LOOP THEN 2DROP"
    (FinishNamedAlgorithm)
;

Export : DefineMinElement ( comparator-xt ops-a "name" -- )
    (ParseAlgorithmName)
    SWAP (AlgTempEmitXt) !
    (PrepareA)
    (AlgTempEmitXt) @ (BindComparator)
    InputIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" min-element Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" min-element SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" min-element Read is required" (RequireA)
    S" OVER >R BEGIN 2DUP A-SENTINEL 0= WHILE R> 2 PICK A-READ OVER A-READ CMP IF 2 PICK SWAP DROP >R ELSE >R THEN SWAP A-NEXT SWAP REPEAT 2DROP R>"
    (FinishNamedAlgorithm)
;

Export : DefineMaxElement ( comparator-xt ops-a "name" -- )
    (ParseAlgorithmName)
    SWAP (AlgTempEmitXt) !
    (PrepareA)
    (AlgTempEmitXt) @ (BindComparator)
    InputIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" max-element Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" max-element SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" max-element Read is required" (RequireA)
    S" OVER >R BEGIN 2DUP A-SENTINEL 0= WHILE R> 2 PICK A-READ OVER A-READ SWAP CMP IF 2 PICK SWAP DROP >R ELSE >R THEN SWAP A-NEXT SWAP REPEAT 2DROP R>"
    (FinishNamedAlgorithm)
;

Export : DefineLowerBound ( comparator-xt ops-a "name" -- )
    (ParseAlgorithmName)
    SWAP (AlgTempEmitXt) !
    (PrepareA)
    (AlgTempEmitXt) @ (BindComparator)
    InputIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" lower-bound Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" lower-bound SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" lower-bound Read is required" (RequireA)
    S" OVER >R BEGIN 2 PICK 2 PICK A-SENTINEL 0= WHILE 2 PICK A-READ OVER CMP IF ROT A-NEXT -ROT ELSE R> DROP 2 PICK >R NIP OVER SWAP THEN REPEAT DROP 2DROP R>"
    (FinishNamedAlgorithm)
;

Export : DefineUpperBound ( comparator-xt ops-a "name" -- )
    (ParseAlgorithmName)
    SWAP (AlgTempEmitXt) !
    (PrepareA)
    (AlgTempEmitXt) @ (BindComparator)
    InputIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" upper-bound Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" upper-bound SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" upper-bound Read is required" (RequireA)
    S" OVER >R BEGIN 2 PICK 2 PICK A-SENTINEL 0= WHILE 2 PICK A-READ >R DUP R> CMP IF R> DROP 2 PICK >R NIP OVER SWAP ELSE ROT A-NEXT -ROT THEN REPEAT DROP 2DROP R>"
    (FinishNamedAlgorithm)
;

Export : DefineAccumulate ( binary-xt ops-a "name" -- )
    (ParseAlgorithmName)
    SWAP (AlgTempEmitXt) !
    (PrepareA)
    (AlgTempEmitXt) @ (BindBinary)
    InputIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" accumulate Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" accumulate SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" accumulate Read is required" (RequireA)
    S" >R BEGIN 2DUP A-SENTINEL 0= WHILE OVER A-READ R> SWAP BIN >R SWAP A-NEXT SWAP REPEAT 2DROP R>"
    (FinishNamedAlgorithm)
;

Export : DefineReduce ( binary-xt ops-a "name" -- )
    (ParseAlgorithmName)
    SWAP (AlgTempEmitXt) !
    (PrepareA)
    (AlgTempEmitXt) @ (BindBinary)
    InputIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" reduce Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" reduce SentinelEqual is required" (RequireA)
    (OPS-READ-XT) S" reduce Read is required" (RequireA)
    S" >R BEGIN 2DUP A-SENTINEL 0= WHILE OVER A-READ R> SWAP BIN >R SWAP A-NEXT SWAP REPEAT 2DROP R>"
    (FinishNamedAlgorithm)
;

Export : DefineIota ( ops-a "name" -- )
    (ParseAlgorithmName)
    (PrepareA)
    InputIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" iota Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" iota SentinelEqual is required" (RequireA)
    (OPS-WRITE-XT) S" iota Write is required" (RequireA)
    S" DUP 3 PICK A-WRITE ROT A-NEXT -ROT DUP @ 1+ OVER !"
    (EmitHelper)
    S" BEGIN 2 PICK 2 PICK A-SENTINEL 0= WHILE HELPER REPEAT 2DROP DROP"
    (FinishNamedAlgorithm)
;

Export : DefineInnerProduct ( multiply-xt add-xt ops-a ops-b "name" -- )
    (ParseAlgorithmName)
    2>R
    (AlgPredicateXt) !
    (AlgTransformXt) !
    R> R>
    (PrepareAB)
    (AlgTransformXt) @ (BindTransform)
    (AlgPredicateXt) @ (BindBinary)
    RandomAccessIteratorCategory (RequireACategory)
    RandomAccessIteratorCategory (RequireBCategory)
    (OPS-ADVANCE-XT) S" inner-product left Advance is required" (RequireA)
    (OPS-ADVANCE-XT) S" inner-product right Advance is required" (RequireB)
    S" SWAP 0 ?DO 2 PICK I A-ADVANCE 2 PICK I B-ADVANCE TRANS BIN LOOP 2DROP"
    (FinishNamedAlgorithm)
;

Export : DefineMin ( comparator-xt "name" -- )
    (BindComparator)
    S" 2DUP CMP IF DROP ELSE NIP THEN"
    (FinishAlgorithm)
;

Export : DefineMax ( comparator-xt "name" -- )
    (BindComparator)
    S" 2DUP SWAP CMP IF DROP ELSE NIP THEN"
    (FinishAlgorithm)
;

Export : DefineMinValue ( comparator-xt "name" -- )
    DefineMin
;

Export : DefineMaxValue ( comparator-xt "name" -- )
    DefineMax
;

Export : DefineFill ( ops-a "name" -- )
    (ParseAlgorithmName)
    (PrepareA)
    InputIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" fill Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" fill SentinelEqual is required" (RequireA)
    (OPS-WRITE-XT) S" fill Write is required" (RequireA)
    S" >R DUP >R OVER >R A-WRITE R> R> R> ROT ROT A-NEXT -ROT"
    (EmitHelper)
    S" >R BEGIN 2DUP A-SENTINEL 0= WHILE R> ROT ROT HELPER >R REPEAT 2DROP R> DROP"
    (FinishNamedAlgorithm)
;

Export : DefineFillN ( ops-a "name" -- )
    (ParseAlgorithmName)
    (PrepareA)
    InputIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" fill Next is required" (RequireA)
    (OPS-WRITE-XT) S" fill Write is required" (RequireA)
    S" >R DUP >R DUP >R A-WRITE R> R> R> ROT A-NEXT"
    (EmitHelper)
    S" >R BEGIN DUP 0> WHILE R> ROT ROT HELPER SWAP 1- SWAP >R REPEAT 2DROP R> DROP"
    (FinishNamedAlgorithm)
;

Export : DefineGenerate ( generator-xt ops-a "name" -- )
    (ParseAlgorithmName)
    (PrepareA)
    (BindGenerator)
    InputIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" generate Next is required" (RequireA)
    (OPS-SENTINEL-EQUAL-XT) S" generate SentinelEqual is required" (RequireA)
    S" >R DUP >R GEN R> R> SWAP A-NEXT SWAP"
    (EmitHelper)
    S" BEGIN 2DUP A-SENTINEL 0= WHILE HELPER REPEAT 2DROP"
    (FinishNamedAlgorithm)
;

Export : DefineGenerateN ( generator-xt ops-a "name" -- )
    (ParseAlgorithmName)
    (PrepareA)
    (BindGenerator)
    InputIteratorCategory (RequireACategory)
    (OPS-NEXT-XT) S" generate Next is required" (RequireA)
    S" DUP >R GEN R> A-NEXT"
    (EmitHelper)
    S" >R BEGIN R> DUP 0> WHILE >R HELPER R> 1- >R REPEAT 2DROP"
    (FinishNamedAlgorithm)
;

EndModule
(AlgorithmImportCount) @ EndImport