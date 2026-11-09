# Failure ledger

Selected blockers from the project's ledger: what went wrong, what was wrongly assumed, and what actually caused it. The selection
favours failures whose cause is likely to recur in other Unity 4 ports.

| Area | Symptom | Wrong assumption | Actual cause | Resolution |
|---|---|---|---|---|
| Data | The gameplay tables could not be found anywhere in the package | The shipped build contains all game data | The tables were server-delivered and cached on the device | A device cache found through an archived forum thread; seeded into the client cache unchanged |
| Boot | The first launch stopped with a null reference in the expansion-file check | Unity's Android Java classes throw on Windows | The game's own plugin wrapper assumed Android paths | Platform guard returning the desktop answer |
| Boot | The game hung after its first server call with no error | A boot hang means a crash | The client waits for a reply forever; the call was simply unanswered | Offline server |
| Bundles | The character database loaded empty from a rebuilt bundle | Unity 6 cannot store such assets | Data loss at export time | Re-export with the bundle grouping preserved |
| Bundles | A bundle's main asset came back as the wrong object, then as null | A lookup name identifies one object | Unity 5 and later bundles have no main asset | Load the shipped main asset by its original lookup name |
| Animation | All clips of one monster failed to load | The clips were missing from the export | The clips existed under a different name than the game requests | Store clips under the `<Model>@<Clip>` names the game uses |
| Camera | A null reference when the battle camera changed | Camera data was missing | Unity's API updater renamed a local variable that shadowed a property | Audit and restore the affected locals |
| Rendering | Translated shaders failed to compile | The translator's rename table was complete | Several built-in uniforms had no mapping | Complete the mapping; verify every shader compiles |
| Particles | Eight legacy particle systems were silently dropped | Static parity scores equal data as equal output | Unity 6 removed the legacy components when loading the prefabs | Convert to Shuriken with the legacy rules emulated |
| Particles | Trail widths followed an S-curve in all trails | Old-format data equal to the original means equal output | Unity 6 interprets the width curve differently | Reproduce Unity 4's linear taper, measured on the original engine |
| Input | Scripted dodges and blocks never registered | Sending the button's press message holds the button | The battle screen releases any button not backed by a real touch | Deliver input as touch through the game's own path |
| Menus | The bottom bar was missing and a menu became unreachable | An engine timing difference | Two identical slide animations cancelled each other because the PC loads too fast | Let the running slide finish |
| Saves | Every restart replayed the tutorial | Tutorial progress is not saved | The game saves on Android's pause, which a Windows quit never triggers | Run the pause save on quit |
| Lineage | Several "shipped" measurements were wrong | The reference folder holds the base build | It held an earlier release (1.3.1) | Every tool names and hashes the build it reads |

## Patterns

- **Silence is not success.** Several failures produced no error at all: a hang, an empty asset, a dropped component. Checks that
  look only for errors miss them.
- **Equal data is not equal behaviour.** Many Unity 4 to Unity 6 differences are in how the engine interprets unchanged data.
- **Speed changes behaviour.** A PC loads and runs much faster than the original phones, and code written for slow devices can
  behave differently when it is not slow. See [lessons learned](lessons-learned.md).
