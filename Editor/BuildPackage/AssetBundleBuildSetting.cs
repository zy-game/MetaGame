using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GameFramework.Runtime.Assets;
using System.IO;

namespace GameEditor.BuildAsset
{
    public class AssetBundleBuildSetting : BaseEditorWindow
    {

        public enum Page
        {
            Main,
            EditorAsset,
            EditorModule,
            LocalAsset,
            UploadAsset,
            UploadCode,
            AssetLink,
            LuaPackage,
        }

        public static void Open(BuildAssetConfig assetConfig)
        {
            var window = Open<AssetBundleBuildSetting>(500, 650, "资源打包");
            window.assetConfig = assetConfig;
            window.ChangePage(Page.Main);
        }

        public static void Open()
        {
            var window = Open<AssetBundleBuildSetting>(500, 650, "资源管理");
            window.ChangePage(Page.AssetLink);
        }

        public BuildAssetConfig assetConfig;
        private Dictionary<Page, BuildAssetPage> pageMap;
        private BuildAssetPage curPage;

        protected override void Init()
        {
            pageMap = new Dictionary<Page, BuildAssetPage>();
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

        public void ChangePage(Page page, object param = null)
        {
            BuildAssetPage buildAssetPage;
            if (!pageMap.TryGetValue(page, out buildAssetPage))
            {
                switch (page)
                {
                    case Page.Main:
                        buildAssetPage = new PageMain(this, page);
                        break;
                    case Page.EditorAsset:
                        buildAssetPage = new PageEditorAsset(this, page);
                        break;
                    case Page.EditorModule:
                        buildAssetPage = new PageEditorModule(this, page);
                        break;
                    case Page.LocalAsset:
                        buildAssetPage = new PageLocalAssetsManager(this, page);
                        break;
                    case Page.UploadAsset:
                        buildAssetPage = new AssetUploadPage(this, page);
                        break;
                    case Page.UploadCode:
                        buildAssetPage = new CodeUploadPage(this, page);
                        break;
                    case Page.AssetLink:
                        buildAssetPage = new AssetLinkEditor(this, page);
                        break;
                    case Page.LuaPackage:
                        buildAssetPage = new LuaPackageEditor(this, page);
                        break;
                }
                if (buildAssetPage == null) return;
                pageMap.Add(page, buildAssetPage);
            }
            if (curPage != null) curPage.Exit();
            curPage = buildAssetPage;
            curPage.Enter(param);
        }

        public void RemovePage(Page page)
        {
            pageMap.Remove(page);
        }

        private void OnGUI()
        {
            if (curPage == null)
                Init();
            curPage.Run();
        }

        public void Save()
        {
            EditorUtility.SetDirty(assetConfig);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        //当前发布设置平台名称
        public string PlatformName
        {
            get
            {
                switch (EditorUserBuildSettings.activeBuildTarget)
                {
                    case BuildTarget.Android:
                        return "Android";
                    case BuildTarget.iOS:
                        return "iOS";
                    case BuildTarget.StandaloneWindows64:
                    case BuildTarget.StandaloneWindows:
                        return "Windows";
                }

                return "None";
            }
        }

    }

    public class GeneratedFileList : BaseEditorWindow
    {
        [MenuItem("Tools/Other/生成FileList")]
        private static void Open()
        {
            Open<GeneratedFileList>(500, 150, "生成FileList");
        }

        private string path;

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("资源根路径:");
            path = GUILayout.TextField(path);
            GUILayout.Space(10);
            if (GUILayout.Button("生成", GUILayout.Height(30), GUILayout.Width(150))) Generated();
        }

        private void Generated()
        {
            AssetManager.GenerateFileList(path, 1, new DirectoryInfo(path).Name.ToLower());
        }
    }
}