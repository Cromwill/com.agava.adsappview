using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace AdsAppView.Utility
{
    public static class FileUtils
    {
        public static string ConstructCacheFilePath(string filePath)
        {
            string name = Path.GetFileName(filePath);
            return Path.Combine(Application.persistentDataPath, name);
        }

        public static bool TryLoadFile(string filePath, out byte[] bytes)
        {
            bytes = null;
            if (File.Exists(filePath))
            {
                bytes = File.ReadAllBytes(filePath);

#if UNITY_EDITOR
                Debug.Log($"#FileUtils# Cache texture loaded from path: {filePath}");
#endif
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log($"#FileUtils# Path {filePath} doesn't exist");
#endif
            }

            return bytes != null;
        }

        public static async Task TrySaveFile(string filePath, byte[] bytes)
        {
            try
            {
                await File.WriteAllBytesAsync(filePath, bytes);
#if UNITY_EDITOR
                Debug.Log($"#FileUtils# File saved to path: {filePath}");
#endif
            }
            catch (IOException exception)
            {
                Debug.LogError("#FileUtils# Fail to save file: " + exception.Message);
            }
        }

        public static bool TryLoadTexture(string filePath, out Texture2D texture)
        {
            texture = null;

            if (TryLoadFile(filePath, out byte[] bytes))
            {
                texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);
            }

            return texture != null;
        }

        public static void TrySaveTexture(string filePath, Texture2D texture)
        {
            TrySaveFile(filePath, texture.EncodeToPNG());
        }

        public static Sprite LoadSprite(byte[] bytes)
        {
            Texture2D texture = new (1, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            texture.LoadImage(bytes);

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
        }
    }
}
