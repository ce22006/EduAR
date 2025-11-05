using UnityEditor;
using UnityEngine;
using System.Linq;

public class BuildScript
{
    static string[] GetScenes()
    {
        // Get all scenes from Build Settings
        return EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
    }

    [MenuItem("Build/Build Android APK")]
    public static void BuildAndroid()
    {
        // Get scenes
        string[] scenes = GetScenes();
        
        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes found in Build Settings! Add scenes to Build Settings first.");
            return;
        }

        Debug.Log("Building with scenes: " + string.Join(", ", scenes));

        string buildPath = "builds/Android/EduAR.apk";

        // Configure build
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        // Android settings
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.eduar.app");
        
        // Set architecture to ARM64 for better compatibility
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        Debug.Log("Building Android APK...");
        Debug.Log("Output path: " + buildPath);
        
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("✅ Build succeeded!");
            Debug.Log("Output: " + buildPath);
            Debug.Log("Size: " + (summary.totalSize / 1024 / 1024) + " MB");
            Debug.Log("Time: " + summary.totalTime);
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("❌ Build failed!");
            Debug.LogError("Total errors: " + summary.totalErrors);
        }
    }
}
