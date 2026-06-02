\ deadband.fs — Deadband trigger in stack-based thinking
\
\ CoCapn Layer 0: when you need to know if a value has drifted
\ from center, but you don't have the RAM for objects.

1 constant ONE-SIDED-FLAG
0 constant NORMAL
1 constant APPROACHING
2 constant EXCEEDED

: deadband-create ( F: center tolerance -- addr )
  here 3 cells allot >r
  r@ 1 cells + f!
  r@ 0 cells + f!
  r@ 2 cells + 0 swap !
  r>
;

: deadband-center ( addr -- F: center ) 0 cells + f@ ;
: deadband-tolerance ( addr -- F: tolerance ) 1 cells + f@ ;
: deadband-flags ( addr -- flags ) 2 cells + @ ;
: deadband-set-flags ( flags addr -- ) 2 cells + ! ;

: deadband-one-sided ( addr -- )
  dup deadband-flags ONE-SIDED-FLAG or swap deadband-set-flags
;

: fabs ( F: r -- |r| ) fdup f0< if fnegate then ;

variable db-addr
fvariable check-absdiff
fvariable check-tolerance
fvariable check-diff

: deadband-check ( F: value addr -- state )
  db-addr !
  db-addr @ deadband-center
  fswap f-
  fdup check-diff f!
  fabs check-absdiff f!
  db-addr @ deadband-tolerance check-tolerance f!

  db-addr @ deadband-flags ONE-SIDED-FLAG and if
    \ One-sided: value <= center means Normal (conservation)
    check-diff f@ 0e0 f>= if
      \ diff = center - value >= 0 means value <= center

      NORMAL exit
    then
    \ value > center: still use two-sided logic below
  then

  \ Two-sided comparison using fvariables
  check-absdiff f@ check-tolerance f@ f< if
    NORMAL
  else
    check-absdiff f@ check-tolerance f@ fdup f+ f< if
      APPROACHING
    else
      EXCEEDED
    then
  then
;
