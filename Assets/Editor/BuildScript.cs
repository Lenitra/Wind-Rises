using UnityEditor;
using UnityEngine;
using System;
using System.IO;

public class BuildScript
{
    private static readonly string BuildDirectory = "Builds/Windows";
    private static readonly string GameName = "Wind";

    [MenuItem("Build/Build Windows")]
    public static void BuildWindows()
    {
        PerformBuild();
    }

    public static void PerformBuild()
    {
        try
        {
            string buildPath = Path.Combine(BuildDirectory, GameName + ".exe");
            string fullBuildPath = Path.GetFullPath(buildPath);

            Debug.Log($"Starting build process...");
            Debug.Log($"Build path: {fullBuildPath}");

            if (!Directory.Exists(BuildDirectory))
            {
                Directory.CreateDirectory(BuildDirectory);
                Debug.Log($"Created build directory: {BuildDirectory}");
            }

            string[] scenes = GetScenePaths();
            if (scenes.Length == 0)
            {
                Debug.LogError("No scenes found in build settings!");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"Building {scenes.Length} scene(s)...");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;

            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {summary.totalSize} bytes");
                Debug.Log($"Build output: {fullBuildPath}");

                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"Build failed: {summary.result}");
                EditorApplication.Exit(1);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Build failed with exception: {e.Message}");
            Debug.LogError(e.StackTrace);
            EditorApplication.Exit(1);
        }
    }

    private static string[] GetScenePaths()
    {
        var scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        return scenes;
    }
}
