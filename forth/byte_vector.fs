INCLUDE basic/module.fs

BeginModule ByteVector

Export : BVData ( bv-addr -- a-addr )
    @
;

Export : BVLength ( bv-addr -- u )
    CELL+ @
;

Export : BVCapacity ( bv-addr -- u )
    2 CELLS + @
;

: BVEnsure ( u bv-addr -- )
    >R R@ BVCapacity 2DUP <=
    IF 
        2DROP RDROP
        EXIT
    THEN

    2* 16 MAX MAX DUP
    R@ BVData ?DUP
    IF
        SWAP RESIZE THROW
    ELSE
        ALLOCATE THROW
    THEN

    R@ !
    R> 2 CELLS + !
;

Export : BVNew ( -- bv-addr )
    3 CELLS ALLOCATE THROW
    DUP 3 CELLS ERASE
;

Export : BVAppendSlice ( bv-addr a-addr u -- )
    ROT >R DUP 0= 
    IF
        2DROP
        R> DROP
        EXIT
    THEN

    DUP R@ BVLength +
    DUP R@ BVEnsure
    R@ BVData
    R@ BVLength
    SWAP +
    1 PICK R@ CELL+ !
    NIP SWAP MOVE
    R> DROP
;

Export : BVAppend ( bv-addr c -- )
    SWAP >R R@ BVLength 1+  
    DUP R@ BVEnsure
    DUP R@ CELL+ !
    R> BVData + c!
;

Export : BVFree ( bv-addr -- )
    DUP BVData ?DUP IF
        FREE THROW
    THEN
    FREE THROW
;


EndModule