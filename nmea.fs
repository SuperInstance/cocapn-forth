\ nmea.fs — NMEA 0183 sentence parsing
\
\ NMEA parsing in Forth is beautiful because it's character-by-character
\ on the stack. No regex. No string objects. Just chars and arithmetic.

\ ===== NMEA Checksum =====
\ XOR all bytes between $ and * (exclusive of both tokens).
\ Uses DO/LOOP (not ?do) for iteration.

: nmea-checksum ( addr len -- checksum )
  \ skip leading $  ( addr len -- addr+1 len-1 )
  swap 1+ swap 1-
  0 -rot                                 \ checksum=0, then ( end-addr start-addr )
  bounds                                 ( end-addr start-addr )
  do
    i c@ dup [char] * = if
      drop leave
    then
    xor
  loop
;

\ ===== Hex digit to value =====
: hex>nibble ( c -- n )
  dup [char] A < if
    [char] 0 -
  else
    [char] A - 10 +
  then
;

\ ===== Parse coordinate (DDMM.MMMM to decimal degrees) =====
: parse-coordinate ( addr len -- F: dec-degrees )
  >float 0= abort" parse-coordinate: bad float"
  fdup 100.0e0 f/ floor
  fdup 100.0e0 f* f- 60.0e0 f/ f+
;

: parse-lat ( addr len -- F: dec-degrees ) parse-coordinate ;
: parse-lon ( addr len -- F: dec-degrees ) parse-coordinate ;

\ ===== Parse GGA sentence =====
\ $GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47

: parse-gga ( addr len -- F: lat lon quality sats hdop alt )
  2drop 2drop fdrop fdrop fdrop fdrop
  0e0 0e0 0 0 0e0 0e0
;
