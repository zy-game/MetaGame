using GameFramework.Runtime.Assets;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace GameEditor.BuildAsset
{

    public class CodeUploadPage : BuildAssetPage
    {
        private const string moduleName = "luapackage";

        private LuaPackageData luaPackage;

        private string buildAssetRoot; //资源缓存根路径

        private UploadAddressConfig addressConfig;
        private string[] uploadConfigPath;//上传配置地址
        private int selectUploadPath;
        private string[] versionNames;
        private int selectVersion;

        private AssetUploader uploader;
        private bool isLoading;//正在加载
        private bool isUpload;//正在上传
        private bool uploadFinished;

        private List<AssetUploader.FileItem> uploadItems;
        private List<AssetUploader.FileItem> delItems;
        private int curUploadCount;

        private bool isSelectAll = false;

        public CodeUploadPage(AssetBundleBuildSetting buildSetting, AssetBundleBuildSetting.Page page) : base(buildSetting, page)
        {
            string configFilePath, luaPackageDataPath;

            if (AssetManager.IsAssetProject())
            {
                configFilePath = "Assets/ArtistRes/assetBuildConfig/uploadAddressConfig.asset";
                luaPackageDataPath = "Assets/ArtistRes/assetBuildConfig/luaConfig/luaPackageConfig.asset";
            }
            else
            {
                string assetProjectPath = AssetManager.GetAssetProjectPath();
                configFilePath = assetProjectPath+ "ArtistRes/assetBuildConfig/uploadAddressConfig.asset";
                luaPackageDataPath = "Assets/ArtistRes/luaConfig/luaPackageConfig.asset";
            }

            //获取地址配置
            if (File.Exists(configFilePath))
            {
                addressConfig = EditorAsset.LoadScriptableObject<UploadAddressConfig>(configFilePath);
                selectUploadPath = addressConfig.current;
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "找不到上传地址配置", "返回");
                buildSetting.DelayCall(0, () => Return());
            }

            if (File.Exists(luaPackageDataPath))
            {
                luaPackage = EditorAsset.LoadScriptableObject<LuaPackageData>(luaPackageDataPath);
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "找不到代码包配置", "返回");
                buildSetting.DelayCall(0, () => Return());
            }
            buildAssetRoot = luaPackage.buildRootPath;
            uploadItems = new List<AssetUploader.FileItem>();
            delItems = new List<AssetUploader.FileItem>();
        }

        public override void Enter(object param)
        {
            string[] dirs = Directory.GetDirectories(buildAssetRoot);
            List<int> versionDirList = new List<int>();
            foreach (var dir in dirs)
            {
                string name = new DirectoryInfo(dir).Name;
                if (int.TryParse(name, out int version)) versionDirList.Add(version);
            }
            versionDirList.Sort();
            versionDirList.Reverse();

            versionNames = new string[versionDirList.Count];
            for (int i = 0; i < versionDirList.Count; i++) versionNames[i] = versionDirList[i].ToString();
            UpdateConfigPath();
        }

        private void UpdateConfigPath()
        {
            uploadConfigPath = new string[addressConfig.paths.Count];
            for (int i = 0; i < addressConfig.paths.Count; i++) uploadConfigPath[i] = addressConfig.paths[i].alias;
            if (selectUploadPath >= addressConfig.paths.Count)
                selectUploadPath = 0;
            ChangeUploadAddress();
        }

        //切换上传地址
        private void ChangeUploadAddress()
        {
            if (selectUploadPath >= addressConfig.paths.Count)
            {
                uploader = null;
                return;
            }
            string downloadUrl = addressConfig.paths[selectUploadPath].downloadPath + moduleName + "/";
            string uploadUrl = addressConfig.paths[selectUploadPath].uploadPath + moduleName + "/";
            string localPath = buildAssetRoot + versionNames[selectVersion] + "/";
            UploaderParam uploadParam = new()
            {
                address = addressConfig.paths[selectUploadPath],
                buildAssetRoot = localPath,
                moduleName = moduleName,
                owner = buildSetting,
                platformName = "",
                localVersion = 1

            };

            if (uploader != null && uploader.Equals(uploadParam)) return;
            if (uploader != null) uploader.Clear();

            if (downloadUrl.StartsWith("http")) uploader = new CodeUploderRemote(uploadParam);
            else uploader = new CodeUploderLocal(uploadParam);

            isLoading = true;
            uploadItems.Clear();
            delItems.Clear();

            uploader.LoadRemoteFileList((fileList) =>
            {
                CheckUploadAsset();
                isLoading = false;
                buildSetting.Repaint();
            });
        }

        //检测要上传的资源
        private void CheckUploadAsset()
        {
            if (uploader == null) return;
            uploadItems.AddRange(uploader.uploadAssets);
            delItems.AddRange(uploader.delItems);

            ResetUploadItemState();
        }

        private void ResetUploadItemState()
        {
            foreach (var v in uploadItems) v.isUpload = isSelectAll;
            foreach (var v in delItems) v.isUpload = isSelectAll;
        }

        public override void Exit()
        {
            if (uploader != null) uploader.Clear();
            buildSetting.StopCorAll();
            buildSetting.RemovePage(AssetBundleBuildSetting.Page.UploadCode);
        }

        public override void Run()
        {
            if (!isUpload)
                Title();
            if (isLoading)
                GUILayout.Label("正在加载...");
            else
                ShowUploadAsset();
        }

        private void Return()
        {
            if (AssetManager.IsAssetProject())
                buildSetting.ChangePage(AssetBundleBuildSetting.Page.Main);
            else
                buildSetting.ChangePage(AssetBundleBuildSetting.Page.LuaPackage);
        }

        private void Title()
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("返回", GUILayout.Height(25), GUILayout.Width(75))) Return();
            if (GUILayout.Button("上传", GUILayout.Height(25), GUILayout.Width(75)))
            {
                if (uploadItems.Count + delItems.Count == 0) return;
                isUpload = true;
                uploader.UploadAsset(UploadFunc);
            }

            GUILayout.Label("全选:",GUILayout.Width(30));
            bool select = GUILayout.Toggle(isSelectAll,"",GUILayout.Width(20));
            if (isSelectAll != select)
            {
                isSelectAll = select;
                ResetUploadItemState();
            }
            GUILayout.Label("版本:", GUILayout.Width(30));
            int i = EditorGUILayout.Popup(selectVersion, versionNames);
            if (selectVersion != i)
            {
                selectVersion = i;
                ChangeUploadAddress();
            }
            GUILayout.Label("地址:", GUILayout.Width(30));
            int selectPath = EditorGUILayout.Popup(selectUploadPath, uploadConfigPath);
            if (selectUploadPath != selectPath)
            {
                selectUploadPath = selectPath;
                addressConfig.current = selectPath;
                EditorUtility.SetDirty(addressConfig);
                AssetDatabase.SaveAssets();
                ChangeUploadAddress();
            }
            GUILayout.EndHorizontal();
            GUILayout.Box("", selfGUIStyle.line, GUILayout.Width(buildSetting.position.width), GUILayout.Height(5));
        }

        private void UploadFunc(int cur, bool finished)
        {
            curUploadCount = cur;
            if (finished)//上传完成 
            {
                uploadFinished = true;
                buildSetting.Repaint();
            }
        }

        private Vector2 scrollView = Vector2.zero;
        //显示要上传的资源
        private void ShowUploadAsset()
        {
            if (uploadItems.Count + delItems.Count == 0)
            {
                GUILayout.Label("没有需要更新的资源");
                return;
            }

            if (isUpload)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("上传:" + curUploadCount + "/" + (uploadItems.Count + delItems.Count));
                if (uploadFinished)
                {
                    if (GUILayout.Button("返回", GUILayout.Width(100)))
                    {
                        buildSetting.DelayCall(0, () =>
                        {
                            Return();
                        });
                    }
                }
                GUILayout.EndHorizontal();
            }
            float width = buildSetting.position.width;
            int index = 1;
            scrollView = GUILayout.BeginScrollView(scrollView);

            foreach (var v in delItems)
            {
                GUILayout.Space(2);
                GUILayout.BeginHorizontal(selfGUIStyle.delItem);
                v.isUpload = GUILayout.Toggle(v.isUpload, "", GUILayout.Width(20));
                GUILayout.Label(index + ".", GUILayout.Width(25));
                GUILayout.Label(v.name);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal(selfGUIStyle.line);
                GUILayout.Box("", selfGUIStyle.newItem, GUILayout.Height(3), GUILayout.Width(width * v.progress));
                GUILayout.EndHorizontal();
                index++;
            }

            index = 1;
            foreach (var v in uploadItems)
            {
                GUILayout.Space(2);
                GUILayout.BeginHorizontal(selfGUIStyle.item);
                v.isUpload = GUILayout.Toggle(v.isUpload, "", GUILayout.Width(20));
                GUILayout.Label(index + ".", GUILayout.Width(25));
                GUILayout.Label(v.name);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal(selfGUIStyle.line);
                GUILayout.Box("", selfGUIStyle.newItem, GUILayout.Height(3), GUILayout.Width(width * v.progress));
                GUILayout.EndHorizontal();
                index++;
            }
            GUILayout.Space(16);
            GUILayout.EndScrollView();
        }
    }
}