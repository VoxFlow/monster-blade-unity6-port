# 2. Assets and asset bundles

## Exporting the original assets

The original scenes, prefabs and assets were exported with AssetRipper. Exporter artefacts that do not belong to the game were
removed: generated `AssemblyInfo` files, injected runtime assemblies, and re-saved copies of shipped plugin libraries (replaced by
the shipped, byte-identical libraries with their identifiers kept).

Asset identifiers are kept throughout. A prefab exported from the original references its materials, meshes and clips by the
same GUID and file ID as in the original, so every reference survives the export unchanged and any port asset can be traced back
to its original object.

## Asset bundles

The original ships 352 Unity 4.0 asset bundles in its expansion file. Unity 6 cannot load Unity 4 bundles, so they are rebuilt:

1. Each bundle's contents are exported as a group that keeps the bundle's membership.
2. A build-time step rebuilds Unity 6 bundles at the original bundle paths, with each asset registered under the lookup name it
   had in the original bundle (`m_Container`). The game's loader (`DLC`, `BundleDownloadHandle`) therefore loads them without
   changes.
3. Where Unity 6 returns no main asset for a bundle in a case where Unity 4 did, the shipped main asset is loaded by its original
   lookup name (register CD-009).

The game requests bundles through `WWW` with local file paths. Unity 6 needs a well-formed `file://` URI for paths with drive
letters and spaces (CD-010). Unity 6 can also complete a local `WWW` inside its constructor, while on the original player a
request never completed in the same frame; the game's loader relies on the later completion, so the port waits one frame after
creating the request (CD-011).

## Animation clips

663 animation clips were stored inside the original FBX models and are loaded by the game as `<Model>@<Clip>`. They are stored as
separate clip assets next to their model under exactly those names.

Compressed legacy clips are stored uncompressed with their decoded curves (CD-022). Unity 6 re-compresses them differently from
Unity 4 when a project is built, which measurably changes poses between keys; storing the decoded curves reproduces the original
values to within 0.002 degrees.

Clips that contain curves with a single key were corrected at the data level (CD-045): Unity 6 applies a single-key curve for the
whole clip where Unity 4 did not, which made, for example, the ground of one boss entrance transparent. The 146 affected curves
were removed from the source clips, the originals are preserved, and the step can be re-run.

## Textures and audio import

Import settings were adjusted so that Unity 6 produces the same texture formats, mipmaps and audio as the original build
(CD-039). Where Unity 6 cannot rebuild a texture's shipped mipmaps from its top level, the shipped levels are stored directly.

## Built-in assets

The game references Unity 4 built-in shaders, default materials and primitive meshes (sphere, cylinder, default particle). Unity 6
has different built-ins, so the shipped versions are stored as project assets under their original names (CD-024, CD-034).

## The gameplay parameter tables

The tables that define monsters, attacks, items, cities and events were never part of the APK or OBB; the client downloaded them
from the server into its cache. The port seeds that cache with original files, bytes unchanged, and answers the client's patch
check so that it accepts them (CD-006). The client's own loaders read them exactly as before.
