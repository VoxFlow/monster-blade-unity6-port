# Troubleshooting

## Motion looks uneven or stutters slightly

On the phones it was made for, the original ran at the display's 60 frames per second; the port caps the frame rate at 60 for the same timing (compatibility register CD-049). On a display
whose refresh rate is not a multiple of 60 (for example 75 Hz or 144 Hz), 60 frames cannot be shown at even intervals, which
appears as a slight judder in camera movement and screen shakes. Setting the display to 60 Hz, or 120 Hz, gives even motion.

## The game does not start or closes immediately

- Make sure the folder was fully extracted and that `Monster Blade_Data` sits next to `Monster Blade.exe`.
- Do not run the game from inside the archive.
- Update the graphics driver; the player requires Direct3D 11 or later.
- Unity writes a log to `%USERPROFILE%\AppData\LocalLow\Nubee Pte Ltd\Monster Blade\Player.log`. Include it when reporting a
  problem.

## The screen shakes strongly in some cutscenes

This is original behaviour. The friend-assist stun and attack cutscenes and several boss entrances use strong, long camera
shakes that are part of their shipped data. See [shipped behaviour kept](../fidelity/shipped-behaviour-kept.md).

## Battle Points drop by 3 when an event starts on Hard

This is original behaviour of the client's display. The client subtracts the Easy price from its own copy of the count; the
correct amount is deducted by the server and the display is corrected with the next update from it. See
[shipped behaviour kept](../fidelity/shipped-behaviour-kept.md).

## Starting over

Close the game and delete the profile and settings files listed in [download and run](download-and-run.md#save-data).

## Reporting a difference from the original

Open an issue with: what you did, what happened, what the original does (a video of the original game is the most useful
evidence), and the `Player.log` from the session.
