using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildAssetBundlesWindow : EditorWindow
{
    string outputPath = "Assets/Resources/AssetBundle";   // đường dẫn mặc định
    BuildTarget buildTarget = BuildTarget.StandaloneWindows;

    [MenuItem("Tools/Build AssetBundles (With GUI)")]
    static void ShowWindow()
    {
        GetWindow<BuildAssetBundlesWindow>("Build AssetBundles");
    }

    void OnGUI()
    {
        GUILayout.Label("Cấu hình Build AssetBundle", EditorStyles.boldLabel);

        // Ô nhập đường dẫn
        EditorGUILayout.BeginHorizontal();
        outputPath = EditorGUILayout.TextField("Output Path", outputPath);
        if (GUILayout.Button("Chọn...", GUILayout.MaxWidth(80)))
        {
            string path = EditorUtility.OpenFolderPanel("Chọn thư mục output", Application.dataPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                // chuyển sang đường dẫn tương đối trong Assets
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }
                outputPath = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        // Chọn platform (BuildTarget)
        buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", buildTarget);

        GUILayout.Space(10);

        if (GUILayout.Button("Build AssetBundles", GUILayout.Height(30)))
        {
            BuildBundles();
        }
    }

    void BuildBundles()
    {
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        BuildPipeline.BuildAssetBundles(
            outputPath,
            BuildAssetBundleOptions.ChunkBasedCompression,
            buildTarget
        );

        // thêm đuôi .ab
        string[] files = Directory.GetFiles(outputPath);
        foreach (string file in files)
        {
            if (file.EndsWith(".manifest") || file.EndsWith(".meta") || file.Contains("AssetBundles"))
                continue;

            string newPath = file + ".ab";
            if (File.Exists(newPath)) File.Delete(newPath);
            File.Move(file, newPath);
        }

        Debug.Log("✅ Build xong! AssetBundle được lưu tại: " + outputPath);
    }
}
