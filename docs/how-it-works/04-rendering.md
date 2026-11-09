# 4. Rendering

The original targets OpenGL ES 2.0 on Android. The port renders with Direct3D 11 on Windows. The art, materials, lighting and
camera setup are the original's; the work in this area is about making the same data produce the same image on a different
graphics API and engine version.

## Shaders

The game uses two kinds of shaders:

- **Game shaders** (character, monster, environment, effects). Unity 4 compiled them for GLES 2.0. The exporter cannot recover
  their source, so the shipped GLES programs were translated back to shader code one to one and are kept under their original
  ShaderLab names.
- **Unity 4 built-in shaders** (for example `Diffuse`, particle shaders). Unity 6 has shaders with the same names, but they compute
  lighting differently: compared against the original engine, Unity 6's `Legacy Shaders/Diffuse` differed by up to 103 levels
  (mean 48) per channel. The shipped Unity 4 programs were translated in the same way and stored as project shaders under the
  original names (register CD-024).

Two translation details matter for fidelity: the shipped built-in diffuse shader doubles direct light and evaluates ambient light
in gamma space; both are kept.

## Platform differences handled at the API level

| Difference | Treatment | Register |
|---|---|---|
| The game builds a combined character texture in ETC1 format, which Direct3D 11 cannot create | Created as RGB24 on non-Android platforms (same channels) | CD-013 |
| NGUI shifts all widgets by half a pixel on Windows players (a Direct3D 9 correction); the original never ran on Windows, so it never applied | The shift is disabled, giving the Android result | CD-023 |
| A soft-particle shader fades against a ground plane computed from the camera's projection matrix; Direct3D clip space differs from OpenGL's | The plane is built from the GPU projection matrix | CD-033 |
| Materials without a `_Color` property returned black in Unity 4 when read; Unity 6 logs an error | The read returns the Unity 4 value without the error | CD-014 |
| The game's user layer named "UI" (layer 8) collides with Unity 6's built-in layer 5 "UI", and lookups by name return the lowest match | Lookups resolve to the game's layer | CD-050 |

## Camera draw order

Several screens use more than one camera at the same depth. A same-source test on both engines showed that equal-depth cameras
draw in the order they were enabled in both Unity 4.0.1 and Unity 6, so no change was needed. (An earlier assumption that the
order follows the camera list was disproved by the same test.)

## Quality settings

The release has one quality level, the original build's settings. A development-only level with full-resolution textures and
multisampling (CD-041) showed no visible difference and is not part of the release.

## What is not changed

No post-processing, no shadows and no lighting changes are added. The art was authored for vertex-lit shaders without shadows,
and the port shows it that way.
