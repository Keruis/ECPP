REQUIRE basic/module.fs

BeginModule Iterator

VARIABLE (IteratorPrivateWid)
VARIABLE (IteratorPublicWid)
VARIABLE (IteratorModuleBase)

GET-CURRENT (IteratorPrivateWid) !
2DUP
(IteratorModuleBase) !
(IteratorPublicWid) !

1 CONSTANT (CAT-INPUT)
2 CONSTANT (CAT-OUTPUT)
3 CONSTANT (CAT-FORWARD)
4 CONSTANT (CAT-BIDIRECTIONAL)
5 CONSTANT (CAT-RANDOM-ACCESS)
6 CONSTANT (CAT-CONTIGUOUS)

Export : InputIteratorCategory ( -- category )
    (CAT-INPUT)
;

Export : OutputIteratorCategory ( -- category )
    (CAT-OUTPUT)
;

Export : ForwardIteratorCategory ( -- category )
    (CAT-FORWARD)
;

Export : BidirectionalIteratorCategory ( -- category )
    (CAT-BIDIRECTIONAL)
;

Export : RandomAccessIteratorCategory ( -- category )
    (CAT-RANDOM-ACCESS)
;

Export : ContiguousIteratorCategory ( -- category )
    (CAT-CONTIGUOUS)
;

0 CONSTANT (ITER-CATEGORY)
CELL CONSTANT (ITER-NEXT-XT)
2 CELLS CONSTANT (ITER-PREV-XT)
3 CELLS CONSTANT (ITER-ADVANCE-XT)
4 CELLS CONSTANT (ITER-DISTANCE-XT)
5 CELLS CONSTANT (ITER-EQUAL-XT)
6 CELLS CONSTANT (ITER-LESS-XT)
7 CELLS CONSTANT (ITER-READ-XT)
8 CELLS CONSTANT (ITER-WRITE-XT)
9 CELLS CONSTANT (ITER-SWAP-XT)
10 CELLS CONSTANT (ITER-BEGIN-XT)
11 CELLS CONSTANT (ITER-END-XT)
12 CELLS CONSTANT (ITER-SENTINEL-EQUAL-XT)
13 CELLS CONSTANT (ITER-DESCRIPTOR-BYTES)

S" Iterator: invalid iterator type definition" EXCEPTION CONSTANT (ITER-ERR-TYPE)
VARIABLE (IteratorBuilder)
VARIABLE (IteratorFinal)

Export : IteratorBuilderActive? ( -- flag )
    (IteratorBuilder) @ 0<>
;

256 CONSTANT (ITERATOR-NAME-BYTES)
VARIABLE (IteratorNameLen)
CREATE (IteratorNameBuffer) (ITERATOR-NAME-BYTES) ALLOT

: (RequiredName) ( "name" -- c-addr u )
    PARSE-NAME
    DUP 0=
    IF
        S" Iterator: a name is required" Fail
    THEN
;

: (ActiveIterator) ( -- iterator-type )
    (IteratorBuilder) @
    ?DUP 0=
    IF
        (ITER-ERR-TYPE) THROW
    THEN
;

: (IteratorFail) ( c-addr u -- )
    2DROP
    0 (IteratorBuilder) !
    (ITER-ERR-TYPE) THROW
;

: (CreateNamed) ( c-addr u -- body-addr )
    NEXTNAME
    CREATE
    HERE
;

: (CategoryValid?) ( category -- flag )
    DUP (CAT-INPUT) =
    OVER (CAT-OUTPUT) = OR
    OVER (CAT-FORWARD) = OR
    OVER (CAT-BIDIRECTIONAL) = OR
    OVER (CAT-RANDOM-ACCESS) = OR
    SWAP (CAT-CONTIGUOUS) =
    OR
;

Export : IteratorCategoryInput? ( category -- flag )
    DUP (CAT-INPUT) =
    OVER (CAT-FORWARD) = OR
    OVER (CAT-BIDIRECTIONAL) = OR
    OVER (CAT-RANDOM-ACCESS) = OR
    SWAP (CAT-CONTIGUOUS) =
    OR
;

Export : IteratorCategoryOutput? ( category -- flag )
    (CAT-OUTPUT) =
;

Export : IteratorCategoryForward? ( category -- flag )
    DUP (CAT-FORWARD) >=
    SWAP (CAT-CONTIGUOUS) <=
    AND
;

Export : IteratorCategoryBidirectional? ( category -- flag )
    DUP (CAT-BIDIRECTIONAL) >=
    SWAP (CAT-CONTIGUOUS) <=
    AND
;

Export : IteratorCategoryRandomAccess? ( category -- flag )
    DUP (CAT-RANDOM-ACCESS) >=
    SWAP (CAT-CONTIGUOUS) <=
    AND
;

Export : IteratorCategoryContiguous? ( category -- flag )
    (CAT-CONTIGUOUS) =
;

Export : BeginIteratorTypeNamed ( category c-addr u -- )
    (IteratorBuilder) @
    IF
        2DROP DROP S" Iterator: nested iterator type definition" (IteratorFail)
    THEN
    ROT
    DUP (CategoryValid?) 0=
    IF
        2DROP DROP S" Iterator: unknown iterator category" (IteratorFail)
    THEN
    >R
    2DUP FIND-NAME IF
        S" Iterator: iterator type already exists" (IteratorFail)
    THEN
    DUP (IteratorNameLen) !
    2DUP
    (IteratorNameBuffer) SWAP MOVE
    (CreateNamed)
    (ITER-DESCRIPTOR-BYTES) ALLOT
    DUP (ITER-DESCRIPTOR-BYTES) ERASE
    R>
    OVER (ITER-CATEGORY) + !
    DUP (IteratorBuilder) !
    DROP
;

Export : BeginIteratorType ( category "name" -- )
    >R
    (RequiredName)
    R>
    -ROT
    BeginIteratorTypeNamed
;

Export : UseIteratorNext ( xt -- )
    (ActiveIterator) (ITER-NEXT-XT) + !
;

Export : UseIteratorPrev ( xt -- )
    (ActiveIterator) (ITER-PREV-XT) + !
;

Export : UseIteratorAdvance ( xt -- )
    (ActiveIterator) (ITER-ADVANCE-XT) + !
;

Export : UseIteratorDistance ( xt -- )
    (ActiveIterator) (ITER-DISTANCE-XT) + !
;

Export : UseIteratorEqual ( xt -- )
    (ActiveIterator) (ITER-EQUAL-XT) + !
;

Export : UseIteratorLess ( xt -- )
    (ActiveIterator) (ITER-LESS-XT) + !
;

Export : UseIteratorRead ( xt -- )
    (ActiveIterator) (ITER-READ-XT) + !
;

Export : UseIteratorWrite ( xt -- )
    (ActiveIterator) (ITER-WRITE-XT) + !
;

Export : UseIteratorSwap ( xt -- )
    (ActiveIterator) (ITER-SWAP-XT) + !
;

Export : UseIteratorBegin ( xt -- )
    (ActiveIterator) (ITER-BEGIN-XT) + !
;

Export : UseIteratorEnd ( xt -- )
    (ActiveIterator) (ITER-END-XT) + !
;

Export : UseIteratorSentinelEqual ( xt -- )
    (ActiveIterator) (ITER-SENTINEL-EQUAL-XT) + !
;

Export : NoIteratorNext ( -- )
    0 (ActiveIterator) (ITER-NEXT-XT) + !
;

Export : NoIteratorPrev ( -- )
    0 (ActiveIterator) (ITER-PREV-XT) + !
;

Export : NoIteratorAdvance ( -- )
    0 (ActiveIterator) (ITER-ADVANCE-XT) + !
;

Export : NoIteratorDistance ( -- )
    0 (ActiveIterator) (ITER-DISTANCE-XT) + !
;

Export : NoIteratorEqual ( -- )
    0 (ActiveIterator) (ITER-EQUAL-XT) + !
;

Export : NoIteratorLess ( -- )
    0 (ActiveIterator) (ITER-LESS-XT) + !
;

Export : NoIteratorRead ( -- )
    0 (ActiveIterator) (ITER-READ-XT) + !
;

Export : NoIteratorWrite ( -- )
    0 (ActiveIterator) (ITER-WRITE-XT) + !
;

Export : NoIteratorSwap ( -- )
    0 (ActiveIterator) (ITER-SWAP-XT) + !
;

Export : NoIteratorBegin ( -- )
    0 (ActiveIterator) (ITER-BEGIN-XT) + !
;

Export : NoIteratorEnd ( -- )
    0 (ActiveIterator) (ITER-END-XT) + !
;

Export : NoIteratorSentinelEqual ( -- )
    0 (ActiveIterator) (ITER-SENTINEL-EQUAL-XT) + !
;

: (RequireXt) ( xt c-addr u -- )
    ROT 0=
    IF
        (IteratorFail)
    ELSE
        2DROP
    THEN
;

: (ValidateIterator) ( iterator-type -- )
    >R
    R@ (ITER-CATEGORY) + @
    DUP (CategoryValid?) 0=
    IF
        DROP S" Iterator: invalid category at finalization" (IteratorFail)
    THEN
    DUP (CAT-OUTPUT) =
    IF
        DROP
        R@ (ITER-NEXT-XT) + @
        S" Iterator: output iterator requires Next" (RequireXt)
        R@ (ITER-EQUAL-XT) + @
        S" Iterator: iterator requires Equal" (RequireXt)
        R@ (ITER-WRITE-XT) + @
        S" Iterator: output iterator requires Write" (RequireXt)
    ELSE
        DUP IteratorCategoryInput? 0=
        IF
            DROP S" Iterator: non-output iterator requires Input capability"
            (IteratorFail)
        THEN
        R@ (ITER-NEXT-XT) + @
        S" Iterator: iterator requires Next" (RequireXt)
        R@ (ITER-EQUAL-XT) + @
        S" Iterator: iterator requires Equal" (RequireXt)
        R@ (ITER-READ-XT) + @
        S" Iterator: input iterator requires Read" (RequireXt)
        R@ (ITER-CATEGORY) + @ IteratorCategoryBidirectional? 0=
        IF
            R@ (ITER-PREV-XT) + @ 0<>
            IF
                S" Iterator: Prev requires bidirectional capability"
                (IteratorFail)
            THEN
        ELSE
            R@ (ITER-PREV-XT) + @
            S" Iterator: bidirectional capability requires Prev" (RequireXt)
        THEN
        R@ (ITER-CATEGORY) + @ IteratorCategoryRandomAccess? 0=
        IF
            R@ (ITER-ADVANCE-XT) + @ 0<>
            R@ (ITER-DISTANCE-XT) + @ 0<> OR
            R@ (ITER-LESS-XT) + @ 0<> OR
            IF
                S" Iterator: random-access operations require random access"
                (IteratorFail)
            THEN
        ELSE
            R@ (ITER-ADVANCE-XT) + @
            S" Iterator: random access requires Advance" (RequireXt)
            R@ (ITER-DISTANCE-XT) + @
            S" Iterator: random access requires Distance" (RequireXt)
            R@ (ITER-LESS-XT) + @
            S" Iterator: random access requires Less" (RequireXt)
        THEN
        DROP
    THEN
    R@ (ITER-BEGIN-XT) + @ 0=
    R@ (ITER-END-XT) + @ 0= <>
    IF
        S" Iterator: Begin and End must be supplied together" (IteratorFail)
    THEN
    R> DROP
;

Export : IteratorTypeCategory ( iterator-type -- category )
    (ITER-CATEGORY) + @
;

Export : IteratorTypeNextXt ( iterator-type -- xt )
    (ITER-NEXT-XT) + @
;

Export : IteratorTypePrevXt ( iterator-type -- xt )
    (ITER-PREV-XT) + @
;

Export : IteratorTypeAdvanceXt ( iterator-type -- xt )
    (ITER-ADVANCE-XT) + @
;

Export : IteratorTypeDistanceXt ( iterator-type -- xt )
    (ITER-DISTANCE-XT) + @
;

Export : IteratorTypeEqualXt ( iterator-type -- xt )
    (ITER-EQUAL-XT) + @
;

Export : IteratorTypeLessXt ( iterator-type -- xt )
    (ITER-LESS-XT) + @
;

Export : IteratorTypeReadXt ( iterator-type -- xt )
    (ITER-READ-XT) + @
;

Export : IteratorTypeWriteXt ( iterator-type -- xt )
    (ITER-WRITE-XT) + @
;

Export : IteratorTypeSwapXt ( iterator-type -- xt )
    (ITER-SWAP-XT) + @
;

Export : IteratorTypeBeginXt ( iterator-type -- xt )
    (ITER-BEGIN-XT) + @
;

Export : IteratorTypeEndXt ( iterator-type -- xt )
    (ITER-END-XT) + @
;

Export : IteratorTypeSentinelEqualXt ( iterator-type -- xt )
    (ITER-SENTINEL-EQUAL-XT) + @
;

: (IteratorName) ( suffix-addr suffix-u -- name-addr name-u )
    DUP (IteratorNameLen) @ + (ITERATOR-NAME-BYTES) >
    IF
        2DROP S" Iterator: generated name is too long" Fail
    THEN
    DUP >R
    (IteratorNameBuffer) (IteratorNameLen) @ +
    SWAP MOVE
    R> (IteratorNameLen) @ +
    (IteratorNameBuffer) SWAP
;

: (StartPublic) ( c-addr u -- xt )
    GET-CURRENT >R
    (IteratorPublicWid) @ SET-CURRENT
    NEXTNAME
    ['] : EXECUTE
    LATEST NAME>INT
    R> SET-CURRENT
;

: (StartPublicConstant) ( n c-addr u -- xt )
    GET-CURRENT >R
    (IteratorPublicWid) @ SET-CURRENT
    NEXTNAME
    ['] CONSTANT EXECUTE
    LATEST NAME>INT
    R> SET-CURRENT
;

: (GenerateSimple) ( suffix-addr suffix-u xt -- )
    >R
    (IteratorName)
    (StartPublic)
    R> SWAP >R
    COMPILE,
    POSTPONE ;
    R> DROP
;

: (GenerateIfXt) ( xt suffix-addr suffix-u -- )
    ROT ?DUP
    IF
        (GenerateSimple)
    ELSE
        2DROP
    THEN
;

: (GenerateConstant) ( n suffix-addr suffix-u -- )
    ROT
    >R
    (IteratorName)
    R>
    -ROT
    (StartPublicConstant) DROP
;

: (GenerateCategoryWords) ( -- )
    (IteratorFinal) @ (ITER-CATEGORY) + @
    S" .Category" (GenerateConstant)

    (IteratorFinal) @ (ITER-CATEGORY) + @ IteratorCategoryInput?
    S" .Input?" (GenerateConstant)
    (IteratorFinal) @ (ITER-CATEGORY) + @ IteratorCategoryOutput?
    S" .Output?" (GenerateConstant)
    (IteratorFinal) @ (ITER-CATEGORY) + @ IteratorCategoryForward?
    S" .Forward?" (GenerateConstant)
    (IteratorFinal) @ (ITER-CATEGORY) + @ IteratorCategoryBidirectional?
    S" .Bidirectional?" (GenerateConstant)
    (IteratorFinal) @ (ITER-CATEGORY) + @ IteratorCategoryRandomAccess?
    S" .RandomAccess?" (GenerateConstant)
    (IteratorFinal) @ (ITER-CATEGORY) + @ IteratorCategoryContiguous?
    S" .Contiguous?" (GenerateConstant)
;

: (GenerateOperationWords) ( -- )
    (IteratorFinal) @ (ITER-NEXT-XT) + @
    S" .Next" (GenerateIfXt)
    (IteratorFinal) @ (ITER-EQUAL-XT) + @
    S" .Equal" (GenerateIfXt)
    (IteratorFinal) @ (ITER-CATEGORY) + @
    IteratorCategoryOutput? 0=
    IF
        (IteratorFinal) @ (ITER-READ-XT) + @
        S" .Read" (GenerateIfXt)
    THEN
    (IteratorFinal) @ (ITER-WRITE-XT) + @
    S" .Write" (GenerateIfXt)
    (IteratorFinal) @ (ITER-CATEGORY) + @
    IteratorCategoryBidirectional?
    IF
        (IteratorFinal) @ (ITER-PREV-XT) + @
        S" .Prev" (GenerateIfXt)
    THEN
    (IteratorFinal) @ (ITER-CATEGORY) + @
    IteratorCategoryRandomAccess?
    IF
        (IteratorFinal) @ (ITER-ADVANCE-XT) + @
        S" .Advance" (GenerateIfXt)
        (IteratorFinal) @ (ITER-DISTANCE-XT) + @
        S" .Distance" (GenerateIfXt)
        (IteratorFinal) @ (ITER-LESS-XT) + @
        S" .Less" (GenerateIfXt)
    THEN
    (IteratorFinal) @ (ITER-SWAP-XT) + @
    S" .Swap" (GenerateIfXt)
    (IteratorFinal) @ (ITER-BEGIN-XT) + @
    S" .Begin" (GenerateIfXt)
    (IteratorFinal) @ (ITER-END-XT) + @
    S" .End" (GenerateIfXt)
    (IteratorFinal) @ (ITER-SENTINEL-EQUAL-XT) + @
    S" .SentinelEqual" (GenerateIfXt)
;

Export : EndIteratorType ( -- )
    (ActiveIterator)
    DUP
    (IteratorFinal) !
    0 (IteratorBuilder) !
    (ValidateIterator)
    (GenerateCategoryWords)
    (GenerateOperationWords)
;

EndModule
