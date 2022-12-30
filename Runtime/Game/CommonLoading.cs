using GameFramework.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework.Runtime.Game
{
    public sealed class CommonLoading : ILoading
    {
        private GameObject _object;

        public static bool UsingInternalLoadingPrefab = true;
        public CommonLoading()
        {
            if (UsingInternalLoadingPrefab)
            {
                _object = GameObject.Instantiate(Resources.Load<GameObject>("Loading/Loading"));
            }
            else
            {
                AssetHandle handle = ResourcesManager.Instance.Load("common/prefab/Loading");
                _object = handle.CreateGameObject();
                _object.AddComponent<AssetBundleBehaviour>().AddAssetHandle(handle);
            }
            version = "version:" + Application.version;
        }
        public GameObject gameObject
        {
            get
            {
                return _object;
            }
        }
        private string _text;
        private string _version;
        public string text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                if (_object != null)
                {
                    Text com = _object.transform.Find("info")?.GetComponent<Text>();
                    if (com != null)
                    {
                        com.text = _text;
                    }
                }
            }
        }

        public string version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
                if (_object != null)
                {
                    Text com = _object.transform.Find("version")?.GetComponent<Text>();
                    if (com != null)
                    {
                        com.text = _version;
                    }
                }
            }
        }

        public void Dispose()
        {

        }

        public void SetLoadingBackground(string textureName)
        {
            AssetHandle handle = ResourcesManager.Instance.Load(textureName);
            if (handle == null)
            {
                return;
            }
            Texture2D texture = handle.LoadAsset<Texture2D>();
            if (texture == null)
            {
                return;
            }
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2, 100);
            this._object.GetComponent<Image>().sprite = sprite;
            ResourcesManager.Instance.SetAssetBundleBehaviour(_object, handle);
        }
    }
}