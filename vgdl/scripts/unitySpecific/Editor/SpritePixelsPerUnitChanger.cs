using UnityEditor;
using UnityEngine;

public class SpritePixelsPerUnitChanger : AssetPostprocessor
{
    void OnPreprocessTexture ()
    {
        TextureImporter textureImporter  = (TextureImporter) assetImporter;
        textureImporter.spritePixelsPerUnit = 24;
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
    }
}