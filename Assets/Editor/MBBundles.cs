// Editor: rebuilds the original asset bundles for Unity 6 under their original paths and lookup names, from
// Assets/Editor/MBBundleMap.txt into Assets/StreamingAssets. Batch: Unity.exe -batchmode -quit -projectPath <project>
// -buildTarget Win64 -executeMethod MBBundles.BuildWin64
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class MBBundles
{
    public static void BuildWin64()
    {
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string work = Path.GetFullPath(Path.Combine(root, "..", "Build", "bundles_win64"));
        Directory.CreateDirectory(work);
        var groups = new Dictionary<string, List<string[]>>();
        foreach (string line in File.ReadAllLines(Path.Combine(Application.dataPath, "Editor", "MBBundleMap.txt")))
        {
            string[] p = line.Split('\t');
            if (p.Length < 3 || p[0].Length == 0) continue;
            if (!groups.TryGetValue(p[0], out var list)) groups[p[0]] = list = new List<string[]>();
            list.Add(p);
        }
        var builds = new List<AssetBundleBuild>();
        int missing = 0;
        foreach (var kv in groups)
        {
            var assets = kv.Value.Where(p => File.Exists(Path.Combine(root, p[1]))).ToList();
            missing += kv.Value.Count - assets.Count;
            builds.Add(new AssetBundleBuild
            {
                assetBundleName = kv.Key,
                assetNames = assets.Select(p => p[1]).ToArray(),
                addressableNames = assets.Select(p => p[2].Length > 0 ? p[2] : Path.GetFileNameWithoutExtension(p[1]).ToLowerInvariant()).ToArray()
            });
        }
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(work, builds.ToArray(),
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneWindows64);
        int copied = 0, withDeps = 0;
        if (manifest != null)
        {
            foreach (string b in manifest.GetAllAssetBundles())
            {
                if (manifest.GetAllDependencies(b).Length > 0) withDeps++;
                string dst = Path.Combine(Application.streamingAssetsPath, b);
                Directory.CreateDirectory(Path.GetDirectoryName(dst));
                File.Copy(Path.Combine(work, b), dst, true);
                copied++;
            }
        }
        File.WriteAllText(Path.Combine(work, "build_result.txt"),
            "result=" + (manifest != null ? "Succeeded" : "Failed") + "\nbundles=" + builds.Count + "\ncopied=" + copied
            + "\nwith_dependencies=" + withDeps + "\nmissing_assets=" + missing + "\n");
        AssetDatabase.Refresh();
        EditorApplication.Exit(manifest != null ? 0 : 1);
    }
}
