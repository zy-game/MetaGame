using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;

namespace GameFramework.Runtime.Assets
{
    //版本管理
    public class VersionManager
    {
        //版本文件名
        public const string versionFileName = "version.txt";
        public AssetVersion localVersion; //本地版本配置
        public AssetVersion remoteVersion;//远程版本配置

        private WaitFinished versionAsync;

        public VersionManager()
        {
            GlobalEvent.AddEvent(EventName.RecheckVersion, RecheckVersion);
        }

        //重新检测版本
        private void RecheckVersion(object obj)
        {
            CorManager.Instance.StartCoroutine(CheckMainAssetVersion());
        }

        public WaitFinished Init()
        {
            versionAsync = new WaitFinished();
            CorManager.Instance.StartCoroutine(CheckMainAssetVersion());
            return versionAsync;
        }

        //检测主资源版本
        private IEnumerator CheckMainAssetVersion()
        {
            bool isEditor = Application.isEditor;


            yield return LoadRomoteConfig();  //加载远程配置

            //编辑器下并且非资源更新模式不检测版本
            if (isEditor && !AppConst.config.editorUpdateAssets)
            {
                versionAsync.Finished();
                yield break;
            }
        
            yield return LoadRemoteVersion();//下载远程资源版本配置

            yield return LoadCodePackageConfig();//加载代码包配置

            if (remoteVersion == null || AssetVersion.remoteCodeFileList == null || string.IsNullOrEmpty(AppConst.config.asseturl))
            {
                GlobalEvent.Notify(EventName.NotLoadRemoteVersion);
                yield break;
            }

            if (remoteVersion.packageVersion != AppConst.config.version)
            {
                GlobalEvent.Notify(EventName.BigVersionUpdate);
                yield break;
            }

            localVersion = LoadLocalVersion();
            if (localVersion == null)//本地没有版本文件
            {
                if (AppConst.config.localReleaseAsset)
                {

                    GlobalEvent.Notify(EventName.StartReleaseAsset);
                    ReleaseAssets releaseAssets = new ReleaseAssets();
                    var async = releaseAssets.Release();
                    yield return async;
                    GlobalEvent.Notify(EventName.EndReleaseAsset);
                    localVersion = LoadLocalVersion();
                }
                if (localVersion == null)
                {
                    //初始化本地资源版本配置
                    localVersion = new AssetVersion();
                    localVersion.versionMap = new Dictionary<string, int>();
                }
            }

            AssetVersion.remoteVersion = remoteVersion;
            AssetVersion.localVersion = localVersion;
            List<string> updateModuleNames = AssetVersion.GetUpdateModules(AppConst.config.mainModuleName, out bool checkUpdateError);
            if (checkUpdateError)//版本检测错误
            {
                GlobalEvent.Notify(EventName.AssetUpdateFail);
                yield break;
            }
            updateModuleNames.Add(AppConst.config.configModuleName);//检测配置更新
            CorManager.Instance.StartCoroutine(UpdateAsset(updateModuleNames.ToArray()));
            yield break;

            //版本检测完成没有更新,版本检测完成
            //versionAsync.Finished();
        }

        //获取本地版本配置
        private AssetVersion LoadLocalVersion()
        {
            string path = AppConst.DataPath + versionFileName;
            if (File.Exists(path))
                return JsonObject.Deserialize<AssetVersion>(File.ReadAllText(path));
            return null;
        }

        //加载远程代码包配置
        private IEnumerator LoadCodePackageConfig()
        {
            string url = AppConst.LuaCodeUrl + "files.txt";
            using UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("获取代码配置错误:" + request.error);
                yield break;
            }
            AssetFileEntity remoteCodeFileList = AssetFileEntity.Get(request.downloadHandler.text);
            AssetFileEntity localCodeFileList;
            string localPath = AppConst.GetModulePath(AppConst.config.codeModuleName) + "files.txt";
            if (!File.Exists(localPath))
                localCodeFileList = AssetFileEntity.Get(remoteCodeFileList);
            else
                localCodeFileList = AssetFileEntity.Get(File.ReadAllText(localPath));
            if (string.IsNullOrEmpty(localCodeFileList.moduleName))
                localCodeFileList.moduleName = AppConst.config.codeModuleName;
            AssetVersion.remoteCodeFileList = remoteCodeFileList;
            AssetVersion.localCodeFileList = localCodeFileList;
        }

        //加载远程配置
        private IEnumerator LoadRomoteConfig()
        {
            if (AppConst.config.useTestUrl)//使用测试地址
            {
                TestUrl testUrl = null;
                foreach (var item in AppConst.config.testUrls)
                {
                    if (item.isUse)
                    {
                        testUrl = item;
                        break;
                    }
                }
                if (testUrl != null)
                {
                    AppConst.config.asseturl = testUrl.assetUrl;
                    AppConst.config.httpUrl = testUrl.httpUrl;
                    AppConst.config.websocketUrl = testUrl.websocketUrl;
                    AppConst.config.configUrl = testUrl.configUrl;
                    yield break;
                }
            }

            string url = AppConst.config.remoteurl;
            Dictionary<string, string> param = new();
            param.Add("id", AppConst.config.remotekey);
            using UnityWebRequest request = Util.PostJson(url, JsonObject.Serialize(param));
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string content = request.downloadHandler.text;
                RequestConfigResult requestConfigResult = JsonObject.Deserialize<RequestConfigResult>(content);
                if (requestConfigResult == null || requestConfigResult.code != 200)
                {
                    Debug.LogError("获取远程配置错误:" + content);
                    AppConst.config.asseturl = string.Empty;
                }
                else
                {
                    string info = requestConfigResult.data.info;
                    string key = AppConst.config.remotekey.Substring(0, 16);
                    info = Util.Decrypt(info, key, key);
                    RemoteConfig remoteConfig = JsonObject.Deserialize<RemoteConfig>(info);
                    AppConst.config.asseturl = remoteConfig.resources_url;
                    AppConst.config.configUrl = remoteConfig.config_url;
                    AppConst.config.httpUrl = remoteConfig.api_url;
                    AppConst.config.websocketUrl = remoteConfig.socket_url;
                }
                yield break;
            }
            Debug.LogError(request.error);
            Debug.LogError("连接服务器错误:" + url);
        }

        //获取远程版本配置
        private IEnumerator LoadRemoteVersion()
        {
            if (remoteVersion != null) yield break;
            string url = AppConst.config.asseturl + AppConst.PlatformName + "/" + versionFileName;
            using UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                remoteVersion = JsonObject.Deserialize<AssetVersion>(request.downloadHandler.text);
                yield break;
            }
            Debug.LogError(request.error);
            Debug.LogError("获取远程版本配置错误:" + url);
        }

        //更新主资源
        private IEnumerator UpdateAsset(string[] moduleName)
        {
            AssetUpdate assetUpdate = new AssetUpdate(moduleName);
            //通知需要更新文件
            assetUpdate.onNotifyUpdateAsset = () =>
            {
                GlobalEvent<AssetUpdate>.Notify(EventName.NotifyUpdateAsset, assetUpdate);
            };
            //资源下载失败回调
            assetUpdate.onDownloadAssetFail = (url) =>
            {
                GlobalEvent<string>.Notify(EventName.DownloadAssetFail, url);
            };
            yield return assetUpdate.CheckUpdate();//等待更新完成
            if (!assetUpdate.IsUpdateError)
            {
                Complete();
            }
            else
                GlobalEvent.Notify(EventName.AssetUpdateFail);
            assetUpdate.Dispose();
        }

        private void Complete()
        {
            versionAsync.Finished();
            GlobalEvent.Remove(EventName.RecheckVersion, RecheckVersion);
        }

        //请求远程配置返回
        public class RequestConfigResult
        {
            public class Data
            {
                public string info;
            }

            public int code;
            public string msg;
            public Data data;

            public string assetUrl;//资源服务器地址
            public string httpUrl;//http连接地址
            public string websocketUrl;//websocket连接地址
            public string conftgUrl;//配置地址
        }

        public class RemoteConfig
        {
            public string resources_url;
            public string config_url;
            public string api_url;
            public string socket_url;
        }
    }
}
