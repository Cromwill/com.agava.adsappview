using System.IO;
using UnityEngine;

namespace AdsAppView.Utility
{
    public static class FileUtils
    {
        public static string ConstructFilePath(string filePath, string name)
        {
            string extension = Path.GetExtension(filePath);
            return Path.Combine(Application.persistentDataPath, name + extension);
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

        public static void TrySaveFile(string filePath, byte[] bytes)
        {
            try
            {
                File.WriteAllBytes(filePath, bytes);
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
                texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);
            }

            return texture != null;
        }

        public static void TrySaveTexture(string filePath, Texture2D texture)
        {
            TrySaveFile(filePath, texture.EncodeToPNG());
        }
    }
}
