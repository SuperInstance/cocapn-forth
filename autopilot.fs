\ autopilot.fs — PID controller in Forth
\ Pure stack-based using fvariable temp storage.

fvariable kp
fvariable ki
fvariable kd
fvariable integral
fvariable last-error
fvariable max-rudder
fvariable heading-tol

fvariable pid-dt
fvariable pid-error
fvariable pid-p
fvariable pid-i
fvariable pid-d

: pid-set-gains ( F: p i d -- ) kd f! ki f! kp f! ;
: pid-reset ( -- ) 0e0 integral f! 0e0 last-error f! ;

: pid-init ( F: p i d max-rud tol -- )
  pid-set-gains max-rudder f! heading-tol f! pid-reset
;

: angle-diff ( F: target current -- F: diff )
  f- fdup -180e0 f< if 360e0 f+ then
  fdup 180e0 f>= if 360e0 f- then
;

\ Compute each PID term independently, store in fvariables, then sum.
: pid-update ( F: current target dt -- F: rudder on-course )
  \ Save dt
  fdup pid-dt f!                 ( F: curr target dt )

  \ Compute error = angle-diff(target, current)
  frot frot                      ( F: dt curr target )
  fswap fswap                    ( F: dt target curr )
  angle-diff                     ( F: dt error )

  \ Save error, also keep a copy
  fswap                          ( F: error dt )
  fdup pid-error f!              \ save error

  \ --- P term: kp * error ---
  pid-error f@ kp f@ f*          ( F: dt P )
  fdup pid-p f!

  \ --- Update integral: I += error * dt ---
  pid-error f@ pid-dt f@ f*      ( F: dt P err*dt )
  integral f@ f+                 ( F: dt P new-int )
  integral f!

  \ --- I term: ki * integral ---
  integral f@ ki f@ f*           ( F: dt P I )
  fdup pid-i f!

  \ --- D term: kd * (error - last_error) / dt ---
  pid-error f@ last-error f@ f-  ( F: dt P I err-diff )
  kd f@ f*                       ( F: dt P I dv )
  pid-dt f@ f/                   ( F: dt P I D )
  fdup pid-d f!

  \ Save error for next iteration
  pid-error f@ last-error f!

  \ --- PID sum: P + I + D ---
  fdrop                          ( F: dt P I )
  fdrop                          ( F: dt P )
  fdrop                          ( F: dt )
  fdrop                          ( F: )
  pid-p f@ pid-i f@ f+ pid-d f@ f+ ( F: rudder-raw )

  \ --- Clamp to max-rudder ---
  max-rudder f@ fdup fnegate     ( F: raw max -max )
  frot frot                      ( F: max raw -max )
  fmax fmin                      ( F: clamped )

  \ --- On-course check: |error| < heading-tol ---
  pid-error f@ fabs heading-tol f@ f<  ( F: clamped on-course? )
;
