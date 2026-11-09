# Porting a Unity 4 game to Unity 6

A checklist distilled from this port, ordered roughly as the work happens. Most items apply to any Unity 4.x mobile title.

## Before writing code

1. **Identify the exact build.** Record version, version code and file hashes of the package you port, and keep reference copies
   of other releases clearly separated.
2. **Find all the data.** Check whether gameplay data ships in the package or was downloaded from a server. Server-delivered data
   that is lost cannot be reconstructed; look for device caches, archived patch files or community captures early.
3. **Catalogue the server calls.** List every service call, its request and the reply fields the client reads. The client's own
   parser is the specification.
4. **Write down a rule for differences.** Decide in advance that every difference from the original needs a reason, evidence and a
   register entry.

## Export and recompile

5. **Keep identifiers.** Export with GUIDs and file IDs preserved, so references survive and every asset maps to its original.
6. **Remove exporter artefacts.** Generated assembly-info files, injected runtime libraries, re-saved plugin DLLs.
7. **Expect API-updater damage.** Unity's automatic API updater can rename local variables that shadow old shortcut properties.
   Audit its changes.
8. **Guard Android-only code.** Java plugin calls must answer as an unavailable service on desktop, not throw.
9. **Check code parity.** Compare recompiled methods with the shipped IL and classify every residual difference.

## Assets

10. **Rebuild asset bundles** under their original paths and lookup names; replace the removed main-asset concept.
11. **Store compressed legacy clips decoded**, and check for single-key curves.
12. **Convert legacy particles** and emulate the rules that differ; do not trust a static comparison that scores the dropped
    components as equal.
13. **Recover shaders from the shipped programs** and ship Unity 4's built-in shaders and primitives under their original names.
14. **Match import settings** to the shipped textures, mipmaps and audio, measured on the shipped bytes.

## Engine behaviour

15. **Triage every Unity API the game calls** for behaviour changes, then measure the candidates on both engine versions. See
    [oracle testing](oracle-ab-testing.md) and [engine differences](../technical/engine-differences.md).
16. **Check the timing rules:** coroutine waits, delayed destruction, web request completion.
17. **Check the runtime library:** sort order of equal elements, culture, number formatting.

## Platform

18. **Cap the frame rate** to the original hardware's rate if any system counts frames.
19. **Replace the Android lifecycle:** save where Android would have paused the app.
20. **Store PlayerPrefs like the original platform did** if the game depends on their behaviour.
21. **Deliver PC input through the game's own touch path**, so its gesture recognition and button rules apply unchanged.

## Testing

22. **Automate a full play-through** with the game's own input path, and run it at real speed as well as in a fast mode.
23. **Compare against recordings of the original.** Many late "defects" turn out to be original behaviour; many others turn out to
    depend on hardware speed rather than on the engine.
