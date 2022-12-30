using GameFramework.Runtime.Assets;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace GameEditor.BuildAsset
{

    public class AssetUploadPage : BuildAssetPage
    {
        private const string configFilePath = "Assets/ArtistRes/assetBuildConfig/uploadAddressConfig.asset";

        private string moduleName;
        private string buildAssetRoot; //资源缓存根路径
        private AssetVersion localAssetVersion;//本地资源版本配置
        private AssetVersion remoteAssetVersion;//远程资源版本配置
        private string[] versionNames;
        private int selectVersion;

        private UploadAddressConfig addressConfig;
        private string[] uploadConfigPath;//上传配置地址
        private int selectUploadPath;

        private bool editorAddress;//配置地址
        private bool isLoading;//正在加载
        private bool isUpload;//正在上传
        private bool uploadFinished;

        private AssetUploader uploader;
        private List<AssetUploader.FileItem> uploadItems;
        private List<AssetUploader.FileItem> delItems;
        private int curUploadCount;

        private Vector2 scrollView = Vector2.zero;

        public AssetUploadPage(AssetBundleBuildSetting buildSetting, AssetBundleBuildSetting.Page page) : base(buildSetting, page)
        {
            //获取地址配置
            if (File.Exists(configFilePath))
            {
                addressConfig = AssetDatabase.LoadAssetAtPath<UploadAddressConfig>(configFilePath);
                selectUploadPath = addressConfig.current;
            }
            if (addressConfig == null)
            {
                addressConfig = ScriptableObject.CreateInstance<UploadAddressConfig>();
                addressConfig.paths = new List<UploadAddressItem>();
                addressConfig.current = -1;
                AssetDatabase.CreateAsset(addressConfig, configFilePath);
                AssetDatabase.Refresh();
            }
            uploadItems = new List<AssetUploader.FileItem>();
            delItems = new List<AssetUploader.FileItem>();
        }

        public override void Enter(object param)
        {
            string cacheRootPath = BuildAssetConfig.buildRootPath + BuildAssetConfig.buildCachePath + buildSetting.PlatformName + "/";
            string versionPath = cacheRootPath + "version.txt";
            if (!File.Exists(versionPath))
            {
                EditorUtility.DisplayDialog("错误", "找不到本地版本配置", "返回");
                buildSetting.DelayCall(0, () => Return());
                return;
            }
            localAssetVersion = JsonObject.Deserialize<AssetVersion>(File.ReadAllText(versionPath));
            moduleName = (string)param;
            int maxVersion = localAssetVersion.FindVersion(moduleName);
            if (maxVersion == 0)
            {
                EditorUtility.DisplayDialog("错误", "Version配置里面找不到对应的模块", "返回");
                buildSetting.DelayCall(0, () => Return());
                return;
            }
            buildAssetRoot = cacheRootPath + moduleName + "_cache/";
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

        private void Return()
        {
            buildSetting.ChangePage(AssetBundleBuildSetting.Page.Main);
        }

        //切换上传地址
        private void ChangeUploadAddress()
        {
            if (selectUploadPath >= addressConfig.paths.Count)
            {
                uploader = null;
                return;
            }
            string downloadUrl = addressConfig.paths[selectUploadPath].downloadPath + buildSetting.PlatformName + "/" + moduleName + "/";
            string uploadUrl = addressConfig.paths[selectUploadPath].uploadPath + buildSetting.PlatformName + "/" + moduleName + "/";
            string localPath = buildAssetRoot + versionNames[selectVersion] + "/";

            UploaderParam uploadParam = new()
            {
                address = addressConfig.paths[selectUploadPath],
                buildAssetRoot = localPath,
                moduleName = moduleName,
                owner = buildSetting,
                platformName = buildSetting.PlatformName,
                localVersion=int.Parse(versionNames[selectVersion])

            };

            if (uploader != null && uploader.Equals(uploadParam)) return;
            if (uploader != null) uploader.Clear();

            if (downloadUrl.StartsWith("http")) uploader = new AssetUploderRemote(uploadParam);
            else uploader = new AssetUploderLocal(uploadParam);

            isLoading = true;
            uploadItems.Clear();
            delItems.Clear();

            //加载远程版本配置
            buildSetting.StartCor(LoadRemoteVersionConfig(() =>
            {
                uploader.LoadRemoteFileList((fileList) =>
                {
                    CheckUploadAsset();
                    isLoading = false;
                    buildSetting.Repaint();
                });

            }));
        }

        //加载远程版本配置
        private IEnumerator LoadRemoteVersionConfig(System.Action action)
        {
            string url = addressConfig.paths[selectUploadPath].downloadPath + buildSetting.PlatformName + "/version.txt";            
            using UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(url);
            yield return new WaitWebRequest(request.SendWebRequest());
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                remoteAssetVersion = JsonObject.Deserialize<AssetVersion>(request.downloadHandler.text);
                action?.Invoke();
            }
            else
            {
                Debug.Log(url);
                Debug.LogError(request.error);
                EditorUtility.DisplayDialog("错误", "下载不到远程版本配置", "确定");
                //buildSetting.DelayCall(0, () => Return());
            }
        }

        //检测要上传的资源
        private void CheckUploadAsset()
        {
            if (uploader == null) return;
            uploadItems.AddRange(uploader.uploadAssets);
            delItems.AddRange(uploader.delItems);
        }

        public override void Exit()
        {
            if (uploader != null) uploader.Clear();
            buildSetting.StopCorAll();
            buildSetting.RemovePage(AssetBundleBuildSetting.Page.UploadAsset);
        }

        public override void Run()
        {
            if (editorAddress)
            {
                EditorAddress();
                return;
            }
            if (!isUpload)
                Title();
            if (isLoading)
                GUILayout.Label("正在加载...");
            else
                ShowUploadAsset();
        }

        //上传地址编辑
        private void EditorAddress()
        {
            GUILayout.BeginHorizontal(selfGUIStyle.item, GUILayout.Height(20));
            GUILayout.Label("编辑资源上传地址:");
            GUILayout.EndHorizontal();
            int delIndex = -1;
            for (int i = 0; i < addressConfig.paths.Count; i++)
            {
                var item = addressConfig.paths[i];
                GUILayout.Space(3);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(EditorGUI.Sub, selfGUIStyle.item, GUILayout.Width(16), GUILayout.Height(88))) delIndex = i;
                GUILayout.Space(1);
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal(selfGUIStyle.item, GUILayout.Height(20));
                GUILayout.Label("alias:", GUILayout.Width(100));
                item.alias = GUILayout.TextField(item.alias);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal(selfGUIStyle.item, GUILayout.Height(20));
                GUILayout.Label("sid:", GUILayout.Width(100));
                item.sid = GUILayout.TextField(item.sid);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(selfGUIStyle.item, GUILayout.Height(20));
                GUILayout.Label("pid:", GUILayout.Width(100));
                item.pid = GUILayout.TextField(item.pid);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(selfGUIStyle.item, GUILayout.Height(20));
                GUILayout.Label("token:", GUILayout.Width(100));
                item.token = GUILayout.TextField(item.token);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(selfGUIStyle.item, GUILayout.Height(20));
                GUILayout.Label("download path:", GUILayout.Width(100));
                item.downloadPath = GUILayout.TextField(item.downloadPath);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(selfGUIStyle.item, GUILayout.Height(20));
                GUILayout.Label("upload path:", GUILayout.Width(100));
                item.uploadPath = GUILayout.TextField(item.uploadPath);
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            if (delIndex != -1)
                addressConfig.paths.RemoveAt(delIndex);

            if (GUILayout.Button("添加上传地址")) addressConfig.paths.Add(new UploadAddressItem());
            if (GUILayout.Button("返回"))
            {
                editorAddress = false;
                UpdateConfigPath();
                EditorUtility.SetDirty(addressConfig);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void Title()
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("返回", GUILayout.Height(25), GUILayout.Width(75))) Return();
            if (GUILayout.Button("配置", GUILayout.Height(25), GUILayout.Width(75))) editorAddress = true;
            if (GUILayout.Button("上传", GUILayout.Height(25), GUILayout.Width(75)))
            {
                isUpload = true;
                uploader.UploadAsset(UploadFunc);
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
                UpdateRemoteVersionConfig();
            }
        }

        //更新远程版本文件
        private void UpdateRemoteVersionConfig()
        {
            remoteAssetVersion.UpdateVersion(moduleName, int.Parse(versionNames[selectVersion]));
            remoteAssetVersion.UpdateDepends(moduleName, localAssetVersion.dependsMap[moduleName]);
            remoteAssetVersion.UpdateLinkCodePackage(moduleName, localAssetVersion.codeLinkMap[moduleName]);
            uploader.UpdateRemoteVersionConfig(JsonObject.Serialize(remoteAssetVersion), (b) =>
              {
                  if (!b)
                  {
                      EditorUtility.DisplayDialog("错误", "远程版本配置更新失败", "确定");
                  }
                  else
                  {
                      EditorUtility.DisplayDialog("提示", "上传完成", "确定");
                      uploadFinished = true;
                  }
              });
        }

        //显示要上传的资源
        private void ShowUploadAsset()
        {
            GUILayout.Label("远程版本:" + remoteAssetVersion.FindVersion(moduleName));
            if (isUpload)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("上传:" + curUploadCount + "/" + (uploadItems.Count+delItems.Count));
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