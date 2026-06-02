\ devices.fs — Device abstraction
\
\ Each device in CoCapn has a tier (capability level)
\ and a set of capabilities encoded as bit flags.
\
\ In Forth, a "device" is just a named word paired with
\ a data structure. No objects. No vtables.

\ Capabilities as bit flags (powers of 2)
1 constant CAP-GPS
2 constant CAP-COMPASS
4 constant CAP-WIND
8 constant CAP-AIS
16 constant CAP-DEPTH
32 constant CAP-RADAR
64 constant CAP-AUTOPILOT
128 constant CAP-COMMS
256 constant CAP-LOGGING
512 constant CAP-DISPLAY

\ Device table entry: each device is 3 cells
\   cell 0: tier (0-3 matching REFLEX..CLOUD)
\   cell 1: capabilities (bitmask)
\   cell 2: next device (linked list)

variable device-list  \ head of linked list
0 device-list !

\ Create a device node on the heap
: device-new ( tier capabilities -- addr )
  here 3 cells allot                   ( tier capabilities addr )
  tuck 0 cells + !                     \ store tier
  swap 1 cells + !                     \ store capabilities
  dup 2 cells + 0 swap !              \ next = 0
;

\ Register a device (add to linked list)
: device-register ( addr -- )
  device-list @ over 2 cells + !
  device-list !
;

\ Higher-level: create and register in one step
: device:create ( tier capabilities -- )
  device-new
  device-register
;

\ Iterate device list — used by higher-level words
: device-iter ( xt -- )
  device-list @
  begin
    dup
  while
    over swap >r execute r>
    @
  repeat
  2drop
;

\ Check if device at addr has a specific capability
: device-can? ( capability addr -- flag )
  1 cells + @ swap and 0<>
;

\ Check if current device is at a given tier
: device-at-tier? ( tier addr -- flag )
  0 cells + @ =
;

\ Walk the list and check if any device has capability
: any-device-can? ( capability -- flag )
  device-list @
  begin
    dup
  while
    over swap 1 cells + @ and if
      2drop true exit
    then
    @
  repeat
  2drop false
;

\ Walk the list and find first device with capability
: find-device-by-cap ( capability -- addr | 0 )
  device-list @
  begin
    dup
  while
    2dup swap 1 cells + @ and if
      nip exit
    then
    @
  repeat
  nip
;

\ Walk the list and find first device at tier
: find-device-by-tier ( tier -- addr | 0 )
  device-list @
  begin
    dup
  while
    2dup swap 0 cells + @ = if
      nip exit
    then
    @
  repeat
  nip
;

\ Print all devices
: .devices ( -- )
  device-list @
  begin
    dup
  while
    cr
    ."  Device at " dup .
    ."   Tier: " dup 0 cells + @ .
    ."   Caps: " dup 1 cells + @ .
    @
  repeat
  drop
;

\ Initialize default devices
: devices-init ( -- )
  0 device-list !
  \ Tier 0 (REFLEX): essential sensors
  REFLEX CAP-GPS CAP-COMPASS or device:create
  \ Tier 1 (BACKBONE): navigation aids
  BACKBONE CAP-WIND CAP-DEPTH or device:create
  \ Tier 2 (CORTEX): advanced systems
  CORTEX CAP-RADAR CAP-AIS or device:create
  \ Tier 3 (CLOUD): connected services
  CLOUD CAP-COMMS CAP-LOGGING or device:create
;
