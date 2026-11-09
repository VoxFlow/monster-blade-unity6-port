# 10. Menus and user interface

The user interface is built with the version of NGUI shipped in the game. The library is kept as shipped, and its behaviour on
the original Android platform is preserved where it branches by platform.

## Start-up

| Step | Original | Port |
|---|---|---|
| Splash | Unity 4 Pro on Android showed the game's splash image while the first scene loaded, scaled to fit, without a Unity logo | Unity 6 splash configured to the same image, black background, no Unity logo, no blur (CD-044) |
| Expansion check | Checked for the expansion file and downloaded it if missing | Reports the data as present (CD-003) |
| Title | Faded in from black | Holds black until the offline server's start-up reply, then fades as the original (CD-062) |

## Platform branches in the interface code

- **Half-pixel offset.** NGUI shifts widgets by half a pixel on Windows to correct for Direct3D 9. The original never ran on
  Windows, so the shift is disabled (CD-023).
- **Android back button.** Mapped to `Esc` on PC; the game's own back-button handling is unchanged.
- **Touch.** The game's touch handling receives the mouse as one finger (CD-025, CD-029).

## Menu bar slide

Some screens slide the top and bottom bars in from their `Start` method, and a few start the same slide twice. On the original
phones the second request arrived after the first slide had finished and did nothing. On a PC the screen loads within a few
frames, the second request arrives during the first slide, and the animation library cancels both, leaving the bars stopped part
way. Measurements show the animation library behaves the same in both engines; the difference is load speed. The port lets an
identical running slide finish (CD-026).

## Animated transparency

One helper animates a widget's transparency through an animation clip that drives a private field. It assumes the widget is
redrawn every frame anyway. On the original hardware the first frames were slow enough that this held; on a PC the widget could
be drawn once with its starting transparency and not again. The port marks the widget as changed when the animated value moves,
so the authored fade is drawn on any hardware (CD-064). Measurements on both engines showed identical code paths; the difference
is hardware timing.
