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
        private string buildAssetRoot; //��Դ�����·��
        private AssetVersion localAssetVersion;//������Դ�汾����
        private AssetVersion remoteAssetVersion;//Զ����Դ�汾����
        private string[] versionNames;
        private int selectVersion;

        private UploadAddressConfig addressConfig;
        private string[] uploadConfigPath;//�ϴ����õ�ַ
        private int selectUploadPath;

        private bool editorAddress;//���õ�ַ
        private bool isLoading;//���ڼ���
        private bool isUpload;//�����ϴ�
        private bool uploadFinished;

        private AssetUploader uploader;
        private List<AssetUploader.FileItem> uploadItems;
        private List<AssetUploader.FileItem> delItems;
        private int curUploadCount;

        private Vector2 scrollView = Vector2.zero;

        public AssetUploadPage(AssetBundleBuildSetting buildSetting, AssetBundleBuildSetting.Page page) : base(buildSetting, page)
        {
            //��ȡ��ַ����
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
                EditorUtility.DisplayDialog("����", "�Ҳ������ذ汾����", "����");
                buildSetting.DelayCall(0, () => Return());
                return;
            }
            localAssetVersion = JsonObject.Deserialize<AssetVersion>(File.ReadAllText(versionPath));
            moduleName = (string)param;
            int maxVersion = localAssetVersion.FindVersion(moduleName);
            if (maxVersion == 0)
            {
                EditorUtility.DisplayDialog("����", "Version���������Ҳ�����Ӧ��ģ��", "����");
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

        //�л��ϴ���ַ
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

            //����Զ�̰汾����
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

        //����Զ�̰汾����
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
                EditorUtility.DisplayDialog("����", "���ز���Զ�̰汾����", "ȷ��");
                //buildSetting.DelayCall(0, () => Return());
            }
        }

        //���Ҫ�ϴ�����Դ
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
                GUILayout.Label("���ڼ���...");
            else
                ShowUploadAsset();
        }

        //�ϴ���ַ�༭
        private void EditorAddress()
        {
            GUILayout.BeginHorizontal(selfGUIStyle.item, GUILayout.Height(20));
            GUILayout.Label("�༭��Դ�ϴ���ַ:");
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

            if (GUILayout.Button("����ϴ���ַ")) addressConfig.paths.Add(new UploadAddressItem());
            if (GUILayout.Button("����"))
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
            if (GUILayout.Button("����", GUILayout.Height(25), GUILayout.Width(75))) Return();
            if (GUILayout.Button("����", GUILayout.Height(25), GUILayout.Width(75))) editorAddress = true;
            if (GUILayout.Button("�ϴ�", GUILayout.Height(25), GUILayout.Width(75)))
            {
                isUpload = true;
                uploader.UploadAsset(UploadFunc);
            }
            GUILayout.Label("�汾:", GUILayout.Width(30));
            int i = EditorGUILayout.Popup(selectVersion, versionNames);
            if (selectVersion != i)
            {
                selectVersion = i;
                ChangeUploadAddress();
            }
            GUILayout.Label("��ַ:", GUILayout.Width(30));
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
            if (finished)//�ϴ���� 
            {
                UpdateRemoteVersionConfig();
            }
        }

        //����Զ�̰汾�ļ�
        private void UpdateRemoteVersionConfig()
        {
            remoteAssetVersion.UpdateVersion(moduleName, int.Parse(versionNames[selectVersion]));
            remoteAssetVersion.UpdateDepends(moduleName, localAssetVersion.dependsMap[moduleName]);
            remoteAssetVersion.UpdateLinkCodePackage(moduleName, localAssetVersion.codeLinkMap[moduleName]);
            uploader.UpdateRemoteVersionConfig(JsonObject.Serialize(remoteAssetVersion), (b) =>
              {
                  if (!b)
                  {
                      EditorUtility.DisplayDialog("����", "Զ�̰汾���ø���ʧ��", "ȷ��");
                  }
                  else
                  {
                      EditorUtility.DisplayDialog("��ʾ", "�ϴ����", "ȷ��");
                      uploadFinished = true;
                  }
              });
        }

        //��ʾҪ�ϴ�����Դ
        private void ShowUploadAsset()
        {
            GUILayout.Label("Զ�̰汾:" + remoteAssetVersion.FindVersion(moduleName));
            if (isUpload)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("�ϴ�:" + curUploadCount + "/" + (uploadItems.Count+delItems.Count));
                if (uploadFinished)
                {
                    if (GUILayout.Button("����", GUILayout.Width(100)))
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