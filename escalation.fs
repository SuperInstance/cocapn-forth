\ escalation.fs — Tier escalation chain
\
\ CoCapn Reflex → Backbone → Cortex → Cloud
\ Each escalation moves decision-making up the chain
\ when the current tier can't handle the situation.

\ Tiers (0-3, integer constants)
0 constant REFLEX
1 constant BACKBONE
2 constant CORTEX
3 constant CLOUD

\ State variables
variable current-tier
variable escalation-count

\ Tier names (9 chars each, including trailing NUL/spaces)
create tier-names
  char R c, char E c, char F char L c, char E char X c,
  32 c, 32 c, 32 c, 32 c,
  char B c, char A c, char C char K c, char B char O c,
  char N char E c, 32 c, 32 c, 32 c,
  char C c, char O c, char R char T c, char E char X c,
  32 c, 32 c, 32 c,
  char C c, char L c, char O char U c, char D char 32 c,
  32 c, 32 c, 32 c,

: current-tier@ ( -- tier )
  current-tier @
;

: escalation-count@ ( -- n )
  escalation-count @
;

: tier-name ( tier -- addr len )
  9 * tier-names + 9
;

: .tier ( -- )
  current-tier@ tier-name type
;

\ Initialize to REFLEX
: escalation-init ( -- )
  REFLEX current-tier !
  0 escalation-count !
;

: escalate ( reason -- new-tier )
  drop
  current-tier @ 1+
  CLOUD min
  dup current-tier !
  1 escalation-count +!
;

: de-escalate ( -- new-tier )
  current-tier @ 1-
  REFLEX max
  dup current-tier !
;
