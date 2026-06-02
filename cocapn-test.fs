\ cocapn-test.fs — Test harness for CoCapn Forth
\
\ Run with: gforth cocapn-test.fs -e bye

include deadband.fs
include autopilot.fs
include escalation.fs
include nmea.fs
include devices.fs

\ Simple assertion
: assert ( flag -- )
  if
    ."  OK" cr
  else
    ."  FAIL" cr
    abort
  then
;

\ Floating point approximate equality
: f~ ( F: a b -- flag )
  f- fabs 0.0001e0 f<
;

\ ===== Test Deadband =====

variable db
variable db-os

: test-deadband ( -- )
  ." TEST: deadband" cr

  \ Create two-sided deadband: center=100, tolerance=5
  100.0e0 5.0e0 deadband-create db !

  \ Value at center -> Normal
  db @ deadband-flags 0= assert
  100.0e0 db @ deadband-check NORMAL = assert

  \ Value within tolerance -> Normal
  103.0e0 db @ deadband-check NORMAL = assert
  97.0e0  db @ deadband-check NORMAL = assert

  \ Value at boundary of approaching -> Approaching
  105.1e0 db @ deadband-check APPROACHING = assert
  94.9e0  db @ deadband-check APPROACHING = assert

  \ Value far beyond -> Exceeded
  111.0e0 db @ deadband-check EXCEEDED = assert
  89.0e0  db @ deadband-check EXCEEDED = assert

  \ Test one-sided mode
  100.0e0 5.0e0 deadband-create db-os !
  db-os @ deadband-one-sided

  \ Below center -> Normal (conservation)
  90.0e0 db-os @ deadband-check NORMAL = assert

  \ Slightly above -> Normal within tolerance
  102.0e0 db-os @ deadband-check NORMAL = assert

  \ Above beyond tolerance -> Approaching
  106.0e0 db-os @ deadband-check APPROACHING = assert

  \ Way above -> Exceeded
  112.0e0 db-os @ deadband-check EXCEEDED = assert

  ." PASS: deadband" cr
;

\ ===== Test Autopilot =====

: test-autopilot ( -- )
  ." TEST: autopilot PID" cr

  \ Initialize PID
  0.8e0 0.1e0 0.3e0 pid-set-gains
  10.0e0 max-rudder f!
  2.0e0 heading-tol f!
  pid-reset

  \ Verify gains
  kp f@ 0.8e0 f~ assert
  ki f@ 0.1e0 f~ assert
  kd f@ 0.3e0 f~ assert

  \ Verify limits
  max-rudder f@ 10.0e0 f~ assert
  heading-tol f@ 2.0e0 f~ assert

  \ Verify reset state
  integral f@ 0.0e0 f~ assert
  last-error f@ 0.0e0 f~ assert

  \ Angle difference test
  \ target=90, current=0 -> diff=90
  90.0e0 0.0e0 angle-diff 90.0e0 f~ assert

  \ target=0, current=350 -> diff=10 (shorter around)
  0.0e0 350.0e0 angle-diff 10.0e0 f~ assert

  \ target=350, current=10 -> diff=-20
  350.0e0 10.0e0 angle-diff -20.0e0 f~ assert

  \ Simple PID test: zero initial error should produce zero command
  pid-reset
  45.0e0 45.0e0 0.1e0 pid-update        ( F: rudder on-course )
  fdrop                                    \ drop on-course flag
  0.0e0 f~ assert                          \ rudder should be ~0

  ." PASS: autopilot" cr
;

\ Clear the FP stack
: fp-clear ( -- )
  begin fdepth 0> while fdrop repeat
;

\ ===== Test Escalation =====

: test-escalation ( -- )
  ." TEST: escalation chain" cr

  escalation-init

  \ Initial state
  current-tier@ REFLEX = assert
  escalation-count@ 0 = assert

  \ Escalate once
  1 escalate               \ reason=1
  current-tier@ BACKBONE = assert
  escalation-count@ 1 = assert

  \ Escalate again
  2 escalate
  current-tier@ CORTEX = assert

  \ Escalate again
  3 escalate
  current-tier@ CLOUD = assert

  \ Cannot escalate past CLOUD
  4 escalate
  current-tier@ CLOUD = assert

  \ De-escalate
  de-escalate
  current-tier@ CORTEX = assert

  de-escalate
  current-tier@ BACKBONE = assert

  \ Cannot de-escalate past REFLEX
  de-escalate
  current-tier@ REFLEX = assert

  de-escalate             \ stays at REFLEX
  current-tier@ REFLEX = assert

  ." PASS: escalation" cr
;

\ ===== Test NMEA =====

: test-nmea ( -- )
  ." TEST: NMEA 0183 parsing" cr

  \ Test checksum
  s" $GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47"
  nmea-checksum
  \ checksum after * is 47 hex = 71 decimal
  71 = assert

  ." PASS: NMEA" cr
;

\ ===== Test Devices =====

: test-devices ( -- )
  ." TEST: device abstraction" cr

  devices-init

  \ Check device list is not empty
  device-list @ 0<> assert

  \ Check capabilities
  CAP-GPS any-device-can? assert
  CAP-COMPASS any-device-can? assert
  CAP-AIS any-device-can? assert

  \ Check non-existent capability
  CAP-DISPLAY any-device-can? 0= assert

  \ Find by capability
  CAP-GPS find-device-by-cap 0<> assert
  CAP-DISPLAY find-device-by-cap 0= assert

  \ Find by tier
  REFLEX find-device-by-tier 0<> assert
  CLOUD find-device-by-tier 0<> assert

  ." PASS: devices" cr
;

\ ===== Run All Tests =====

: run-all-tests ( -- )
  cr ." ==============================" cr
  ."  CoCapn Forth Test Suite" cr
  ." ==============================" cr cr
  test-deadband
  test-autopilot
  test-escalation
  test-nmea
  test-devices
  cr ." ==============================" cr
  ."  All tests passed!" cr
  cr
;

\ Run tests on startup
run-all-tests
bye
