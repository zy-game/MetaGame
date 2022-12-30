using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Runtime.Assets
{
    public class ResourceLoader : AssetLoad
    {
        public override List<AssetHandle> GetAssetHandleListByPackageName(string packageName)
        {
            return new List<AssetHandle>();
        }

        public override AssetHandle Load(string path)
        {
            ResourceHandle handle = new ResourceHandle(path);
            return handle;
        }

        public override AssetLoadAsync LoadAsync(string path)
        {
            AssetLoadAsync loadAsync = AssetLoadAsync.Get();
            CorManager.Instance.DelayCall(this,0,()=> 
            {
                loadAsync.callback(new ResourceHandle(path));
            });
            return loadAsync;
        }       

        public override void SetRefCount(AssetHandle assetHandle, bool isAdd)
        {
          
        }
    }

    public class ResourceHandle : AssetHandle
    {
        public ResourceHandle(string path) : base(path)
        {
        }

        public override void AddToReleasePool()
        {

        }

        private string GetFullPath(string assetName)
        {
            string pt = path;
            if (!string.IsNullOrEmpty(assetName))
                pt = pt + "/" + assetName;
            return pt;
        }

        public override GameObject CreateGameObject(Transform parent = null, string assetName = "")
        {
            GameObject obj =LoadAsset<GameObject>(assetName);
            if (obj == null)
                return null;
            GameObject go = Instantiate(obj, parent);
            return go;

        }

        public override AssetHandleAsync<GameObject> CreateGameObjectAsync(Transform parent = null, string assetName = "")
        {
            AssetHandleAsync<GameObject> handleAsync = AssetHandleAsync<GameObject>.Get();
            var handle= LoadAssetAsync<GameObject>(assetName);
            handle.callback = (obj) =>
            {
                if (obj == null)
                {
                    handleAsync.Finished(null);
                    return;
                }
                GameObject go = Instantiate(obj as GameObject, parent);
                handleAsync.Finished(go);
            };
            return handleAsync;
        }

        public override string[] GetDepends()
        {
            return null;
        }

        public override UnityEngine.Object LoadAsset(Type type, string assetName = "")
        {
            return LoadAsset<UnityEngine.Object>(assetName);
        }

        public override T LoadAsset<T>(string assetName = "")
        {
            return Resources.Load<T>(GetFullPath(assetName));
        }

        public override AssetHandleAsync<UnityEngine.Object> LoadAssetAsync(Type type, string assetName = "")
        {
            AssetHandleAsync<UnityEngine.Object> assetHandleAsync = AssetHandleAsync<UnityEngine.Object>.Get();
            CorManager.Instance.StartCoroutine(LoadCor(assetHandleAsync, assetName));
            return assetHandleAsync;
        }

        public override AssetHandleAsync<T> LoadAssetAsync<T>(string assetName = "")
        {
            AssetHandleAsync<T> assetHandleAsync = AssetHandleAsync<T>.Get();
            CorManager.Instance.StartCoroutine(LoadCor(assetHandleAsync, assetName));
            return assetHandleAsync;
        }

        private IEnumerator LoadCor<T>(AssetHandleAsync<T> handle,string assetName) where T : UnityEngine.Object
        {
            string fullPath = GetFullPath(assetName);
            var wait = Resources.LoadAsync<T>(fullPath);
            yield return wait;
            handle.callback(wait.asset as T);

        }

        public override AsyncOperation LoadSceneAsync(string assetName)
        {
            throw new NotImplementedException();
        }
    }
}
