using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Runtime.Assets
{
    public class ResourcesManager : Singleton<ResourcesManager>
    {
        private AssetLoad assetLoad;
        private WebAssetLoad remoteLoad;

        public void Init()
        {
            remoteLoad = new WebAssetLoad();
            if (!Application.isEditor || AppConst.config.editorLoadAssetBundle)
            {
                assetLoad = new RuntimeAssetLoad();
            }
            else
            {
#if UNITY_EDITOR

                assetLoad = new EditorAssetLoad();
#else
                assetLoad = new RuntimeAssetLoad();
#endif
            }
        }
        //获取加载器
        private AssetLoad GetAssetLoad(string path)
        {
            if (path.StartsWith("http"))
                return remoteLoad;
            return assetLoad;
        }

        public AssetHandle Load(string path)
        {
            return assetLoad.Load(path);
        }

        public AssetLoadAsync LoadAsync(string path)
        {
            return GetAssetLoad(path).LoadAsync(path);
        }

        public void SetRefCount(AssetHandle assetHandle, bool isAdd)
        {
            if (assetHandle == null) return;
            GetAssetLoad(assetHandle.path).SetRefCount(assetHandle, isAdd);
        }

        public void RemoveCache(string path)
        {
            GetAssetLoad(path).RemoveCache(path);
        }

        //获取对应包体的所有资源包
        public List<AssetHandle> GetAssetHandleListByPackageName(string packageName)
        {
            return assetLoad.GetAssetHandleListByPackageName(packageName);
        }

        //手动卸载一个资源包
        public void UnloadByPath(string path, bool unloadAllLoadedObjects)
        {
            GetAssetLoad(path).UnloadByPath(path, unloadAllLoadedObjects);
        }

        //手动卸载对应包体的所有资源包
        public void UnloadByPackageName(string packageName, bool unloadAllLoadedObjects)
        {
            assetLoad.UnloadByPackageName(packageName, unloadAllLoadedObjects);
        }

        public GameObject CreateGameObject(string path, Transform parent = null, string assetName = "")
        {
            AssetHandle handle = Load(path);
            if (handle == null) return null;
            GameObject go = handle.CreateGameObject(parent, assetName);
            SetAssetBundleBehaviour(go, handle);
            return go;
        }

        public void CreateGameObjectAsync(string path, System.Action<GameObject> func, Transform parent = null, string assetName = "")
        {
            LoadAsync(path).callback = (handle) =>
            {
                if (handle == null)
                {
                    func?.Invoke(null);
                    return;
                }

                handle.CreateGameObjectAsync(parent, assetName).callback = (go) =>
                {
                    SetAssetBundleBehaviour(go, handle);
                    func?.Invoke(go);
                };
            };
        }

        //public T LoadAsset<T>(string path, GameObject bindGameObject = null, string assetName = "") where T : Object
        //{
        //    AssetHandle handle = Load(path);
        //    if (handle == null) return null;
        //    handle.LoadAsset
        //}

        public void SetAssetBundleBehaviour(GameObject go, AssetHandle assetHandle)
        {
            if (!go) return;
            var behaviour = go.GetComponent<AssetBundleBehaviour>();
            if (!behaviour) behaviour = go.AddComponent<AssetBundleBehaviour>();
            behaviour.AddAssetHandle(assetHandle);
        }

    }
}
