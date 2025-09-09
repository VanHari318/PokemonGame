using UnityEngine;
using UnityEditor;

public class SetAllTexturesTo16PPU : EditorWindow
{
    [MenuItem("Tools/Convert All Textures to 16 PPU")]
    static void ConvertTextures()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D");

        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16; // đặt PPU = 16
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
                count++;
            }
        }

        Debug.Log($"✅ Đã chuyển {count} texture sang 16 PPU.");
    }
}
