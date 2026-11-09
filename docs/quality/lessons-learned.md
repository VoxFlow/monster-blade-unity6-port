# Lessons learned

## Evidence

1. **Name the build you measure.** A measurement taken from the wrong release looks exactly like a measurement from the right one.
   Every tool should state which build it read and verify it by hash.
2. **Measure the original engine, not your memory of it.** Several engine "facts" that seemed certain were wrong when tested on
   Unity 4.0.1 itself. A small test program built with the original engine version settles a question in minutes.
3. **Build both sides of a comparison from the same source, and check the inputs first.** Different compilers can read the same
   source differently (encoding, constant folding). Compare the input rows before comparing the outputs.
4. **Every comparison needs a positive control.** A test that reports "no difference" must also show that it can detect one.
5. **Distinguish proven, supported and inferred.** A conclusion from code reading is not a measurement, and a measurement on a
   desktop build is not a measurement on the phone. Label each claim, and correct overclaims as soon as they are noticed.

6. **Search community archives early.** Data that only lived on dead servers can survive in device caches shared on forums, in
    archived threads and in player wikis. A plain web search (`web archive <game>`) found the decisive files here.

## Fidelity

7. **Fix the cause, not the number.** Tuning a value until the output matches hides the reason it differed. Every accepted
   difference in the port has a cause traced to code, data or engine behaviour.
8. **Many reported defects are original behaviour.** Before changing anything, check whether the original code produces the same
   result. In this project, many defects reported in late testing were the original game's own behaviour.
9. **Speed is a variable.** Code written for slow phones can depend on being slow: two animations started a frame apart, a fade
   that is redrawn only because frames are slow, a loading screen that is always visible. A PC changes these without any engine
   difference.
10. **Equal data is not equal behaviour.** The engine's interpretation of unchanged data (animation compression, single-key curves,
   width curves, built-in shaders) changed between versions.

## Testing

11. **Silence is not success.** Hangs, empty assets and dropped components produce no errors. Watchdogs, progress checks and
    content checks find what error logs do not.
12. **Fast mode is not real speed.** Fixed-step fast runs are repeatable and cheap, but timing-dependent outcomes must be confirmed
    at real speed.
13. **Read the log before blaming a cause.** An apparent hit that killed the player was a timer setting health to zero; the log
    showed no collision at that moment.
14. **Test the tests.** Plant a fault in each verification tool once and confirm it reports the fault.

## Process

15. **Write findings down before building on them.** A register of differences, a list of disproved hypotheses and a failure
    ledger prevented the same investigation from being repeated.
16. **One question per experiment.** Probes that measure one behaviour each are cheaper to build and easier to trust than broad
    comparisons.
