using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

[InitializeOnLoad]
public class AutoImportUnityStoreAssets
{
    private static readonly string assetStorePath = "Assets/"; // Change this if needed
    private static readonly string importedFlagPath = "Assets/Editor/FirstLaunchComplete.txt";
    
    private static readonly List<string> requiredAssets = new List<string>
    {
        "FREE CASUAL PACK SFX",
        "Dialogue Editor",
        "2D Casual UI HD",
        "Loading screen animation",
        "Lowpoly Environment - Nature Free - MEDIEVAL FANTASY SERIES",
        "Low Poly Modular Characters",
        "Free Pixel Font - Thaleah",
        "FREE Low Poly Human - RPG Character",
        "Footsteps - Essentials",
        "Fantasy Skybox FREE",
        "Fantasy landscape"
    };

    static AutoImportUnityStoreAssets()
    {
        if (!EditorPrefs.HasKey("ProjectFirstLaunch") || !File.Exists(importedFlagPath))
        {
            EditorApplication.update += ImportAssetsOnFirstLaunch;
        }
    }

    private static void ImportAssetsOnFirstLaunch()
    {
        EditorApplication.update -= ImportAssetsOnFirstLaunch;

        Debug.Log("First-time launch detected! Importing Unity Asset Store packages...");

        // Mark project as initialized
        EditorPrefs.SetBool("ProjectFirstLaunch", true);
        File.WriteAllText(importedFlagPath, "Initialized");

        ImportDownloadedPackages();
    }

    private static void ImportDownloadedPackages()
    {
        string assetStoreDownloadPath = GetAssetStoreDownloadPath();

        if (!Directory.Exists(assetStoreDownloadPath))
        {
            Debug.LogError($"Asset Store download directory not found: {assetStoreDownloadPath}");
            return;
        }

        string[] downloadedPackages = Directory.GetFiles(assetStoreDownloadPath, "*.unitypackage", SearchOption.AllDirectories);

        foreach (string package in downloadedPackages)
        {
            foreach (string assetName in requiredAssets)
            {
                if (package.Contains(assetName))
                {
                    Debug.Log($"Importing asset: {package}");
                    AssetDatabase.ImportPackage(package, false);
                }
            }
        }

        AssetDatabase.Refresh();
    }

    private static string GetAssetStoreDownloadPath()
    {
        string userProfile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        
        if (Application.platform == RuntimePlatform.WindowsEditor)
            return Path.Combine(userProfile, "AppData/Roaming/Unity/Asset Store-5.x");
        
        if (Application.platform == RuntimePlatform.OSXEditor)
            return Path.Combine(userProfile, "Library/Unity/Asset Store-5.x");

        return string.Empty; // No support for Linux
    }
}
