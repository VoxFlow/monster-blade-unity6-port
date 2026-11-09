# Building from source

## Requirements

| Item | Version |
|---|---|
| Unity Editor | 6000.3.8f1, with Windows Build Support (Mono) |
| Operating system | Windows 10 or 11, 64-bit |
| Git | With Git LFS installed (the project's binary assets are stored in LFS) |
| Disk space | About 10 GB for the project, the library cache and a build |

## Steps

1. Clone the repository with Git LFS enabled, so that the binary assets are downloaded rather than their pointer files:
   ```
   git lfs install
   git clone <repository URL>
   ```
2. Open the project folder in Unity 6000.3.8f1. The first import takes a while because every asset is imported.
3. Build the player in one of two ways:
   - from the Unity menu: **File > Build Profiles**, select **Windows**, architecture **x86_64**, scripting backend **Mono**, and build
     into an empty folder; or
   - from a command line:
     ```
     Unity.exe -batchmode -quit -projectPath <project folder> -executeMethod MBBuild.BuildWin64Release
     ```
     which writes the player to `Build/Release`.
4. Run `Monster Blade.exe` from the build folder.

## What the build contains

The release build contains only the game and the port's runtime adapters. The development tooling used to build and test the
port (automated play driver, measurement probes, test switches) is not part of the published source.

## Asset bundles

The rebuilt asset bundles are included in `Assets/StreamingAssets/assetbundles` and are copied into the player by the build. They
are generated from `Assets/BundleSources` by a separate editor step, which only needs to run after a bundle's source assets
change:

```
Unity.exe -batchmode -quit -projectPath <project folder> -buildTarget Win64 -executeMethod MBBundles.BuildWin64
```

Each bundle is written under its original path, and each asset keeps the lookup name it had in the original bundle.
