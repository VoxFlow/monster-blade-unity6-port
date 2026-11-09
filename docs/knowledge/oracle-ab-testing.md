# Measuring engine behaviour with oracle builds

When a port must reproduce how an old engine version behaved, the most reliable reference is the old engine itself. An "oracle"
is a small test program built with the original engine version that measures one behaviour and writes the result to a log. The
same program is built with the target engine, and the two logs are compared.

## Method

1. **One behaviour per probe.** Measure one thing, for example how `Destroy(obj, t)` is scheduled, with a grid of inputs (delays,
   call sites, frame rates).
2. **Same source on both sides.** Share the probe code between the two projects. Keep it ASCII-only and avoid constructs that the
   older compiler treats differently: the Unity 4 compiler read a source file without a byte-order mark in the system code page,
   and folded `-0f` to `+0`.
3. **Fixed timing.** Run at a fixed frame rate (`Time.captureFramerate`) where the old engine honours it; otherwise log the frame
   time on every line and compare by time instead of frame number.
4. **Compare inputs first.** Check that both logs contain the same input rows before comparing results. A row present in only one
   log means the probes differ, not the engines.
5. **Include a positive control.** Every probe needs a case that must differ (or must produce output). A comparison that shows
   "no difference" is only meaningful if the probe can show one.
6. **Classify the result.** Same, different (with the exact rule of each engine), or not covered.
7. **Fit the rule, then verify it.** When behaviour differs, derive each engine's rule and check that it reproduces every measured
   case of that engine before writing a bridge.

## Limits

- The oracle runs the old engine on desktop graphics, not on the original phone. Engine-side behaviour (timing, ordering, data
  interpretation) transfers; GPU output of mobile shaders does not.
- Licensing and installation of old engine versions can be fragile; keep the oracle setup reproducible.

## What it settled in this port

Coroutine resume timing, delayed destruction, web replies, sorting, animation sampling and cross-fades, particle rules, component
lookup on inactive objects and callback order. Several assumed engine differences were disproved the same way. See the
[probe catalogue](../technical/probe-catalogue.md).
