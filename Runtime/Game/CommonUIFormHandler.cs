using System.Linq;
using System;
using UnityEngine;
using XLua;
using System.Collections.Generic;
using DG.Tweening;
using GameFramework.Runtime.Assets;
namespace GameFramework.Runtime.Game
{
    public enum TriggerType
    {
        Start,
        Enable,
        Disable,
        Time,
        Click,
    }
    public sealed class CommonUIFormHandler : IUIHandler
    {

        class LuaAdapter : GObject
        {
            private LuaTable table;
            private CommonUIFormHandler handler;
            private LuaFunction _start;
            private LuaFunction _dispose;
            private LuaFunction _disable;
            private LuaFunction _enable;
            private LuaFunction _eventHandle;
            private LuaFunction _fixedUpdate;

            public int layer { get; }


            public LuaAdapter(LuaTable table, CommonUIFormHandler handler)
            {
                if (table == null)
                {
                    return;
                }
                this.table = table;
                this.handler = handler;
                this.layer = table.Get<int>("layer");
                _start = table.Get<LuaFunction>("start");
                _dispose = table.Get<LuaFunction>("dispose");
                _disable = table.Get<LuaFunction>("disable");
                _enable = table.Get<LuaFunction>("enable");
                _fixedUpdate = table.Get<LuaFunction>("update");
                _eventHandle = table.Get<LuaFunction>("event");
            }

            public void Start()
            {
                if (_start == null)
                {
                    return;
                }
                _start.Call(table, handler);
            }

            public void Disable()
            {
                if (_disable == null)
                {
                    return;
                }
                _disable.Call(table, handler);
            }

            public void Enable()
            {
                if (_enable == null)
                {
                    return;
                }
                _enable.Call(table, handler);
            }

            public void EventHandle(string eventId, GameObject sender, object args)
            {
                if (_eventHandle == null)
                {
                    return;
                }
                _eventHandle.Call(table, handler, eventId, sender, args);
            }

            public void Dispose()
            {
                if (_dispose == null)
                {
                    return;
                }
                _dispose.Call(table, handler);
            }
        }
        public int layer
        {
            get
            {
                if (adapter == null)
                {
                    return -1;
                }
                return adapter.layer;
            }
        }

        public string name
        {
            get;
        }

        public GameObject gameObject
        {
            get;
        }

        public Camera UICamera
        {
            get;
            private set;
        }
        public Canvas canvas
        {
            get;
            set;
        }

        private IUIManager manager;
        private LuaAdapter adapter;


        private Dictionary<string, RectTransform> childs;

        private Dictionary<string, List<RectTransform>> clones = new Dictionary<string, List<RectTransform>>();
        private Dictionary<string, Queue<RectTransform>> cloneCaches = new Dictionary<string, Queue<RectTransform>>();

        /// <summary>
        /// UI管道
        /// </summary>
        /// <param name="uIManager"></param>
        /// <param name="table"></param>
        public CommonUIFormHandler(IUIManager uIManager, string name, GameObject gameObject, LuaTable table)
        {
            this.manager = uIManager;
            this.gameObject = gameObject;
            this.name = name;
            this.UICamera = uIManager.UICamera;
            adapter = new LuaAdapter(table, this);
        }

        /// <summary>
        /// 释放UI管理�?
        /// </summary>
        public void Dispose()
        {
            adapter.Dispose();
            adapter = null;
            GameObject.DestroyImmediate(this.gameObject);
        }

        /// <summary>
        /// 隐藏UI
        /// </summary>
        public void OnDisable()
        {
            this.gameObject.SetActive(false);
            adapter.Disable();
        }

        /// <summary>
        /// 显示UI
        /// </summary>
        public void OnEnable()
        {
            this.gameObject.SetActive(true);
            adapter.Enable();
        }

        /// <summary>
        /// 初�?�化
        /// </summary>
        public void Start()
        {
            childs = new Dictionary<string, RectTransform>();
            InitUnityEvent(this.gameObject);
            adapter.Start();
        }

        private void InitUnityEvent(GameObject gameObject)
        {
            RectTransform[] transforms = gameObject.transform.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (childs.ContainsKey(transforms[i].name))
                {
                    continue;
                }
                childs.Add(transforms[i].name, transforms[i]);
            }
            UnityEngine.UI.Button[] buttons = gameObject.transform.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            for (var i = 0; i < buttons.Length; i++)
            {
                GameObject eventObject = buttons[i].gameObject;
                buttons[i].onClick.AddListener(() =>
                {
                    OnNotify(eventObject.name, eventObject, null);
                });
            }

            UnityEngine.UI.InputField[] inputs = gameObject.transform.GetComponentsInChildren<UnityEngine.UI.InputField>(true);
            for (var i = 0; i < inputs.Length; i++)
            {
                GameObject eventObject = inputs[i].gameObject;
                inputs[i].onSubmit.AddListener((args) =>
                {
                    OnNotify(eventObject.name, eventObject, args);
                });
            }

            TMPro.TMP_InputField[] tmp_input = gameObject.transform.GetComponentsInChildren<TMPro.TMP_InputField>(true);
            for (var i = 0; i < tmp_input.Length; i++)
            {
                GameObject eventObject = tmp_input[i].gameObject;
                tmp_input[i].onEndEdit.AddListener((args) =>
                {
                    OnNotify(eventObject.name, eventObject, args);
                });
            }

            UnityEngine.UI.Toggle[] toggles = gameObject.transform.GetComponentsInChildren<UnityEngine.UI.Toggle>(true);
            for (var i = 0; i < toggles.Length; i++)
            {
                GameObject eventObject = toggles[i].gameObject;
                toggles[i].onValueChanged.AddListener((args) =>
                {
                    if (!args)
                    {
                        return;
                    }
                    OnNotify(eventObject.name, eventObject, args);
                });
            }

            UnityEngine.UI.Dropdown[] dropdowns = gameObject.transform.GetComponentsInChildren<UnityEngine.UI.Dropdown>(true);
            for (var i = 0; i < dropdowns.Length; i++)
            {
                GameObject eventObject = dropdowns[i].gameObject;
                dropdowns[i].onValueChanged.AddListener(args =>
                {
                    OnNotify(eventObject.name, eventObject, args);
                });
            }

            TMPro.TMP_Dropdown[] tmp_dropdowns = gameObject.transform.GetComponentsInChildren<TMPro.TMP_Dropdown>(true);
            for (var i = 0; i < tmp_dropdowns.Length; i++)
            {
                GameObject eventObject = tmp_dropdowns[i].gameObject;
                tmp_dropdowns[i].onValueChanged.AddListener(args =>
                {
                    OnNotify(eventObject.name, eventObject, args);
                });
            }
            UnityEngine.UI.Slider[] sliders = gameObject.transform.GetComponentsInChildren<UnityEngine.UI.Slider>(true);
            for (var i = 0; i < sliders.Length; i++)
            {
                GameObject eventObject = sliders[i].gameObject;
                sliders[i].onValueChanged.AddListener(args =>
                {
                    OnNotify(eventObject.name, eventObject, args);
                });
            }
        }

        /// <summary>
        /// 加载子级UI界面
        /// </summary>
        /// <param name="name"></param>
        /// <param name="childName"></param>
        public GameObject GenerateSubUIHandler(string name, string path, string childName)
        {
            AssetHandle handle = ResourcesManager.Instance.Load(path);
            if (handle == null)
            {
                return default;
            }
            GameObject gameObject = handle.CreateGameObject(GetChild(name).transform, childName);
            if (gameObject == null)
            {
                Debug.Log("创建子级UI出错");
                return default;
            }
            gameObject.AddComponent<AssetBundleBehaviour>().AddAssetHandle(handle);
            gameObject.SetActive(true);
            InitUnityEvent(gameObject);
            return gameObject;
        }

        public GameObject GenerateSubUIHandler(string parentName, string path, string uiName, LuaTable luaScript)
        {
            CommonUIFormHandler handler = (CommonUIFormHandler)manager.OpenUI(path, uiName, luaScript);
            if (handler == null)
            {
                Debug.LogError("load sub ui error");
                return default;
            }
            handler.gameObject.SetParent(GetChild(parentName), Vector3.zero, Vector3.zero, Vector3.one);
            return gameObject;
        }

        /// <summary>
        /// 克隆对象
        /// </summary>
        /// <param name="name"></param>
        public GameObject Instantiate(string name)
        {
            return Instantiate(name, null);
        }

        /// <summary>
        /// 克隆对象
        /// </summary>
        /// <param name="name"></param>
        public GameObject Instantiate(string name, GameFrameworkAction<GameObject> OnClick)
        {
            RectTransform transform = null;
            if (!cloneCaches.TryGetValue(name, out Queue<RectTransform> queue))
            {
                cloneCaches.Add(name, queue = new Queue<RectTransform>());
            }
            if (queue.Count > 0)
            {
                transform = queue.Dequeue();
            }
            else
            {
                GameObject template = GetChild(name);
                if (template == null)
                {
                    return default;
                }
                transform = GameObject.Instantiate<GameObject>(template, template.transform.parent).GetComponent<RectTransform>();
                transform.name = template.name;
                transform.gameObject.SetParent(template.transform.parent.gameObject);
            }

            if (!clones.TryGetValue(name, out List<RectTransform> temps))
            {
                clones.Add(name, temps = new List<RectTransform>());
            }

            if (OnClick != null)
            {
                UnityEngine.UI.Button buttons = transform.GetComponent<UnityEngine.UI.Button>();
                if (buttons != null)
                {
                    buttons.onClick.AddListener(() => { OnClick(transform.gameObject); });
                }
                else
                {
                    UnityEngine.UI.Toggle Toggles = transform.GetComponent<UnityEngine.UI.Toggle>();
                    if (Toggles != null)
                    {
                        Toggles.onValueChanged.AddListener((state) => { if (state) { OnClick(transform.gameObject); } });
                    }
                }
            }
            temps.Add(transform);
            transform.gameObject.SetActive(true);
            transform.SetAsLastSibling();
            return transform.gameObject;
        }

        public void ClearClone(bool isCache = true)
        {
            string[] keys = clones.Keys.ToArray();
            for (var i = 0; i < keys.Length; i++)
            {
                ClearClone(keys[i], isCache);
            }
        }

        /// <summary>
        /// 清理克隆物体
        /// </summary>
        /// <param name="isCache"></param>
        public void ClearClone(string name, bool isCache = true)
        {
            if (!clones.TryGetValue(name, out List<RectTransform> temps))
            {
                return;
            }
            if (!cloneCaches.TryGetValue(name, out Queue<RectTransform> cache))
            {
                if (isCache)
                {
                    cloneCaches.Add(name, cache = new Queue<RectTransform>());
                }
            }
            for (var i = 0; i < temps.Count; i++)
            {
                if (isCache)
                {
                    temps[i].gameObject.SetActive(false);
                    cache.Enqueue(temps[i]);
                }
                else
                {
                    GameObject.DestroyImmediate(temps[i].gameObject);
                }
            }
            temps.Clear();
            clones.Remove(name);
        }

        /// <summary>
        /// 时间通知
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnNotify(string eventId, GameObject sender, object args)
        {
            Debug.Log("notify ui event:" + eventId);
            adapter.EventHandle(eventId, sender, args);
        }

        /// <summary>
        /// 获取子节�?
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GameObject GetChild(string name)
        {
            if (!childs.TryGetValue(name, out RectTransform transform))
            {
                return default;
            }
            return transform.gameObject;
        }

        /// <summary>
        /// 获取精灵图片
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Sprite GetSprite(string name)
        {
            if (!childs.TryGetValue(name, out RectTransform transform))
            {
                return default;
            }
            UnityEngine.UI.Image image = transform.GetComponent<UnityEngine.UI.Image>();
            if (image == null)
            {
                return default;
            }
            return image.sprite;
        }

        /// <summary>
        /// 设置精灵图片
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sprite"></param>
        public void SetSprite(string name, Sprite sprite)
        {
            if (!childs.TryGetValue(name, out RectTransform transform))
            {
                return;
            }
            UnityEngine.UI.Image image = transform.GetComponent<UnityEngine.UI.Image>();
            if (image == null)
            {
                return;
            }
            image.sprite = sprite;
        }

        /// <summary>
        /// 设置到指定的层级
        /// </summary>
        /// <param name="layer"></param>
        public void ToLayer(int layer)
        {
            manager.ToLayer(this, layer);
        }

        /// <summary>
        /// 设置精灵图片
        /// </summary>
        /// <param name="name"></param>
        /// <param name="spriteName"></param>
        public void SetSprite(string name, string spriteName)
        {
            AssetHandle handle = ResourcesManager.Instance.Load(spriteName);
            if (handle == null)
            {
                return;
            }

            Sprite sprite = (Sprite)handle.LoadAsset(typeof(Sprite));
            SetSprite(name, sprite);
        }


        /// <summary>
        /// 设置精灵图片
        /// </summary>
        /// <param name="name"></param>
        /// <param name="spriteName"></param>
        public void SetSprite(GameObject obj, string spriteName)
        {
            UnityEngine.UI.Image image = obj.GetComponent<UnityEngine.UI.Image>();
            if (image == null) return;
            AssetHandle handle = ResourcesManager.Instance.Load(spriteName);
            if (handle == null) return;
            Sprite sprite = (Sprite)handle.LoadAsset(typeof(Sprite));
            image.sprite = sprite;
        }

        /// <summary>
        /// 获取文本
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetText(string name)
        {
            if (!childs.TryGetValue(name, out RectTransform transform))
            {
                return default;
            }
            UnityEngine.UI.Text text = transform.GetComponent<UnityEngine.UI.Text>();
            if (text == null)
            {
                return default;
            }
            return text.text;
        }

        /// <summary>
        /// 设置文本
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        public void SetText(string name, string info)
        {
            if (!childs.TryGetValue(name, out RectTransform transform))
            {
                return;
            }
            UnityEngine.UI.Text text = transform.GetComponent<UnityEngine.UI.Text>();
            if (text == null)
            {
                return;
            }
            text.text = info;
        }

        /// <summary>
        /// 设置文本
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        public void SetText(GameObject obj, string info)
        {
            if (obj == null) return;
            UnityEngine.UI.Text text = obj.GetComponent<UnityEngine.UI.Text>();
            if (text == null) return;
            text.text = info;
        }

        public void SetText(Transform tran, string info)
        {
            SetText(tran.gameObject, info);
        }

        /// <summary>
        /// 设置指定gameObject的按�?点击事件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="OnClick"></param>
        public void SetButtonOnClick(GameObject obj, GameFrameworkAction OnClick)
        {
            UnityEngine.UI.Button buttons = obj.GetComponent<UnityEngine.UI.Button>();
            if (buttons)
            {
                buttons.onClick.AddListener(() =>
                {
                    OnClick();
                });
            }
        }

        /// <summary>
        /// 设置指定gameObject的isActive
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="OnClick"></param>
        public void SetActive(GameObject obj, bool isActive)
        {
            if (obj) obj.SetActive(isActive);
        }

        public void SetActive(Transform tran, bool isActive)
        {
            if (tran) SetActive(tran.gameObject, isActive);
        }
    }
}