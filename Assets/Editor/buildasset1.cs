using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class AssetBundleFolderBuilder : EditorWindow
{
    private static readonly string baseOutputFolderNameSuffix = "_AssetBundles";
    private string selectedFolderPath = "Assets";

    private bool buildForWindows = true;
    private bool buildForAndroid = false;
    private bool buildForIOS = false;

    // Cache for paths to avoid repeated calculations
    private readonly Dictionary<string, string> relativePathCache = new Dictionary<string, string>();

    private readonly HashSet<string> createdDirectories = new HashSet<string>();

    [MenuItem("AssetBundles/Build Textures from Specified Folder")]
    public static void ShowWindow()
    {
        AssetBundleFolderBuilder window = GetWindow<AssetBundleFolderBuilder>("Build AssetBundles");
        window.selectedFolderPath = GetInitialPath();
    }

    private static string GetInitialPath()
    {
        Object selectedObject = Selection.activeObject;
        if (selectedObject == null) return "Assets";

        string assetPath = AssetDatabase.GetAssetPath(selectedObject);
        if (string.IsNullOrEmpty(assetPath)) return "Assets";

        if (AssetDatabase.IsValidFolder(assetPath))
        {
            return assetPath;
        }

        string parentFolder = Path.GetDirectoryName(assetPath);
        return AssetDatabase.IsValidFolder(parentFolder) ? parentFolder : "Assets";
    }

    private void OnGUI()
    {
        GUILayout.Label("Build AssetBundles from Specified Folder", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        DrawFolderSelection();
        EditorGUILayout.Space();
        DrawPlatformSelection();
        EditorGUILayout.Space();
        DrawBuildButton();
    }

    private void DrawFolderSelection()
    {
        EditorGUILayout.BeginHorizontal();
        selectedFolderPath = EditorGUILayout.TextField("Source Folder Path", selectedFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(70)))
        {
            SelectFolder();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void SelectFolder()
    {
        string path = EditorUtility.OpenFolderPanel("Select Source Folder", selectedFolderPath, "");
        if (string.IsNullOrEmpty(path)) return;

        if (path.StartsWith(Application.dataPath))
        {
            selectedFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
        }
        else
        {
            Debug.LogWarning("Selected folder is outside the Assets folder. Please choose a folder within your Unity project.");
            selectedFolderPath = "";
        }
    }

    private void DrawPlatformSelection()
    {
        GUILayout.Label("Select Target Platforms:", EditorStyles.boldLabel);
        buildForWindows = EditorGUILayout.Toggle("Windows (StandaloneWindows64)", buildForWindows);
        buildForAndroid = EditorGUILayout.Toggle("Android", buildForAndroid);
        buildForIOS = EditorGUILayout.Toggle("iOS", buildForIOS);
    }

    private void DrawBuildButton()
    {
        if (string.IsNullOrEmpty(selectedFolderPath))
        {
            EditorGUILayout.HelpBox("Please specify a valid source folder path.", MessageType.Warning);
            return;
        }

        if (!AssetDatabase.IsValidFolder(selectedFolderPath))
        {
            EditorGUILayout.HelpBox("The specified path is not a valid folder in the project. Please ensure it exists and is within your Assets folder.", MessageType.Error);
            return;
        }

        if (!buildForWindows && !buildForAndroid && !buildForIOS)
        {
            EditorGUILayout.HelpBox("Please select at least one target platform.", MessageType.Warning);
            return;
        }

        EditorGUILayout.HelpBox($"Ready to process folder: {selectedFolderPath}", MessageType.Info);
        if (GUILayout.Button("Build AssetBundles"))
        {
            BuildSelectedAssetBundles();
        }
    }

    private static bool IsTypeAFolder(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder(folderPath)) return false;
        return true;
    }

    private void BuildSelectedAssetBundles()
    {
        if (!AssetDatabase.IsValidFolder(selectedFolderPath))
        {
            Debug.LogError("Invalid source folder path. Aborting build.");
            return;
        }

        // Clear caches for new build
        relativePathCache.Clear();
        createdDirectories.Clear();

        List<string> foldersToProcess = GetFoldersToProcess();
        if (foldersToProcess.Count == 0)
        {
            Debug.LogError("No folders determined for AssetBundle building. Aborting.");
            return;
        }

        BuildAssetBundlesForFolders(foldersToProcess);

        AssetDatabase.Refresh();
        Debug.Log("Finished all selected AssetBundle builds.");
    }

    private List<string> GetFoldersToProcess()
    {
        var foldersToProcess = new List<string>();

        if (IsTypeAFolder(selectedFolderPath))
        {
            Debug.Log($"'{selectedFolderPath}' is a 'Type A' folder. Building AssetBundles directly from it.");
            foldersToProcess.Add(selectedFolderPath);
        }
        else
        {
            Debug.Log($"'{selectedFolderPath}' is not a 'Type A' folder. Checking its direct subfolders for 'Type A' patterns.");

            string[] directSubFolders = AssetDatabase.GetSubFolders(selectedFolderPath);
            bool foundAnyTypeA = false;

            foreach (string subFolder in directSubFolders)
            {
                if (IsTypeAFolder(subFolder))
                {
                    Debug.Log($"Found 'Type A' folder: '{subFolder}'. Adding to process list.");
                    foldersToProcess.Add(subFolder);
                    foundAnyTypeA = true;
                }
            }

            if (!foundAnyTypeA)
            {
                Debug.LogWarning($"No 'Type A' folders found within '{selectedFolderPath}'. Processing as general folder.");
                foldersToProcess.Add(selectedFolderPath);
            }
        }

        return foldersToProcess;
    }

    private void BuildAssetBundlesForFolders(List<string> foldersToProcess)
    {
        var platforms = new List<(BuildTarget target, string suffix)>();

        if (buildForWindows) platforms.Add((BuildTarget.StandaloneWindows64, "_windows"));
        if (buildForAndroid) platforms.Add((BuildTarget.Android, "_android"));
        if (buildForIOS) platforms.Add((BuildTarget.iOS, "_ios"));

        foreach (string folderToBuild in foldersToProcess)
        {
            foreach (var (target, suffix) in platforms)
            {
                Debug.Log($"Initiating build for {target} from '{folderToBuild}'...");
                BuildAssetBundlesForFolder(folderToBuild, target, suffix);
            }
        }
    }

    private void BuildAssetBundlesForFolder(string rootFolderPath, BuildTarget targetPlatform, string platformSuffix)
    {
        if (!AssetDatabase.IsValidFolder(rootFolderPath))
        {
            Debug.LogWarning($"Selected path is not a valid folder: {rootFolderPath}");
            return;
        }

        string rootFolderName = Path.GetFileName(rootFolderPath);
        string parentDirectory = Path.GetDirectoryName(rootFolderPath);
        string outputRootPath = Path.Combine(parentDirectory, rootFolderName + baseOutputFolderNameSuffix + platformSuffix);

        // Get all PNG assets in one call
        string[] allPngGuids = AssetDatabase.FindAssets("t:Texture2D", new string[] { rootFolderPath });
        var validPngPaths = new List<string>();

        // Filter to only PNG files
        foreach (string guid in allPngGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string extension = Path.GetExtension(assetPath);
            if (extension.Equals(".png", System.StringComparison.OrdinalIgnoreCase) || extension.Equals(".jpg", System.StringComparison.OrdinalIgnoreCase) || extension.Equals(".jpeg", System.StringComparison.OrdinalIgnoreCase))
            {
                validPngPaths.Add(assetPath);
            }
        }

        if (validPngPaths.Count == 0)
        {
            Debug.LogWarning($"No PNG files found in '{rootFolderPath}'");
            return;
        }

        Debug.Log($"Found {validPngPaths.Count} PNG files to process for {targetPlatform}");

        // Pre-create all necessary directories
        EnsureDirectoriesExist(validPngPaths, rootFolderPath, outputRootPath);

        // Batch build all AssetBundles in one call
        BatchBuildAssetBundles(validPngPaths, rootFolderPath, outputRootPath, targetPlatform);

        Debug.Log($"Finished building AssetBundles for {targetPlatform}.");
    }

    private void EnsureDirectoriesExist(List<string> assetPaths, string rootFolderPath, string outputRootPath)
    {
        // Create output root directory
        EnsureDirectoryExists(outputRootPath);

        // Create all subdirectories needed
        var requiredDirs = new HashSet<string>();
        foreach (string assetPath in assetPaths)
        {
            string relativePath = GetRelativePathCached(rootFolderPath, assetPath);
            string assetOutputDir = Path.Combine(outputRootPath, Path.GetDirectoryName(relativePath));
            requiredDirs.Add(assetOutputDir);
        }

        foreach (string dir in requiredDirs)
        {
            EnsureDirectoryExists(dir);
        }
    }

    private void EnsureDirectoryExists(string path)
    {
        if (createdDirectories.Contains(path)) return;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        createdDirectories.Add(path);
    }

    private string GetRelativePathCached(string rootPath, string fullPath)
    {
        string cacheKey = $"{rootPath}|{fullPath}";
        if (relativePathCache.TryGetValue(cacheKey, out string cachedPath))
        {
            return cachedPath;
        }

        string relativePath = GetRelativePath(rootPath, fullPath);
        relativePathCache[cacheKey] = relativePath;
        return relativePath;
    }

    private void BatchBuildAssetBundles(List<string> assetPaths, string rootFolderPath, string outputRootPath, BuildTarget targetPlatform)
    {
        // Group assets by their output directory to build them in batches
        var assetGroups = new Dictionary<string, List<string>>();

        foreach (string assetPath in assetPaths)
        {
            string relativePath = GetRelativePathCached(rootFolderPath, assetPath);
            string assetOutputDir = Path.Combine(outputRootPath, Path.GetDirectoryName(relativePath));

            if (!assetGroups.TryGetValue(assetOutputDir, out List<string> group))
            {
                group = new List<string>();
                assetGroups[assetOutputDir] = group;
            }
            group.Add(assetPath);
        }

        // Build each directory's assets in one batch
        foreach (var kvp in assetGroups)
        {
            string outputDir = kvp.Key;
            List<string> assets = kvp.Value;

            BuildAssetBundleBatch(assets, outputDir, targetPlatform);
        }
    }

    private void BuildAssetBundleBatch(List<string> assetPaths, string outputDir, BuildTarget targetPlatform)
    {
        var builds = new AssetBundleBuild[assetPaths.Count];

        for (int i = 0; i < assetPaths.Count; i++)
        {
            string assetPath = assetPaths[i];
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

            if (texture == null)
            {
                Debug.LogWarning($"Could not load texture at path: {assetPath}");
                continue;
            }

            string exactAssetName = texture.name;
            string desiredFileName = exactAssetName + ".unity3d";

            builds[i].assetBundleName = desiredFileName;
            builds[i].assetNames = new string[] { assetPath };
        }

        try
        {
            // Build all AssetBundles in this directory in one call
            BuildPipeline.BuildAssetBundles(outputDir, builds, BuildAssetBundleOptions.None, targetPlatform);

            // Handle any case-sensitivity issues in batch
            HandleCaseSensitivity(builds, outputDir);

            Debug.Log($"Successfully built {builds.Length} AssetBundles for {targetPlatform} in '{outputDir}'");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error building AssetBundle batch in '{outputDir}': {e.Message}");
        }
    }

    private void HandleCaseSensitivity(AssetBundleBuild[] builds, string outputDir)
    {
        foreach (var build in builds)
        {
            if (string.IsNullOrEmpty(build.assetBundleName)) continue;

            string desiredFileName = build.assetBundleName;
            string desiredFullPath = Path.Combine(outputDir, desiredFileName);

            string actualBuiltFileName = build.assetBundleName.ToLower();
            string actualBuiltFullPath = Path.Combine(outputDir, actualBuiltFileName);

            if (File.Exists(actualBuiltFullPath) &&
                !string.Equals(Path.GetFileName(actualBuiltFullPath), Path.GetFileName(desiredFullPath), System.StringComparison.Ordinal))
            {
                try
                {
                    File.Move(actualBuiltFullPath, desiredFullPath);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Could not rename '{actualBuiltFileName}' to '{desiredFileName}': {e.Message}");
                }
            }
        }
    }

    private static string GetRelativePath(string rootPath, string fullPath)
    {
        rootPath = rootPath.Replace('\\', '/');
        fullPath = fullPath.Replace('\\', '/');

        if (fullPath.StartsWith(rootPath + "/"))
        {
            return fullPath.Substring(rootPath.Length + 1);
        }

        return fullPath.Equals(rootPath) ? "" : fullPath;
    }
}