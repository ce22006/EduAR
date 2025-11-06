using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

public class BuildScript
{
    static string[] GetScenes()
    {
        // Get all enabled scenes from Build Settings
        return EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
    }

    // This is the method that will be called by CI/CD
    public static void PerformAndroidBuild()
    {
        try
        {
            Debug.Log("===== Starting Android Build =====");
            
            // Get scenes
            string[] scenes = GetScenes();
            
            if (scenes.Length == 0)
            {
                Debug.LogError("No scenes found in Build Settings! Add scenes to Build Settings first.");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"Building with {scenes.Length} scenes:");
            foreach (var scene in scenes)
            {
                Debug.Log($"  - {scene}");
            }

            // Set Android build settings
            EditorUserBuildSettings.buildAppBundle = false; // Build APK, not AAB
            
            // Configure Android player settings
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            
            // Set bundle identifier
            string bundleIdentifier = "com.eduar.app";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, bundleIdentifier);
            Debug.Log($"Bundle Identifier: {bundleIdentifier}");
            
            // Set architecture to ARM64
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            Debug.Log("Target Architecture: ARM64");

            // Set scripting backend
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            Debug.Log("Scripting Backend: IL2CPP");

            // Build path
            string buildPath = "builds/Android/EduAR.apk";
            Debug.Log($"Output path: {buildPath}");

            // Configure build options
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log("Starting BuildPipeline.BuildPlayer...");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("===== BUILD SUCCEEDED =====");
                Debug.Log($"Output: {buildPath}");
                Debug.Log($"Size: {summary.totalSize / 1024 / 1024} MB");
                Debug.Log($"Time: {summary.totalTime}");
                Debug.Log($"Warnings: {summary.totalWarnings}");
                Debug.Log($"Errors: {summary.totalErrors}");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError("===== BUILD FAILED =====");
                Debug.LogError($"Result: {summary.result}");
                Debug.LogError($"Total errors: {summary.totalErrors}");
                Debug.LogError($"Total warnings: {summary.totalWarnings}");
                
                // Log build steps for debugging
                foreach (BuildStep step in report.steps)
                {
                    Debug.LogError($"Step: {step.name} - Duration: {step.duration}");
                }
                
                EditorApplication.Exit(1);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception during build: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            EditorApplication.Exit(1);
        }
    }

    [MenuItem("Build/Build Android APK")]
    public static void BuildAndroid()
    {
        PerformAndroidBuild();
    }
}
