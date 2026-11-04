REQUIRE basic/module.fs

BeginModule ElemType

0 CONSTANT (TYPE-SIZE)
CELL CONSTANT (TYPE-ALIGN)
2 CELLS CONSTANT (TYPE-RAW-COPY)
3 CELLS CONSTANT (TYPE-RAW-RELOCATE)
4 CELLS CONSTANT (TYPE-NOTHROW-MOVE)
5 CELLS CONSTANT (TYPE-DEFAULT-XT)
6 CELLS CONSTANT (TYPE-COPY-XT)
7 CELLS CONSTANT (TYPE-MOVE-XT)
8 CELLS CONSTANT (TYPE-DESTROY-XT)
9 CELLS CONSTANT (TYPE-COPY-ASSIGN-XT)
10 CELLS CONSTANT (TYPE-MOVE-ASSIGN-XT)
11 CELLS CONSTANT (TYPE-DESCRIPTOR-BYTES)

S" ElementType: invalid element type definition" EXCEPTION CONSTANT (ELEM-ERR-TYPE)

VARIABLE (ElementBuilder)

Export : ElementTypeError ( -- n )
    (ELEM-ERR-TYPE)
;

Export : ElementTypeBuilderActive? ( -- flag )
    (ElementBuilder) @ 0<>
;

: (RequiredName) ( "name" -- c-addr u )
    PARSE-NAME
    DUP 0=
    IF
        S" ElementType: a name is required" Fail
    THEN
;

: (ActiveElement) ( -- type-addr )
    (ElementBuilder) @
    ?DUP 0=
    IF
        (ELEM-ERR-TYPE) THROW
    THEN
;

: (TypeFail) ( c-addr u -- )
    2DROP
    0 (ElementBuilder) !
    (ELEM-ERR-TYPE) THROW
;

: (CreateNamed) ( c-addr u -- body-addr )
    NEXTNAME
    CREATE
    HERE
;

Export : BeginElementType ( "name" -- )
    (ElementBuilder) @
    IF
        S" ElementType: nested element type definition" (TypeFail)
    THEN
    (RequiredName)
    2DUP FIND-NAME IF
        S" ElementType: element type already exists" (TypeFail)
    THEN
    (CreateNamed)
    (TYPE-DESCRIPTOR-BYTES) ALLOT
    DUP (TYPE-DESCRIPTOR-BYTES) ERASE
    (ElementBuilder) !
;

Export : ElementSize ( address-units -- )
    (ActiveElement) (TYPE-SIZE) + !
;

Export : ElementAlignment ( address-units -- )
    (ActiveElement) (TYPE-ALIGN) + !
;

Export : RawCopyable ( -- )
    -1 (ActiveElement) (TYPE-RAW-COPY) + !
;

Export : NoRawCopyable ( -- )
    0 (ActiveElement) (TYPE-RAW-COPY) + !
;

Export : RawRelocatable ( -- )
    -1 (ActiveElement) (TYPE-RAW-RELOCATE) + !
;

Export : NoRawRelocatable ( -- )
    0 (ActiveElement) (TYPE-RAW-RELOCATE) + !
;

Export : NoThrowMove ( -- )
    -1 (ActiveElement) (TYPE-NOTHROW-MOVE) + !
;

Export : MayThrowMove ( -- )
    0 (ActiveElement) (TYPE-NOTHROW-MOVE) + !
;

Export : UseDefaultConstruct ( xt -- )
    (ActiveElement) (TYPE-DEFAULT-XT) + !
;

Export : UseCopyConstruct ( xt -- )
    (ActiveElement) (TYPE-COPY-XT) + !
;

Export : UseMoveConstruct ( xt -- )
    (ActiveElement) (TYPE-MOVE-XT) + !
;

Export : UseDestroy ( xt -- )
    (ActiveElement) (TYPE-DESTROY-XT) + !
;

Export : UseCopyAssign ( xt -- )
    (ActiveElement) (TYPE-COPY-ASSIGN-XT) + !
;

Export : UseMoveAssign ( xt -- )
    (ActiveElement) (TYPE-MOVE-ASSIGN-XT) + !
;

Export : NoDefaultConstruct ( -- )
    0 (ActiveElement) (TYPE-DEFAULT-XT) + !
;

Export : NoCopyConstruct ( -- )
    0 (ActiveElement) (TYPE-COPY-XT) + !
;

Export : NoMoveConstruct ( -- )
    0 (ActiveElement) (TYPE-MOVE-XT) + !
;

Export : NoDestroy ( -- )
    0 (ActiveElement) (TYPE-DESTROY-XT) + !
;

Export : NoCopyAssign ( -- )
    0 (ActiveElement) (TYPE-COPY-ASSIGN-XT) + !
;

Export : NoMoveAssign ( -- )
    0 (ActiveElement) (TYPE-MOVE-ASSIGN-XT) + !
;

Export : EndElementType ( -- )
    (ActiveElement) (TYPE-SIZE) + @
    DUP 0<=
    IF
        DROP S" ElementType: size must be positive" (TypeFail)
    THEN
    DROP
    (ActiveElement) (TYPE-ALIGN) + @
    DUP 0<=
    IF
        DROP S" ElementType: alignment must be positive" (TypeFail)
    THEN
    DUP 1- OVER AND
    IF
        DROP S" ElementType: alignment must be a power of two" (TypeFail)
    THEN
    DROP
    (ActiveElement) (TYPE-SIZE) + @
    (ActiveElement) (TYPE-ALIGN) + @ MOD
    IF
        S" ElementType: size must be a multiple of alignment" (TypeFail)
    THEN
    0 (ElementBuilder) !
;

Export : ElementTypeSize ( type -- address-units )
    (TYPE-SIZE) + @
;

Export : ElementTypeAlignment ( type -- address-units )
    (TYPE-ALIGN) + @
;

Export : ElementTypeRawCopyable? ( type -- flag )
    (TYPE-RAW-COPY) + @
;

Export : ElementTypeRawRelocatable? ( type -- flag )
    (TYPE-RAW-RELOCATE) + @
;

Export : ElementTypeNoThrowMove? ( type -- flag )
    (TYPE-NOTHROW-MOVE) + @
;

Export : ElementTypeCopyConstructible? ( type -- flag )
    DUP (TYPE-RAW-COPY) + @
    SWAP (TYPE-COPY-XT) + @ 0<>
    OR
;

Export : ElementTypeMoveConstructible? ( type -- flag )
    DUP (TYPE-MOVE-XT) + @ 0<>
    IF
        DROP -1
        EXIT
    THEN
    DUP (TYPE-RAW-COPY) + @
    SWAP (TYPE-DESTROY-XT) + @ 0=
    AND
;

Export : ElementTypeCopyAssignable? ( type -- flag )
    DUP (TYPE-RAW-COPY) + @
    SWAP (TYPE-COPY-ASSIGN-XT) + @ 0<>
    OR
;

Export : ElementTypeMoveAssignable? ( type -- flag )
    DUP (TYPE-MOVE-ASSIGN-XT) + @ 0<>
    IF
        DROP -1
        EXIT
    THEN
    ElementTypeCopyAssignable?
;

Export : ElementTypeHasDestructor? ( type -- flag )
    (TYPE-DESTROY-XT) + @ 0<>
;

Export : ElementTypeDefaultConstructXt ( type -- xt )
    (TYPE-DEFAULT-XT) + @
;

Export : ElementTypeCopyConstructXt ( type -- xt )
    (TYPE-COPY-XT) + @
;

Export : ElementTypeMoveConstructXt ( type -- xt )
    (TYPE-MOVE-XT) + @
;

Export : ElementTypeDestroyXt ( type -- xt )
    (TYPE-DESTROY-XT) + @
;

Export : ElementTypeCopyAssignXt ( type -- xt )
    (TYPE-COPY-ASSIGN-XT) + @
;

Export : ElementTypeMoveAssignXt ( type -- xt )
    (TYPE-MOVE-ASSIGN-XT) + @
;

EndModule
