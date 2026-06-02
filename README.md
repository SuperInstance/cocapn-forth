# cocapn-forth

CoCapn in Forth — the language of bare metal. When the ESP32 has 4KB and the ocean has no patience.

## Why Forth?

Forth runs on spacecraft. The Philae lander that touched down on comet 67P ran Forth. Forth runs in your microwave. Forth runs in systems with 1KB of RAM and a processor that measures its clock speed in kilohertz.

Forth is what you use when you have almost nothing and need to do something anyway.

For CoCapn, Forth is Layer 0 — the code that runs when everything else has failed. When the Pi is offline, when the Jetson is rebooting, when the cloud is unreachable, the ESP32 still steers. And on that ESP32, Forth is the lightest possible expression of the deadband, the PID, the escalation chain.

## What's Here

| File | Purpose |
|------|---------|
| `deadband.fs` | Deadband trigger — relative tolerance, one-sided conservation checks |
| `autopilot.fs` | PID heading controller — anti-windup, rudder limiting, heading wraparound |
| `escalation.fs` | Tier escalation chain — Reflex → Backbone → Cortex → Cloud |
| `nmea.fs` | NMEA 0183 sentence parser — GGA, checksum verification, coordinate conversion |
| `devices.fs` | Device tier and capability model |
| `cocapn-test.fs` | Test suite — exercises all modules |
| `Makefile` | Build with `gforth` |

## The Stack Philosophy

Forth has no variables. Forth has no objects. Forth has no heap allocation. Forth has the stack.

The deadband in Forth is words that push and pop floats. The PID is words that update global state (because Forth globals ARE the minimal state machine). The NMEA parser reads characters one at a time and pushes results onto the stack.

This is not a limitation. This is clarity. When you can't hide behind objects, you see the problem for what it actually is.

### Deadband Example

```forth
\ Create a deadband centered at 100 with 5% tolerance
100.0e 0.05e deadband-create
\ Check a value
dup 97.0e swap deadband-check  \ → 0 (NORMAL)
dup 107.0e swap deadband-check \ → 2 (EXCEEDED)
```

Four numbers. No object. No heap. No garbage collector. Just the truth about whether you're on course.

### PID Autopilot Example

```forth
\ Set gains
0.8e 0.1e 0.3e pid-set-gains
\ Update with current heading, target heading, timestep
45.0e 90.0e 0.1e pid-update
\ → rudder command on stack, on-course flag
```

## Line Count Comparison

The same deadband across languages:

| Language | Lines | What you get |
|----------|-------|-------------|
| Forth | ~40 | Works on a comet |
| C | ~60 | Works everywhere |
| Zig | ~80 | Compile-time verification |
| Ada | ~100 | Provable correctness |
| Rust | ~120 | Memory safety |
| Java | ~400 | Enterprise architecture |

Forth is the shortest because Forth has the fewest assumptions. Every assumption you add is a line of code.

## Running

```bash
make test    # Run test suite with gforth
make deadband # Run deadband tests only
```

Requires [Gforth](https://www.gnu.org/software/gforth/) (available in most package managers).

## The Lesson

Forth teaches that most of what we build is scaffolding around a very simple core. The deadband is: "is this value close enough to center?" The PID is: "correct proportional to the error." The escalation is: "if I can't handle it, ask someone who can."

These ideas don't need objects. They don't need classes. They don't need heap allocation. They need arithmetic and comparison, which is what the stack gives you.

The Forth implementation of CoCapn is not a toy. It's the truth of the system, stripped to its bones.

## License

MIT
