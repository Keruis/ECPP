: (CC_Thens) ( u -- ) ( C: orig...u -- )
    0 ?DO
        POSTPONE THEN
    LOOP
;

: (CC_CSRoll) ( u -- )
    DUP 0 ?DO
        DUP >R CS-ROLL R>
    LOOP
    DROP
;

: CC_Begin ( -- u ) ( C: -- dest )
    POSTPONE BEGIN
    1
; IMMEDIATE

: CC_Again ( u -- ) ( C: orig... dest -- )
    >R
    POSTPONE AGAIN
    R> 1- (CC_Thens)
; IMMEDIATE

: CC_Until ( u -- ) ( C: orig... dest -- )
    >R
    POSTPONE UNTIL
    R> 1- (CC_Thens)
; IMMEDIATE

: CC_While ( u -- u ) ( C: orig... dest -- orig... orig dest )
    >R
    POSTPONE WHILE
    R>
; IMMEDIATE

: CC_Repeat ( u -- ) ( C: orig... orig dest -- )
    >R
    POSTPONE REPEAT
    R> 1- (CC_Thens)
; IMMEDIATE

: CC_Break ( u -- u+1 ) ( C: dest -- orig dest )
    >R
    POSTPONE AHEAD
    1 CS-ROLL
    R> 1+
; IMMEDIATE

: CC_Continue ( u -- u ) ( C: dest -- dest )
    >R
    0 CS-PICK
    POSTPONE AGAIN
    R>
; IMMEDIATE

: CC_If ( u -- u )
    >R
    POSTPONE IF
    R@ (CC_CSRoll)
    R>
; IMMEDIATE

: CC_Else ( u -- u )
    >R
    R@ CS-ROLL
    POSTPONE ELSE
    R@ (CC_CSRoll)
    R>
; IMMEDIATE

: CC_Then ( u -- u )
    >R
    R@ CS-ROLL
    POSTPONE THEN
    R>
; IMMEDIATE