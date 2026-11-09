// MBBuild - Editor-only release build for the Monster Blade Unity 6 port (Windows 64-bit, Mono).
// Menu: Monster Blade > Build Windows Release. Batch:
//   Unity.exe -batchmode -quit -projectPath <project> -executeMethod MBBuild.BuildWin64Release
// The player is written to <project>/Build/Release as "Monster Blade.exe" (outside Assets; not part of the repository).
// The application icon is Assets/Icon/icon.png, set as the default icon before each build.
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MBBuild
{
    [MenuItem("Monster Blade/Build Windows Release")]
    public static void BuildWin64Release()
    {
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string outDir = Path.Combine(root, "Build", "Release");
        Directory.CreateDirectory(outDir);
        SetIcon();
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = Path.Combine(outDir, "Monster Blade.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
            extraScriptingDefines = new[] { "MB_RELEASE" }
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log("[MBBuild] " + report.summary.result + ", " + report.summary.totalErrors + " errors, " + report.summary.totalSize + " bytes");
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }

    [MenuItem("Monster Blade/Set Application Icon")]
    public static void SetIcon()
    {
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Icon/icon.png");
        if (icon == null)
        {
            Debug.LogWarning("[MBBuild] Assets/Icon/icon.png not found; the build uses Unity's default icon");
            return;
        }
        PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
        AssetDatabase.SaveAssets();
    }
}
