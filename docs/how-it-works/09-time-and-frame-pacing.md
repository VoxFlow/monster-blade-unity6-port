# 9. Time and frame pacing

Much of the game's feel depends on timing: dodge windows, block windows, combo timing, effect delays and cutscene pacing. The port
reproduces the original's timing rules where Unity 6 differs.

## Frame rate

The shipped start-up scene requests 120 frames per second with vertical sync off. On the Android phones the game was made for,
the display limited it to 60. On a desktop the same request runs at about 117 frames per second, and several systems count frames
rather than seconds: breaking free from petrification removes 0.125 seconds of stone per frame of touch, the result-screen score
counters advance a fixed step per frame, and the fusion fades step once per frame. The port therefore caps the frame rate at the
original hardware's 60 on non-Android platforms (measured 59.8 fps) (CD-049).

When the display's refresh rate is a whole multiple of 60 Hz, frames are presented on vertical sync so the 60 frames are evenly
spaced, as they were on the phone. At other refresh rates (75 Hz, 144 Hz) 60 frames cannot be spaced evenly; see
[troubleshooting](../start/troubleshooting.md).

## Coroutine waits

The game uses `WaitForSeconds` extensively. Unity 4.0.1 stored the wake-up time as a single-precision value (start + seconds) and
resumed the coroutine on the first frame whose single-precision frame time reached it; in physics context the start was the fixed
time. Unity 6 compares in double precision from the frame time. In 25 of 72 measured cases Unity 6 resumed one frame later. The
port's wait helper applies the Unity 4.0.1 rule (CD-059). Both rules were confirmed against measurements of each engine (72 of 72
cases each).

## Delayed destruction

`Destroy(obj, t)` removes the object at the original engine's frame (see [animation and effects](05-animation-and-effects.md),
CD-053).

## Loading screens and start-up

Two timing adjustments concern loading, where a PC is much faster than the original phones:

- **Loading screens** stay in clear view for at least four and a half seconds. The loading screen goes up under a fade to black and is
  seen only between the first scene's fade-in and the next fade-out; on a PC that gap was a few frames, so the tip was hidden or
  flickered. The next fade-out now waits until the tip has been fully visible for that long (CD-063).
- **The title fade-in** waits until the offline server's start-up reply has arrived, so the first frames do not show the title's
  placeholder text before the fade (CD-062).

Both are labelled quality fixes, not original behaviour.

## Sorting

Several game systems sort lists whose elements can compare equal (for example user-interface widgets of equal depth). The order of
equal elements depends on the sorting algorithm of the runtime library. The original's Mono 2.x used a quicksort; Unity 6's
runtime uses an introspective sort, which orders equal elements differently. The port sorts through a copy of the original
algorithm (CD-055), verified to reproduce the original engine's output row for row.
