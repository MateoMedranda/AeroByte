using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AeroByteWindowsBuild
{
    private const string IconPath = "Assets/_Game/Menu/Missions/A-F14TOMCAT.png";
    private const string OutputDirectory = @"C:\Users\elkin\Desktop\AeroByte";

    public static void Build()
    {
        Directory.CreateDirectory(OutputDirectory);
        PlayerSettings.productName = "AeroByte";

        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        if (icon != null)
        {
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone, new[]
            {
                icon, icon, icon, icon, icon, icon, icon, icon
            });
        }

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = Array.ConvertAll(EditorBuildSettings.scenes, scene => scene.path),
            locationPathName = Path.Combine(OutputDirectory, "AeroByte.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception($"Windows build failed: {report.summary.result}");
        }
    }
}
