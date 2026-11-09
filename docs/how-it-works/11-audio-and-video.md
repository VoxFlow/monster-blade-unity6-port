# 11. Audio and video

## Audio

The game's sound manager, playlists and voice logic are original. Audio was compared between the original engine and Unity 6 with
the same clips and settings.

| Difference | Treatment | Register |
|---|---|---|
| Unity 4 stored "3D" as a property of the audio clip and applied it on any source; Unity 6 decides it on the audio source, default 2D. Five clips in 1.3.3 carry the flag (the tutorial ambience and four attack voices) | Sources playing those clips are set to 3D, as the clip flag would | CD-047 |
| Unity 4's import settings produced different encodings and loading behaviour from Unity 6's defaults | Import settings matched to the original's result | CD-039 |
| The original requested 512-sample buffers, three of them; Unity 6 does not allow setting the buffer count and uses four | Buffer size kept; one residual buffer of latency remains | CD-048 |

Warnings such as "Playlist X does not exist" appear in the log during play. They come from the shipped data: the playlists are
referenced but not defined in 1.3.3. They are original and harmless.

## Video

The game plays an intro movie in Theora format through an Android-only native plugin that decoded the video and uploaded its
picture planes to textures. Unity's own video player does not accept Theora on Windows. The port provides the same plugin
interface on Windows with a reference Theora decoder and uploads the planes from C# (CD-035). The game's movie code is unchanged.
