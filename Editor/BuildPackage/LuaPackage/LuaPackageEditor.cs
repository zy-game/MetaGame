using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Object = UnityEngine.Object;

namespace GameEditor.BuildAsset
{
    public class LuaPackageEditor : BuildAssetPage
    {
        private class Item
        {
            public int index;
            public bool isSelect;//是否选择了
            public bool isNew;//是否是新加的资源
            public bool isRepeat;//是否是重复的资源
            public LuaPackageData.BuildLuaItem luaItem;//基础配置
            public Object obj;
        }

        private const string outputDir = "luapackage";
        private LocalCommonConfig localConfig;
        private LuaPackageData luaPackageConfig;
        private List<Item> items;
        private bool isBuild;
        private int curSelectCount;
        public bool isSelectAll;
        private Vector2 scroll;

        public LuaPackageEditor(AssetBundleBuildSetting buildSetting, AssetBundleBuildSetting.Page page) : base(buildSetting, page)
        {
            Init();
        }

        public override void Enter(object param)
        {
          
        }

        public override void Exit()
        {

        }

        private  void Init()
        {
            localConfig = LocalCommonConfig.Get();
            //selfGUIStyle = new SelfGUIStyle();
            string configPath = AssetDirConfig.assetRootPath + AssetDirConfig.luaDir + "/" + AssetDirConfig.luaConfigName;
            if (File.Exists(configPath))
                luaPackageConfig = AssetDatabase.LoadAssetAtPath<LuaPackageData>(configPath);
            else
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<LuaPackageData>(), configPath);
                luaPackageConfig = AssetDatabase.LoadAssetAtPath<LuaPackageData>(configPath);
                luaPackageConfig.items = new List<LuaPackageData.BuildLuaItem>();
                AssetDatabase.Refresh();
            }

            items = new List<Item>();
            int index = 0;
            foreach (var v in luaPackageConfig.items)
            {
                Item item = new Item();
                item.obj = v.asset;
                item.index = index++;
                item.luaItem = v;
                items.Add(item);
            }
            CheckRepeat();
            LoadLocalConfig();
        }

        private void LoadLocalConfig()
        {
            string path = AppConst.AppConfigPath;
            if (!File.Exists(path))
                AppConst.config = new LocalCommonConfig();
            else
                AppConst.config = LocalCommonConfig.Get();
        }

        public override void Run()
        {
            Title();
            Content();
        }

        private void Title()
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("返回", GUILayout.Height(25), GUILayout.Width(65)))
            {
                buildSetting.ChangePage(AssetBundleBuildSetting.Page.AssetLink);
            }
            if (GUILayout.Button("保存", GUILayout.Height(25), GUILayout.Width(65)))
            {
                Save();
            }
            if (GUILayout.Button("打包", GUILayout.Height(25), GUILayout.Width(65))) isBuild = true;

            if (GUILayout.Button("上传", GUILayout.Height(25), GUILayout.Width(65))) 
            {
                buildSetting.ChangePage(AssetBundleBuildSetting.Page.UploadCode);
            }

            GUILayout.Label(string.Format("已选择{0}个", curSelectCount), GUILayout.Width(65), GUILayout.Height(25));
            bool temp = isSelectAll;
            isSelectAll = EditorGUILayout.Toggle(isSelectAll, GUILayout.Width(25), GUILayout.Height(25));
            if (isSelectAll != temp) ChangeSelectAll(isSelectAll);
            GUILayout.EndHorizontal();
            GUILayout.Box("", selfGUIStyle.line, GUILayout.Width(buildSetting.position.width), GUILayout.Height(5));

            if (isBuild)
            {
                isBuild = false;
                BuildAsset();
            }
        }

        private void Content()
        {
            scroll = GUILayout.BeginScrollView(scroll);
            Item delItem = null;
            int index = 0;
            curSelectCount = 0;
            foreach (var item in items)
            {
                item.index = index++;
                if (DrawItem(item))
                {
                    delItem = item;
                }
            }
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Box("", selfGUIStyle.line, GUILayout.Width(buildSetting.position.width - 20), GUILayout.Height(3));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(buildSetting.position.width / 2 - 60);
            if (GUILayout.Button("添加代码包", GUILayout.Height(25), GUILayout.Width(120)))
            {
                var item = new Item();
                item.isNew = true;
                item.luaItem = new LuaPackageData.BuildLuaItem();
                items.Add(item);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            if (delItem != null)
            {
                items.Remove(delItem);
                CheckRepeat();
            }
        }

        //绘制资源item
        private bool DrawItem(Item item)
        {
            GUIStyle style = selfGUIStyle.item;
            if (item.isNew) style = selfGUIStyle.newItem;
            else if (item.isSelect) style = selfGUIStyle.delItem;
            else if (item.isRepeat) style = selfGUIStyle.line;

            GUILayout.Space(2);
            GUILayout.BeginHorizontal(style, GUILayout.Height(20));
            if (!item.isNew)
            {
                item.isSelect = EditorGUILayout.Toggle(item.isSelect, GUILayout.Width(15));
                if (item.isSelect) curSelectCount++;
            }
            else
                GUILayout.Space(15);
            GUILayout.Label((item.index + 1) + ". " + item.luaItem.name, GUILayout.Width(120));
            Object obj = EditorGUILayout.ObjectField(item.obj, typeof(Object), true, GUILayout.Width(310));
            if (obj != item.obj)
            {
                item.obj = obj;
                item.luaItem.name = obj == null ? "" : obj.name.ToLower();
                CheckRepeat();
            }
            bool del = GUILayout.Button("del", GUILayout.Width(35));
            GUILayout.EndHorizontal();
            return del;
        }

        private void ChangeSelectAll(bool isSelectAll)
        {
            foreach (var item in items)
            {
                item.isSelect = isSelectAll;
            }
        }

        private void CheckRepeat()
        {
            foreach (var item in items)
            {
                item.isRepeat = false;
            }

            foreach (var item in items)
            {
                foreach (var item1 in items)
                {
                    if (item == item1 || item.obj == null || item1.obj == null) continue;
                    if (item.obj == item1.obj)
                    {
                        item.isRepeat = true;
                        item1.isRepeat = true;
                    }
                }
            }
        }

        private void BuildAsset()
        {
            string outputPath = BuildAssetConfig.buildRootPath + outputDir + "/";
            List<Item> buildAssets = new List<Item>();
            foreach (var item in items)
            {
                if (!item.isSelect) continue;

                if (item.luaItem.asset)
                    buildAssets.Add(item);
            }

            if (buildAssets.Count == 0)
            {
                Debug.LogError("没有选择要打包的代码!");
                return;
            }

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            string[] dirs = Directory.GetDirectories(outputPath);
            List<int> dirNames = new List<int>();
            foreach (var dir in dirs)
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                dirNames.Add(int.Parse(di.Name));
            }

            int version = 1;//当前发布资源本地版本
            if (dirNames.Count > 0)
            {
                dirNames.Sort();
                version = dirNames[dirNames.Count - 1];
            }
            int newVersion = version + 1;
            string newDirPath = outputPath + newVersion + "/";
            Directory.CreateDirectory(newDirPath);
            if (version != 1)
            {
                string lastDirPath = outputPath + version + "/";
                string[] oldFiles = Directory.GetFiles(lastDirPath, "*" + localConfig.buildLuaCodeExtName);
                foreach (var v in oldFiles)
                {
                    FileInfo fi = new FileInfo(v);
                    File.Copy(v, newDirPath + fi.Name);
                }
            }

            //打包
            BuildCode(newDirPath, buildAssets, newVersion);


            //当前数量大于设置的保存数量，需删除多余的缓存版本，从小到大开始删除
            if (dirNames.Count > BuildAssetConfig.cacheCount)
            {
                int dt = dirNames.Count - BuildAssetConfig.cacheCount;
                for (int i = 0; i < dt; i++)
                {
                    int v = dirNames[i];
                    Directory.Delete(outputPath + v, true);
                }
            }

            Debug.Log("打包完成:" + newDirPath);
            System.Diagnostics.Process.Start("explorer.exe", newDirPath.Replace("/", @"\"));

        }

        private void BuildCode(string outPath, List<Item> buildAssets, int version)
        {
            Dictionary<string, byte[]> luabytes = new Dictionary<string, byte[]>();
            foreach (var v in buildAssets)
            {
                string name = v.luaItem.name;
                byte[] bts = new LuaBuildBytes(v.luaItem.asset as DefaultAsset).Build();
                if (bts == null) continue;
                luabytes.Add(name, bts);
            }

            foreach (var v in luabytes)
            {
                string filePath = outPath + v.Key + localConfig.buildLuaCodeExtName;
                File.WriteAllBytes(filePath, v.Value);
            }

            AssetManager.GenerateFileList(outPath, version, localConfig.codeModuleName, new string[] { ".luabytes" });
        }


        private void Save()
        {
            luaPackageConfig.items.Clear();
            foreach (var v in items)
            {
                if (v.obj)
                {
                    LuaPackageData.BuildLuaItem luaItem = new();
                    luaItem.asset = v.obj;
                    luaItem.name = v.obj.name.ToLower();
                    luaPackageConfig.items.Add(luaItem);
                }
            }
            luaPackageConfig.buildRootPath =new DirectoryInfo(BuildAssetConfig.buildRootPath + outputDir + "/").FullName;          
            AssetDatabase.SaveAssetIfDirty(luaPackageConfig);
            EditorUtility.SetDirty(luaPackageConfig);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();           
        }

    }
}