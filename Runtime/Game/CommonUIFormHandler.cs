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

        private Dictionary<string, List<RectTransform>> clones;
        private Dictionary<string, Queue<RectTransform>> cloneCaches;

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
        /// 释放UI管理器
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
        /// 初始化
        /// </summary>
        public void Start()
        {
            childs = new Dictionary<string, RectTransform>();
            clones = new Dictionary<string, List<RectTransform>>();
            cloneCaches = new Dictionary<string, Queue<RectTransform>>();
            RectTransform[] transforms = this.gameObject.transform.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                childs.Add(transforms[i].name, transforms[i]);
            }

            UnityEngine.UI.Button[] buttons = this.gameObject.transform.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            foreach (var item in buttons)
            {
                GameObject eventObject = item.gameObject;
                item.onClick.AddListener(() =>
                {
                    OnNotify(item.gameObject.name, eventObject, null);
                });
            }

            UnityEngine.UI.InputField[] inputs = this.gameObject.transform.GetComponentsInChildren<UnityEngine.UI.InputField>(true);
            foreach (var item in inputs)
            {
                GameObject eventObject = item.gameObject;
                item.onEndEdit.AddListener((args) =>
                {
                    OnNotify(item.gameObject.name, eventObject, args);
                });
            }
            adapter.Start();
        }
        /// <summary>
        /// 克隆对象
        /// </summary>
        /// <param name="name"></param>
        public void Instantiate(string name)
        {
            Instantiate(name, null);
        }

        /// <summary>
        /// 克隆对象
        /// </summary>
        /// <param name="name"></param>
        public void Instantiate(string name, GameFrameworkAction<GameObject> OnClick)
        {
            RectTransform transform = null;
            if (cloneCaches.TryGetValue(name, out Queue<RectTransform> queue))
            {
                if (queue.Count > 0)
                {
                    transform = queue.Dequeue();
                }
                else
                {
                    GameObject template = GetChild(name);
                    if (template == null)
                    {
                        Debug.LogError(template);
                        return;
                    }
                    transform = GameObject.Instantiate<GameObject>(template).GetComponent<RectTransform>();
                    transform.name = template.name;
                }
            }
            if (!clones.TryGetValue(name, out List<RectTransform> temps))
            {
                clones.Add(name, temps = new List<RectTransform>());
            }
            
            if (OnClick == null)
            {
                return;
            }
            UnityEngine.UI.Button buttons = transform.GetComponent<UnityEngine.UI.Button>();
            buttons.onClick.AddListener(() =>
            {
                OnClick(transform.gameObject);
            });
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
            adapter.EventHandle(eventId, sender, args);
        }

        /// <summary>
        /// 获取子节点
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
    }
}