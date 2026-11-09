# Testing and validation

The port was validated at four levels. Each level answers a different question, and each has limits that are stated below.

## 1. Static parity: does the port contain what the original contains?

| Check | Method |
|---|---|
| Code parity | Every recompiled method compared with the shipped IL. Residual differences are classified; none is behavioural |
| Asset parity | Exported and rebuilt assets compared with the shipped serialised data, field by field, by original identifier |
| Bundle parity | Rebuilt bundles checked for membership and lookup names against the original bundles |
| Import parity | Texture formats, mipmaps and audio encodings compared with the shipped bytes |

**Limit:** equal data does not prove equal behaviour when the engine interprets the data differently.

## 2. Engine semantics: does Unity 6 do what Unity 4.0.1 did with the same input?

Small test programs ("probes") were built from the same source with Unity 4.0.1 and Unity 6 and run on both. Each probe measures
one behaviour (animation sampling, cross-fades, particle emission, delayed destruction, coroutine timing, sorting, web replies,
callback order) and compares the logs. Probes carry a positive control, a case that must differ, so a blank result cannot pass
by accident. See the [probe catalogue](../technical/probe-catalogue.md).

**Limit:** the Unity 4.0.1 test builds run on Windows with Direct3D, not on an Android phone with OpenGL ES. Engine-side values
match, but GPU output of the original shaders had to be checked against an analytic model of the shipped GLES programs instead.

## 3. Play-through: does the game run from start to finish?

An automated driver played the game through its own input path: taps and swipes delivered as touch, attacks answered with
timed dodges and blocks. Runs covered the tutorial, the story campaign and the six god-beast events, many times with different
random seeds. Each run logged every exception, stall, unanswered attack and menu action. A watchdog stopped a run that made no
progress for two minutes and recorded the state.

Runs were made both at real speed and at a fixed-step fast mode. Fast mode is repeatable frame for frame, but it is not a
substitute for real speed: one failure (Aramaz's final-phase damage margin) appeared only at real speed.

**Limit:** a driver plays more consistently than a person and exercises a narrower set of situations. Its findings show that the
game *can* be completed and where it breaks; they do not show that every situation a player meets behaves as in the original.

## 4. Comparison with the original

Recordings of the original game were compared with the port screen by screen: timings, rewards, menu flows, effects and camera
motion. Several surrogate server rules were corrected to match what the recordings showed.

**Limit:** recordings cover only what was recorded, at the recording's frame rate and resolution.

## Testing the tests

The verification tools themselves were tested by deliberately planting faults and checking that each tool reported them. Several
tools that had never been exercised this way were found to pass faults silently and were corrected before their results were
trusted.

## What is not covered

- Android: the port targets Windows only.
- Hardware other than the development machine's GPU vendor and display configuration.
- Online features, which the original no longer offers.
