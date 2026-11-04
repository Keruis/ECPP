REQUIRE vector.fs
REQUIRE algorithm.fs

Import Vector
Import Algorithm
+

BeginModule ByteVectorModule

Export Import Vector
Export Import Algorithm

BeginElementType ByteElement
    1 ElementSize
    1 ElementAlignment
    RawCopyable
    RawRelocatable
EndElementType

DefineVectorType ByteVector ByteElement

: (BYTE-SWAP) ( address1 address2 -- )
    >R
    DUP C@
    R@ C@
    SWAP
    R@ C!
    SWAP C!
    R> DROP
;

ContiguousIteratorCategory
BeginIteratorType ByteVectorAlgorithmIterator
    ByteVectorIterator IteratorTypeNextXt UseIteratorNext
    ByteVectorIterator IteratorTypePrevXt UseIteratorPrev
    ByteVectorIterator IteratorTypeAdvanceXt UseIteratorAdvance
    ByteVectorIterator IteratorTypeDistanceXt UseIteratorDistance
    ByteVectorIterator IteratorTypeLessXt UseIteratorLess
    ByteVectorIterator IteratorTypeReadXt UseIteratorRead
    ByteVectorIterator IteratorTypeWriteXt UseIteratorWrite
    ByteVectorIterator IteratorTypeEqualXt UseIteratorEqual
    ByteVectorIterator IteratorTypeSentinelEqualXt UseIteratorSentinelEqual
    ByteVectorIterator IteratorTypeBeginXt UseIteratorBegin
    ByteVectorIterator IteratorTypeEndXt UseIteratorEnd
    ' (BYTE-SWAP) UseIteratorSwap
EndIteratorType

DefineAlgorithmOps ByteVectorOps ByteVectorAlgorithmIterator

: BytePositive? ( byte-address -- flag )
    C@ 0>
;

: ByteZero? ( byte-address -- flag )
    C@ 0=
;

: ByteEqual? ( left-address right-address -- flag )
    C@ SWAP C@ =
;

: ByteLess? ( left-address right-address -- flag )
    C@ SWAP C@ >
;

: ByteAdd ( accumulator byte-address -- accumulator )
    C@ +
;

: ByteIncrementInto ( source-address destination-address -- )
    SWAP C@ 1+ SWAP C!
;

' BytePositive? ByteVectorOps DefineAllOf ByteVectorAlgorithm.AllOf
' BytePositive? ByteVectorOps DefineAnyOf ByteVectorAlgorithm.AnyOf
' BytePositive? ByteVectorOps DefineNoneOf ByteVectorAlgorithm.NoneOf
' BytePositive? ByteVectorOps DefineCountIf ByteVectorAlgorithm.CountIf
' BytePositive? ByteVectorOps DefineFindIf ByteVectorAlgorithm.FindIfPositive
' ByteZero? ByteVectorOps DefineFindIf ByteVectorAlgorithm.FindZero

' ByteEqual? ByteVectorOps DefineFind ByteVectorAlgorithm.Find
' ByteEqual? ByteVectorOps ByteVectorOps DefineEqual ByteVectorAlgorithm.Equal
' ByteEqual? ByteVectorOps ByteVectorOps DefineMismatch ByteVectorAlgorithm.Mismatch
' ByteEqual? ByteVectorOps ByteVectorOps DefineSearch ByteVectorAlgorithm.Search
' ByteLess? ByteVectorOps DefineLowerBound ByteVectorAlgorithm.LowerBound
' ByteLess? ByteVectorOps DefineUpperBound ByteVectorAlgorithm.UpperBound

ByteVectorOps ByteVectorOps DefineCopy ByteVectorAlgorithm.Copy
ByteVectorOps ByteVectorOps DefineCopyN ByteVectorAlgorithm.CopyN
ByteVectorOps DefineFill ByteVectorAlgorithm.Fill
' ByteIncrementInto ByteVectorOps ByteVectorOps DefineTransform ByteVectorAlgorithm.Increment
' ByteAdd ByteVectorOps DefineAccumulate ByteVectorAlgorithm.Sum
' ByteLess? ByteVectorOps DefineMinElement ByteVectorAlgorithm.MinElement
' ByteLess? ByteVectorOps DefineMaxElement ByteVectorAlgorithm.MaxElement
ByteVectorOps DefineReverse ByteVectorAlgorithm.Reverse

EndModule

EndImport