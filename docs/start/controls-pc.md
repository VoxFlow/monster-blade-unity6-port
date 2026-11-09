# PC controls

The original game is controlled by touch only. On Windows, the mouse acts as one finger and the keyboard provides shortcuts for
the battle controls. The mapping is a port addition (compatibility register CD-029). The game's own touch handling is unchanged:
every key press is delivered to it as a touch, so button locks, cooldowns and tutorial restrictions apply exactly as they do for
a tap.

## Mouse

| Touch gesture | Mouse |
|---|---|
| Tap | Left click |
| Swipe (sword strike, scrolling a list) | Hold the left button and drag |
| Hold (dodge, block, cutscene speed-up) | Hold the left button on the on-screen control |

## Keyboard (battle only)

| Action | Key | What the game receives |
|---|---|---|
| Sword strike | `W` `X` `A` `D` | A swipe through the middle of the screen toward the key: `W` upward, `X` downward, `A` to the left, `D` to the right |
| Diagonal sword strike | `Q` `E` `Z` `C` | A diagonal swipe toward that key's corner of the keyboard layout: `Q` up-left, `E` up-right, `Z` down-left, `C` down-right |
| Dodge left / right | `Left arrow` / `Right arrow` (hold) | The dodge button held |
| Block | `Space` (hold) | The block button held |
| Super attack | `Up arrow` | A tap on the super button |
| Friend assist | `F` | A tap on the assist button |
| Speed up a cutscene | `Tab` (hold) | The speed-up button held |

The eight strike keys follow a keyboard map that already exists, unused, in the shipped code
(`PlayerController.InterpretKeyboardInput`).

## Menus

| Action | Key |
|---|---|
| Back (the Android back button: pause in battle, close or go back in menus) | `Esc` |

Outside battle, menus are operated with the mouse; the keyboard keeps only the user interface library's own key handling.

## Notes

- The on-screen buttons appear only when the game shows them, as on the phone. A key whose button is hidden or locked does
  nothing.
- Because strikes are generated as swipes, they go through the game's own gesture recognition.
