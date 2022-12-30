using System.Collections.Generic;
using System.IO;
using GameFramework.Editor.UIGenerator;
using UnityEditor;
using UnityEngine;
namespace GameFramework.Editor
{
    public static class EditorMenu
    {
        [MenuItem("Game/UI Generate %`")]
        public static void ShowEditor()
        {
            UIGeneratorWindow.GetWindow<UIGeneratorWindow>("UI Generate", true).Show();
        }

        [MenuItem("Game/Generic Preview")]
        public static void GenericPreviewImage()
        {
            foreach (var item in Selection.objects)
            {
                Texture2D texture = AssetPreview.GetAssetPreview(item);
                string assetpath = AssetDatabase.GetAssetPath(item);
                string dir = Path.GetDirectoryName(assetpath) + "/../icon/";
                assetpath = dir + Path.GetFileNameWithoutExtension(assetpath) + ".png";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                if (File.Exists(assetpath))
                {
                    File.Delete(assetpath);
                }
                if (texture == null)
                {
                    continue;
                }
                File.WriteAllBytes(assetpath, texture.EncodeToPNG());
                // AssetDatabase.CreateAsset(texture, assetpath);
                // AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetpath);
                if (importer == null)
                {
                    continue;
                }
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.sRGBTexture = true;
                importer.alphaIsTransparency = true;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.isReadable = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.maxTextureSize = 512;
                importer.compressionQuality = 100;
                importer.SaveAndReimport();
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("Game/Generic Thing Json")]
        public static void GetAssetPath()
        {
            if (Selection.objects == null || Selection.objects.Length <= 0)
            {
                return;
            }

            List<object> path = new List<object>();
            for (var i = 0; i < Selection.objects.Length; i++)
            {
                string temp = AssetDatabase.GetAssetPath(Selection.objects[i]);//.Replace("Assets/ArtistRes/", "").Replace(".mat", "").Replace(".png", "").Replace(".prefab", "");
                string fileName = Path.GetFileNameWithoutExtension(temp);
                var thingData = new
                {
                    name = fileName,
                    path = "thing/prefab/" + fileName,
                    icon = "thing/icon/" + fileName
                };
                path.Add(thingData);
            }

            TextEditor te = new TextEditor();
            te.text = Newtonsoft.Json.JsonConvert.SerializeObject(path);
            te.SelectAll();
            te.Copy();
        }


        [MenuItem("Game/Build")]
        public static void BuidleAndroid()
        {
            LocalCommonConfig localCommonConfig = Get();
            string[] args = System.Environment.GetCommandLineArgs();
            if (args == null || args.Length <= 0)
            {
                return;
            }

            string packagename = "meta";
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == "-Version")
                {
                    string version = args[i].Split(':')[1];
                    PlayerSettings.bundleVersion = version;
                }

                if (args[i] == "-PackagedName")
                {
                    packagename = args[i].Split(':')[1];
                    PlayerSettings.companyName = packagename.Split('.')[1];
                    PlayerSettings.productName = packagename.Split('.')[2];
                }

                if (args[i] == "-ServerAddres")
                {
                    localCommonConfig.remoteurl = args[i].Split(':')[1];
                }
            }
            localCommonConfig.editorLoadAssetBundle = true;
            localCommonConfig.editorUpdateAssets = true;
            localCommonConfig.editorUseLuaBytes = true;
            localCommonConfig.useTestUrl = false;
            localCommonConfig.testUrls?.Clear();
            Save(localCommonConfig);
            BuildPlayerOptions opt = new BuildPlayerOptions();

#if UNITY_STANDALONE_WIN
        opt.target = BuildTarget.StandaloneWindows;
        opt.locationPathName = Application.dataPath + "/../../build/window/" + packagename + ".exe";
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
#elif UNITY_ANDROID
            opt.target = BuildTarget.Android;
            opt.locationPathName = Application.dataPath + "/../../build/android/" + packagename + ".apk";
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
#elif UNITY_IOS
        opt.target = BuildTarget.iOS;
        opt.locationPathName = Application.dataPath + "/../../build/ios/";
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
#endif
            if (!Directory.Exists(Path.GetDirectoryName(opt.locationPathName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(opt.locationPathName));
            }
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_4_6);
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            opt.scenes = new string[] { "Assets/Scene/MainWorld.unity" };
            opt.options = BuildOptions.None;
            BuildPipeline.BuildPlayer(opt);
        }

        public static LocalCommonConfig Get()
        {
            string path = AppConst.AppConfigPath;
            if (!System.IO.File.Exists(path)) return new LocalCommonConfig();
            string content = System.IO.File.ReadAllText(path);
            return JsonObject.Deserialize<LocalCommonConfig>(content);
        }

        public static void Save(LocalCommonConfig config)
        {
            if (config == null) return;
            string path = AppConst.AppConfigPath;
            string content = JsonObject.Serialize(config);
            System.IO.File.WriteAllText(path, content);
        }

    }
}