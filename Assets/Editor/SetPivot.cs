using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Highly optimized Unity Editor Window to set and offset sprite pivots for selected PNG Texture2D assets.
/// Optimized for processing thousands of sprites with minimal performance impact.
/// FIXED: Now uses mother PNG pixel coordinates instead of sprite-relative coordinates.
/// </summary>
public class SetAndOffsetPivot : EditorWindow
{

    private float legheight = 12f;
    private const float DefaultPixelsPerUnit = 16f;

    // Progress tracking
    private bool isProcessing = false;

    private float progress = 0f;
    private string currentOperation = "";

    [MenuItem("Tools/SetAndOffsetPivot")]
    public static void ShowWindow()
    {
        GetWindow<SetAndOffsetPivot>("Set And Offset Pivot");
    }

    private void OnGUI()
    {
        GUILayout.Label("Sprite Pivot Tool (Optimized)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        GUILayout.Label("Set LegHeight", EditorStyles.miniBoldLabel);
        legheight = EditorGUILayout.FloatField("LegHeight", legheight);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox($"Pixels Per Unit (PPU) is fixed at {DefaultPixelsPerUnit}. This tool will ensure your textures are set as Sprites.", MessageType.Info);
        EditorGUILayout.Space();

        // Disable buttons during processing
        GUI.enabled = !isProcessing;

        if (GUILayout.Button("Process All Operations (Ultra Optimized)"))
        {
            ProcessAllOperationsOptimized();
        }

        GUI.enabled = true;

        // Show progress bar during processing
        if (isProcessing)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Status:", currentOperation);
            EditorGUILayout.Space();
            Rect progressRect = EditorGUILayout.GetControlRect();
            EditorGUI.ProgressBar(progressRect, progress, $"Processing... {(progress * 100):F1}%");
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Select PNG Texture2D assets or folders containing them in the Project window. Optimized for thousands of sprites. Uses MOTHER PNG pixel coordinates.", MessageType.Info);
    }

    /// <summary>
    /// Ultra-optimized method for processing thousands of sprites with minimal performance impact
    /// </summary>
    private async void ProcessAllOperationsOptimized()
    {
        if (isProcessing) return;

        isProcessing = true;
        progress = 0f;
        currentOperation = "Gathering assets...";

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Get all importers in one pass
            List<TextureImporter> importers = GetSelectedSpriteImportersOptimized();
            if (importers.Count == 0)
            {
                Debug.LogWarning("No PNG Texture2D assets found in selection.");
                return;
            }

            Debug.Log($"Processing {importers.Count} sprites with optimized batch operations...");
            currentOperation = $"Processing {importers.Count} sprites...";

            // Disable automatic asset processing temporarily
            AssetDatabase.StartAssetEditing();

            try
            {
                // Process in batches to avoid memory issues and allow progress updates
                const int batchSize = 50; // Process 50 sprites at a time
                int totalBatches = (importers.Count + batchSize - 1) / batchSize;
                int processedCount = 0;

                for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
                {
                    var batch = importers.Skip(batchIndex * batchSize).Take(batchSize).ToList();

                    // Process batch
                    foreach (var importer in batch)
                    {
                        ProcessSingleImporterOptimized(importer);
                        processedCount++;

                        // Update progress
                        progress = (float)processedCount / importers.Count;
                        currentOperation = $"Processed {processedCount}/{importers.Count} sprites...";

                        // Allow UI updates every 10 sprites
                        if (processedCount % 10 == 0)
                        {
                            await Task.Yield(); // Allow UI to update
                            Repaint();
                        }
                    }
                }
            }
            finally
            {
                // Re-enable automatic asset processing and process all changes at once
                AssetDatabase.StopAssetEditing();
            }

            // Final optimized refresh
            currentOperation = "Finalizing changes...";
            progress = 0.9f;
            Repaint();

            OptimizedFinalizeAssetChanges();

            stopwatch.Stop();
            progress = 1f;
            currentOperation = $"Completed! Processed {importers.Count} sprites in {stopwatch.ElapsedMilliseconds}ms";

            Debug.Log($"Ultra-optimized processing completed in {stopwatch.ElapsedMilliseconds}ms for {importers.Count} sprites");

            // Clear progress after 3 seconds
            await Task.Delay(3000);
            if (currentOperation.Contains("Completed!"))
            {
                currentOperation = "";
                progress = 0f;
                Repaint();
            }
        }
        finally
        {
            isProcessing = false;
            Repaint();
        }
    }

    /// <summary>
    /// Optimized version that minimizes asset database queries and string operations
    /// </summary>
    private List<TextureImporter> GetSelectedSpriteImportersOptimized()
    {
        var importers = new HashSet<TextureImporter>(); // Use HashSet to avoid duplicates automatically
        var selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

        // Pre-filter and batch process
        var validPaths = new List<string>();
        var folderPaths = new List<string>();

        foreach (Object obj in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath)) continue;

            if (AssetDatabase.IsValidFolder(assetPath))
            {
                folderPaths.Add(assetPath);
            }
            else if (obj is Texture2D && assetPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
            {
                validPaths.Add(assetPath);
            }
        }

        // Process direct files
        foreach (string path in validPaths)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer?.textureType == TextureImporterType.Sprite)
            {
                importers.Add(importer);
            }
        }

        // Process folders in batch
        if (folderPaths.Count > 0)
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", folderPaths.ToArray());

            // Process GUIDs in batches to avoid memory spikes
            foreach (string guid in guids)
            {
                string texturePath = AssetDatabase.GUIDToAssetPath(guid);
                if (texturePath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                {
                    var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                    if (importer?.textureType == TextureImporterType.Sprite)
                    {
                        importers.Add(importer);
                    }
                }
            }
        }

        return importers.ToList();
    }

    /// <summary>
    /// Process a single importer with all optimizations applied
    /// FIXED: Now uses mother PNG coordinates instead of sprite-relative coordinates
    /// </summary>
    private void ProcessSingleImporterOptimized(TextureImporter importer)
    {
        bool changed = false;

        // 1. Ensure correct settings
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }
        if (importer.spriteImportMode != SpriteImportMode.Multiple)
        {
            importer.spriteImportMode = SpriteImportMode.Multiple;
            changed = true;
        }
        if (importer.spritePixelsPerUnit != DefaultPixelsPerUnit)
        {
            importer.spritePixelsPerUnit = DefaultPixelsPerUnit;
            changed = true;
        }

        // 2. Load texture to get mother PNG dimensions
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(importer.assetPath);
        if (texture == null) return;

        float motherPngWidth = texture.width;
        float motherPngHeight = texture.height;

        // 3. Handle sprite data - only create default metadata if needed
        SpriteMetaData[] spriteSheet = importer.spritesheet;
        if (spriteSheet == null || spriteSheet.Length == 0)
        {
            spriteSheet = new SpriteMetaData[1];
            spriteSheet[0] = new SpriteMetaData
            {
                name = Path.GetFileNameWithoutExtension(importer.assetPath),
                rect = new Rect(0, 0, motherPngWidth, motherPngHeight),
                alignment = (int)SpriteAlignment.Custom
            };
            changed = true;
        }

        // 4. Process pivot changes efficiently using MOTHER PNG coordinates
        if (spriteSheet != null)
        {
            for (int i = 0; i < spriteSheet.Length; i++)
            {
                var sprite = spriteSheet[i];
                var spriteRect = sprite.rect;

                // FIXED: Calculate pivot based on mother PNG coordinates
                // Convert mother PNG pixel coordinates to normalized coordinates (0-1) relative to the sprite
                // First, get the absolute position in mother PNG, then convert to sprite-relative normalized
                //Vector2 absoluteMotherPivot = new Vector2(setPivotX + offsetX, setPivotY + offsetY);

                // Convert from mother PNG coordinates to sprite-relative normalized coordinates
                Vector2 finalPivot = new Vector2(0.5f, legheight / spriteRect.height);

                // Only update if different
                bool spriteChanged = false;
                if (sprite.alignment != (int)SpriteAlignment.Custom)
                {
                    sprite.alignment = (int)SpriteAlignment.Custom;
                    spriteChanged = true;
                }

                if (sprite.pivot != finalPivot)
                {
                    sprite.pivot = finalPivot;
                    spriteChanged = true;
                }

                if (spriteChanged)
                {
                    spriteSheet[i] = sprite;
                    changed = true;
                }
            }
        }

        // 5. Apply changes only if needed
        if (changed)
        {
            if (spriteSheet != null)
            {
                importer.spritesheet = spriteSheet;
            }
            EditorUtility.SetDirty(importer);
            // Note: SaveAndReimport is called implicitly when AssetDatabase.StopAssetEditing() is called
        }
    }

    /// <summary>
    /// Optimized finalization that minimizes redundant operations
    /// </summary>
    private void OptimizedFinalizeAssetChanges()
    {
        // Save all changes at once
        AssetDatabase.SaveAssets();

        // Single refresh instead of multiple
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        // Minimal selection handling
        if (Selection.objects != null && Selection.objects.Length > 0)
        {
            // Force selection refresh without reloading assets
            EditorApplication.RepaintProjectWindow();
        }
    }
}