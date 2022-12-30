using UnityEngine;
using GameFramework.Runtime.Assets;
using UnityEngine.UI;

namespace GameFramework.Runtime.Game
{
    public sealed class CommonMessageBox : IMessageBox
    {
        private GameObject _object;
        public static bool UsingInternalMessageBoxPrefab = true;

        public void SetTilet(string tilet)
        {
            if (_object != null)
            {
                Text com = _object.transform.Find("Tilet")?.GetComponent<Text>();
                if (com != null)
                {
                    com.text = tilet;
                }
            }
        }


        public void SetMessage(string message)
        {
            if (_object != null)
            {
                Text com = _object.transform.Find("Text")?.GetComponent<Text>();
                if (com != null)
                {
                    com.text = message;
                }
            }
        }


        public void Dispose()
        {
            GameObject.DestroyImmediate(_object);
        }

        public void Show(Transform parent, string tilet, string message, GameFrameworkAction ok, GameFrameworkAction cancel)
        {
            if (UsingInternalMessageBoxPrefab)
            {
                _object = GameObject.Instantiate(Resources.Load<GameObject>("Loading/MessageBox"));
            }
            else
            {
                AssetHandle handle = ResourcesManager.Instance.Load("common/prefab/MessageBox");
                _object = handle.CreateGameObject();
                _object.AddComponent<AssetBundleBehaviour>().AddAssetHandle(handle);
            }
            SetTilet(tilet);
            SetMessage(message);
            _object.transform.Find("Cancel").gameObject.SetActive(cancel != null);
            Button cancelBtn = _object.transform.Find("Cancel")?.GetComponent<Button>();
            Button entryBtn = _object.transform.Find("Entry")?.GetComponent<Button>();
            entryBtn.onClick.AddListener(() => { ok(); });
            cancelBtn.onClick.AddListener(() => { cancel(); });
            entryBtn.transform.localPosition = cancel == null ? new Vector3(0, -245, 0) : new Vector3(219, -245, 0);
            _object.SetParent(parent, Vector3.zero, Vector3.zero, Vector3.one);
        }
    }
}