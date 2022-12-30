using UnityEngine;
using UnityEditor;
using System.IO;

namespace GameEditor
{
    public class EditorAsset 
    {
        private const string tempDir = "Assets/__assettempload";

        private static void DeleteDir(string path)
        {
            if (Directory.Exists(tempDir)) Directory.Delete(path, true);
            if (File.Exists(tempDir + ".meta"))
                File.Delete(tempDir + ".meta");
        }

        public static T LoadScriptableObject<T>(string path) where T : ScriptableObject
        {            
            if(path.StartsWith("Assets")) return AssetDatabase.LoadAssetAtPath<T>(path); 
            if (Directory.Exists(tempDir)) DeleteDir(tempDir);
            Directory.CreateDirectory(tempDir);
            string metaPath = path + ".meta";
            if (!File.Exists(path) || !File.Exists(metaPath)) return null;
            string fileName = Path.GetFileName(path);
            string newPath = tempDir + "/" + fileName;
            File.Copy(path, newPath);
            File.Copy(metaPath, newPath + ".meta");
            AssetDatabase.Refresh();
            var t = AssetDatabase.LoadAssetAtPath<T>(newPath);
            if (t) t =Object.Instantiate(t);
            if (Directory.Exists(tempDir)) DeleteDir(tempDir);
            AssetDatabase.Refresh();
            t.name = Path.GetFileNameWithoutExtension(path);
            return t;
        }
    }
}